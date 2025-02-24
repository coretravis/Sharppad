using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Azure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SharpPad.Server.Services.Execution;
using SharpPad.Server.Services.Execution.Compiler;
using SharpPad.Server.Services.Execution.Storage;
using SharpPad.Server.Services.Nugets;
using SharpPad.Shared.Models.Compiler;
using SharpPad.Shared.Models.Nuget;

namespace SharpPad.Server.Services.Streaming;

/// <summary> 
/// A streaming code execution service that streams output via SignalR and waits for input (Interactive Mode). 
/// </summary> 

public class StreamingCodeExecutionService(
    INugetPackageService nugetPackageService,
    ICompilerVersionService compilerVersionService,
    IFileService fileStorageService,
    ILogger<CodeAnalysisProgress> logger,
    IHubContext<CodeExecutionHub> hubContext) : IStreamingCodeExecutionService
{
    private readonly INugetPackageService _nugetPackageService = nugetPackageService ?? throw new ArgumentNullException(nameof(nugetPackageService));
    private readonly ICompilerVersionService _compilerVersionService = compilerVersionService ?? throw new ArgumentNullException(nameof(compilerVersionService));
    private readonly IFileService _fileService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
    private readonly ILogger<CodeAnalysisProgress> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHubContext<CodeExecutionHub> _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

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
            
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("ReceiveOutput", new { type = "error", content = "No code provided." });
            return;
        }

        code = CodePreprocessor.PreprocessUserCode(code);

        _logger.LogInformation("Preprocessed code: {Code}", code);

        // Create a new response
        var response = new CodeExecutionResponse
        {
            CompilerVersion = compilerVersion,
            Outputs = new List<ExecutionOutput>(),
            Files = new List<string>(),
            Success = false,
            Metrics = new ExecutionMetrics()
        };


        // Create custom streaming writer/reader
        var streamingWriter = new StreamingTextWriter(_hubContext, sessionId, interactive ?? false);
        var streamingErrorWriter = new StreamingErrorTextWriter(_hubContext, sessionId, interactive ?? false);
        var streamingReader = new StreamingTextReader(_hubContext, sessionId);

        // Register the reader for this session.
        StreamingTextReaderRegistry.Register(sessionId, streamingReader);

        // Swap out Console streams with streaming versions.
        var originalOut = Console.Out;
        var originalIn = Console.In;
       // var originalError = Console.Error;

        Console.SetOut(streamingWriter);
        Console.SetIn(streamingReader);
       // Console.SetError(streamingErrorWriter);

        // Register an empty execution response in FileService
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

            
            // Choose full program execution if code has a Main method; otherwise, execute as a script.
            if (code.Contains("class Program") && code.Contains("Main("))
            {
                _logger.LogInformation("Executing full program");
                await ExecuteFullProgram(code, allReferences, compilerVersion);
            }
            else
            {
                _logger.LogInformation("Executing script");
                await ExecuteScript(code, allReferences, compilerVersion);
            }

            // Record metrics
            metrics.CompilationTime = DateTime.UtcNow - startTime;
            metrics.OutputCount = 0; // todoo: add app count
            metrics.PeakMemoryUsage = Process.GetCurrentProcess().PeakWorkingSet64;


            response.Metrics = metrics;
            // Set the code execution to successful
            response.Success = true;

            _logger.LogInformation("Finished executing code in session {SessionId}", sessionId);
            // Signal execution complete.
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("ExecutionComplete", response);

            _logger.LogInformation("Execution complete in session {SessionId}", sessionId);
        }
        catch (CompilationErrorException cex)
        {
            _logger.LogError(cex, "Compilation error in session {SessionId}", sessionId);
            var errorContent = string.Join(Environment.NewLine, cex.Diagnostics.Select(diag => diag.ToString()));
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("ReceiveOutput", new { type = OutputType.CompilationError, content = errorContent });

            // Signal execution complete.
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("ExecutionComplete", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Runtime error in session {SessionId}", sessionId);
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("ReceiveOutput", new { type = OutputType.RuntimeError, content = ex.ToString() });
            // Signal execution complete.
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("ExecutionComplete", response);
        }
        finally
        {
            // Restore original Console streams.
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
            
         //   Console.SetError(originalError);

            // Clean up the reader registration.
            StreamingTextReaderRegistry.Unregister(sessionId);
        }
    }

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
            File = _fileService
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