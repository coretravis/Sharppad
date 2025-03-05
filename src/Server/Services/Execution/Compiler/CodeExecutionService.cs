using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Scripting;
using SharpPad.Server.Services.Execution.FileSystem;
using SharpPad.Server.Services.Nugets;
using SharpPad.Shared.Models.Compiler;
using SharpPad.Shared.Models.Nuget;

namespace SharpPad.Server.Services.Execution.Compiler;

public class CodeExecutionService(
    INugetPackageService nugetPackageService,
    ICompilerVersionService compilerVersionService,
    IFileService fileStorageService, ILogger<CodeAnalysisProgress> logger) : ICodeExecutionService
{
    private readonly INugetPackageService _nugetPackageService = nugetPackageService 
        ?? throw new ArgumentNullException(nameof(nugetPackageService));
    private readonly ICompilerVersionService _compilerVersionService = compilerVersionService 
        ?? throw new ArgumentNullException(nameof(compilerVersionService));
    private readonly IFileService fileService = fileStorageService 
        ?? throw new ArgumentNullException(nameof(fileStorageService));
    private readonly ILogger<CodeAnalysisProgress> _logger = logger 
        ?? throw new ArgumentNullException(nameof(logger));

    public async Task<CodeExecutionResponse> ExecuteCodeAsync(string code, string compilerVersion, List<NugetPackage> nugetPackages)
    {
        _logger.LogInformation("Executing code: {Code}", code);
        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogInformation("No code provided.");
            return new CodeExecutionResponse
            {
                Success = false,
                Outputs = new List<ExecutionOutput>
                {
                    new ExecutionOutput { Type = OutputType.Error, Content = "No code provided.", Timestamp = DateTime.UtcNow }
                }
            };
        }

        // Create a new response
        var response = new CodeExecutionResponse
        {
            CompilerVersion = compilerVersion,
            Outputs = new List<ExecutionOutput>(),
            Files = new List<string>()
        };

        // Register the current response so that any file saved via FileStorageService is recorded.
        fileService.SetExecutionResponse(response);

        var metrics = new ExecutionMetrics();
        var startTime = DateTime.UtcNow;


       
        var originalOut = Console.Out;
        var originalInput = Console.In;
        try
        {

            // Redirect console output
            var outputInterceptor = new OutputInterceptor(response.Outputs);
            var inputInterceptor = new InputInterceptor(response.Outputs);

          
            Console.SetOut(outputInterceptor);
            Console.SetIn(inputInterceptor);

            _logger.LogInformation("Redirected console output and input.");
            // Get all the necessary references
            var nugetReferences = await _nugetPackageService.GetMetadataReferencesAsync(nugetPackages, compilerVersion);
            var defaultReferences = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();
            var allReferences = defaultReferences.Concat(nugetReferences).ToList();

            _logger.LogInformation("Got all references.");
            // Execute as full program or as script
            if (code.Contains("class Program") && code.Contains("Main("))
            {
                _logger.LogInformation("Executing as full program");
                await ExecuteFullProgram(code, allReferences, compilerVersion, response);
            }
            else
            {
                _logger.LogInformation("Executing as script");
                await ExecuteScript(code, allReferences, compilerVersion, response);
            }

            // Record metrics
            metrics.CompilationTime = DateTime.UtcNow - startTime;
            metrics.OutputCount = response.Outputs.Count;
            metrics.PeakMemoryUsage = Process.GetCurrentProcess().PeakWorkingSet64;
            response.Success = true;
            response.Metrics = metrics;
        }
        catch(InteractiveModeRequiredException)
        {
            _logger.LogInformation("Interactive mode required.");
            response.Success = false;
        }
        catch (CompilationErrorException cex)
        {
            _logger.LogInformation("Compilation error.");
            response.Success = false;
            response.Outputs.Add(new ExecutionOutput
            {
                Type = OutputType.CompilationError,
                Content = string.Join(Environment.NewLine, cex.Diagnostics.Select(diag => diag.ToString())),
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>
                {
                    { "ErrorCount", cex.Diagnostics.Count().ToString() },
                    { "Severity", "Error" }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Runtime error.");
            response.Success = false;
            response.Outputs.Add(new ExecutionOutput
            {
                Type = OutputType.RuntimeError,
                Content = ex.ToString(),
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, string>()
            });
        }
        finally
        {
            _logger.LogInformation("Execution completed.");
            // Restore original Console streams.
            Console.SetOut(originalOut);
            Console.SetIn(originalInput);

        }

        return response;
    }

    private Task ExecuteFullProgram(string code, List<MetadataReference> references, string compilerVersion, CodeExecutionResponse response)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
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
        _logger.LogInformation("Compilation result: {EmitResult}", emitResult);
        if (!emitResult.Success)
        {
            _logger.LogInformation("Compilation failed.");
            throw new CompilationErrorException("Compilation failed",
                emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToImmutableArray());
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        var programType = assembly.GetType("Program")
            ?? throw new Exception("Could not find a 'Program' class.");

        var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static)
            ?? throw new Exception("Could not find a public static 'Main' method.");

        mainMethod.Invoke(null, null);

        return Task.CompletedTask;
    }



    private async Task ExecuteScript(string code, List<MetadataReference> references, string compilerVersion, CodeExecutionResponse response)
    {
        var scriptOptions = ScriptOptions.Default
            .WithReferences(references)
            .WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text.Json");

        scriptOptions = _compilerVersionService.AdjustScriptOptions(scriptOptions, compilerVersion);

        // Pass the file storage service into the script's global context.
        var globals = new ScriptGlobals
        {
            File = fileService
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