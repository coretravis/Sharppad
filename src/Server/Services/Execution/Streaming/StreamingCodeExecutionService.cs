using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SharpPad.Server.Services.Execution.Analysis;
using SharpPad.Server.Services.Execution.Compiler;
using SharpPad.Server.Services.Execution.FileSystem;
using SharpPad.Server.Services.Execution.Preprocessors;
using SharpPad.Server.Services.Nugets;
using SharpPad.Shared.Models.Compiler;
using SharpPad.Shared.Models.Nuget;

namespace SharpPad.Server.Services.Execution.Streaming;

public class StreamingCodeExecutionService(
    INugetPackageService nugetPackageService,
    ICompilerVersionService compilerVersionService,
    IStaticAnalysisService analysisService,
    IFileService fileStorageService,
    IDirectoryService directoryService,
    IPathService pathService,
    ILogger<CodeAnalysisProgress> logger,
    IHubContext<CodeExecutionHub> hubContext) : IStreamingCodeExecutionService
{
    private readonly INugetPackageService _nugetPackageService = nugetPackageService ?? throw new ArgumentNullException(nameof(nugetPackageService));
    private readonly IStaticAnalysisService _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
    private readonly ICompilerVersionService _compilerVersionService = compilerVersionService ?? throw new ArgumentNullException(nameof(compilerVersionService));
    private readonly IFileService _fileService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
    private readonly IDirectoryService _directoryService = directoryService ?? throw new ArgumentNullException(nameof(directoryService));
    private readonly IPathService _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
    private readonly ILogger<CodeAnalysisProgress> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHubContext<CodeExecutionHub> _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

    private const string OutputMethod = "ReceiveOutput";
    private const string ExecutionCompleteMethod = "ExecutionComplete";

    /// <summary>
    /// Executes code in a streaming session. Outputs are sent immediately to the client via SignalR,
    /// and Console.ReadLine calls will wait until input is provided (via the SignalR hub).
    /// </summary>
    /// <param name="code">The code to execute.</param>
    /// <param name="compilerVersion">The target compiler version.</param>
    /// <param name="nugetPackages">The list of NuGet packages.</param>
    /// <param name="sessionId">The unique session identifier (SignalR group name).</param>
    public async Task ExecuteCodeStreamingAsync(string code, string compilerVersion, List<NugetPackage> nugetPackages, string sessionId, bool? interactive = false)
    {
        _logger.LogInformation("Executing code in session {SessionId}", sessionId);
        if (string.IsNullOrWhiteSpace(code))
        {
            await SendOutputAsync(sessionId, OutputType.CompilationError, "No code provided");
            await SendExecutionCompleteAsync(sessionId, new CodeExecutionResponse
            {
                CompilerVersion = compilerVersion,
                Success = false
            });
            return;
        }

        // Strip program wrapper and preprocess code.
        code = CodePreprocessor.StripProgramWrapper(code);
        code = CodePreprocessor.PreprocessUserCode(code);

        // Run static analysis on the code to check for unsafe code.
        var analysisResult = _analysisService.AnalyzeCode(code);

        if (!analysisResult.IsSafe)
        {
            // unsafe code detected
            var errorContent = string.Join(Environment.NewLine, analysisResult.Warnings);
            await SendOutputAsync(sessionId, OutputType.CompilationError, errorContent);
            await SendExecutionCompleteAsync(sessionId, new CodeExecutionResponse
            {
                CompilerVersion = compilerVersion,
                Success = false
            });
            return;
        }

        _logger.LogInformation("Preprocessed code: {Code}", code);

        // Create a new response.
        var response = new CodeExecutionResponse
        {
            CompilerVersion = compilerVersion,
            Outputs = new List<ExecutionOutput>(),
            Files = new List<string>(),
            Success = false,
            Metrics = new ExecutionMetrics()
        };

        // Create custom streaming writers/readers.
        var streamingWriter = new StreamingTextWriter(_hubContext, sessionId, interactive ?? false);
        var streamingErrorWriter = new StreamingErrorTextWriter(_hubContext, sessionId, interactive ?? false);
        var streamingReader = new StreamingTextReader(_hubContext, sessionId);

        // Register the reader for this session.
        StreamingTextReaderRegistry.Register(sessionId, streamingReader);

        // Swap out Console streams with streaming versions.
        var originalOut = Console.Out;
        var originalIn = Console.In;
        var originalError = Console.Error;
        Console.SetOut(streamingWriter);
        Console.SetIn(streamingReader);
        Console.SetError(streamingErrorWriter);

        // Register the response in FileService.
        _fileService.SetExecutionResponse(response);

        var metrics = new ExecutionMetrics();
        var startTime = DateTime.UtcNow;

        try
        {
            // Prepare metadata references by combining default assemblies with NuGet packages.
            var nugetReferences = await _nugetPackageService.GetMetadataReferencesAsync(nugetPackages, compilerVersion);
            var defaultReferences = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();
            var allReferences = defaultReferences.Concat(nugetReferences).ToList();

            // Choose script execution, no need to check for programs any longer since we convert programs to scripts.
            await ExecuteScript(code, allReferences, compilerVersion);

            // Record metrics.
            metrics.CompilationTime = DateTime.UtcNow - startTime;
            metrics.OutputCount = 0; // TODO: add actual output count, this might be removed because we are now streaming
            metrics.PeakMemoryUsage = Process.GetCurrentProcess().PeakWorkingSet64;
            response.Metrics = metrics;
            response.Success = true;

            _logger.LogInformation("Finished executing code in session {SessionId}", sessionId);
            await SendExecutionCompleteAsync(sessionId, response);
            _logger.LogInformation("Execution complete in session {SessionId}", sessionId);
        }
        catch (CompilationErrorException cex)
        {
            _logger.LogError(cex, "Compilation error in session {SessionId}", sessionId);
            var errorContent = string.Join(Environment.NewLine, cex.Diagnostics.Select(diag => diag.ToString()));
            await SendOutputAsync(sessionId, OutputType.CompilationError, errorContent);
            await SendExecutionCompleteAsync(sessionId, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Runtime error in session {SessionId}", sessionId);
            await SendOutputAsync(sessionId, OutputType.RuntimeError, ex.ToString());
            await SendExecutionCompleteAsync(sessionId, response);
        }
        finally
        {
            // Restore original Console streams.
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            Console.SetError(originalError);

            // Clean up the reader registration.
            StreamingTextReaderRegistry.Unregister(sessionId);
        }
    }

    /// <summary>
    /// Helper method to send output messages via SignalR.
    /// </summary>
    private async Task SendOutputAsync(string sessionId, OutputType type, string content)
    {
        await _hubContext.Clients.Group(sessionId)
            .SendAsync(OutputMethod, new { type, content });
    }

    /// <summary>
    /// Helper method to signal execution completion via SignalR.
    /// </summary>
    private async Task SendExecutionCompleteAsync(string sessionId, CodeExecutionResponse response)
    {
        await _hubContext.Clients.Group(sessionId)
            .SendAsync(ExecutionCompleteMethod, response);
    }

    [Obsolete("Code will be converted to scripts so there is no need to run full programs anymore")]
    private async Task ExecuteFullProgram(string code, List<MetadataReference> references, string compilerVersion)
    {
        var languageVersion = _compilerVersionService.GetLanguageVersion(compilerVersion);
        var parseOptions = new CSharpParseOptions(languageVersion: languageVersion);
        var syntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions);

        var assemblyName = Path.GetRandomFileName();

        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
        compilationOptions = _compilerVersionService.AdjustCompilationOptions(compilationOptions, compilerVersion);

        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            compilationOptions
        );

        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);
        if (!emitResult.Success)
        {
            throw new CompilationErrorException("Compilation failed",
                emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToImmutableArray());
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        var programType = assembly.GetType("Program")
            ?? throw new Exception("Could not find a 'Program' class.");
        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static)
            ?? throw new Exception("Could not find a public static 'Main' method.");

        var result = mainMethod.Invoke(null, null);
        if (result is Task task)
        {
            await task;
        }
    }

    private async Task ExecuteScript(string code, List<MetadataReference> references, string compilerVersion)
    {
        var scriptOptions = ScriptOptions.Default
            .WithReferences(references)
            .WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text.Json");
        scriptOptions = _compilerVersionService.AdjustScriptOptions(scriptOptions, compilerVersion);

        var globals = new ScriptGlobals
        {
            File = _fileService,
            Directory = _directoryService,
            Path = _pathService
        };

        var result = await CSharpScript.EvaluateAsync(code, scriptOptions, globals: globals);
        if (result != null)
        {
            if (result is IEnumerable<object> collection)
            {
                Console.WriteLine(JsonSerializer.Serialize(collection));
            }
            else if (result.GetType().IsClass && result.GetType() != typeof(string))
            {
                Console.WriteLine(JsonSerializer.Serialize(result));
            }
            else
            {
                Console.WriteLine(result.ToString());
            }
        }
    }
}
