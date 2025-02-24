using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Formatting;
using SharpPad.Shared.Models.Compiler;

namespace SharpPad.Server.Services.Execution.Compiler;




public class RoslynService : IRoslynService
{
    private const string Name = "TRUSTED_PLATFORM_ASSEMBLIES";
    private AdhocWorkspace _workspace;
    private Project _project;

    public RoslynService()
    {
        // Create a MEF composition container
        var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);

        // Create the workspace with the MEF container
        _workspace = new AdhocWorkspace(host);

        var projectInfo = ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Create(),
            "CodeAnalysis",
            "CodeAnalysis",
            LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithMetadataReferences(GetDefaultReferences());

        _project = _workspace.AddProject(projectInfo);
    }

    public async Task<string> FindDefinitionAsync(string code, int position)
    {
        // Find the symbol at the specified position
        var document = CreateDocument(code);
        var semanticModel = await document.GetSemanticModelAsync();
        var sourceText = await document.GetTextAsync();
        var symbol = await SymbolFinder.FindSymbolAtPositionAsync(document, position);

        // Return the symbol information
        if (symbol == null)
            return string.Empty;

        return $"{symbol.Kind} {symbol.Name} defined in {symbol.ContainingNamespace}";
    }

    public string FormatCode(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var workspace = new AdhocWorkspace();
        var formattedNode = Formatter.Format(root, workspace);
        return formattedNode.ToFullString();
    }

    public async Task<List<string>> GetCompletionsAsync(string code, int position)
    {
        var document = CreateDocument(code);
        var completionService = CompletionService.GetService(document);

        if (completionService == null)
        {
            return new List<string>();
        }

        var completions = await completionService.GetCompletionsAsync(document, position);

        return completions?.ItemsList
            .Select(item => item.DisplayText)
            .ToList() ?? new List<string>();
    }

    /// <summary>
    /// Get Syntax Errors and Diagnostics with location details
    /// </summary>
    public List<DiagnosticInfo> GetDiagnostics(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var diagnostics = tree.GetDiagnostics();
        var results = new List<DiagnosticInfo>();

        foreach (var diag in diagnostics)
        {
            var lineSpan = diag.Location.GetLineSpan();
            results.Add(new DiagnosticInfo
            {
                Message = diag.GetMessage(),
                StartLine = lineSpan.StartLinePosition.Line + 1,
                StartColumn = lineSpan.StartLinePosition.Character + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                EndColumn = lineSpan.EndLinePosition.Character + 1,
                Severity = diag.Severity.ToString() // e.g., "Error", "Warning"
            });
        }

        return results;
    }


    private Document CreateDocument(string code)
    {
        var documentId = DocumentId.CreateNewId(_project.Id);
        // Create a new solution with the document added
        var solution = _workspace.CurrentSolution
            .AddDocument(documentId, "Analysis.cs", code);

        // Update the workspace and project
        _workspace.TryApplyChanges(solution);
        _project = solution.GetProject(_project.Id) ?? throw new InvalidOperationException("Project not found");

        // Return the document
        return solution?.GetDocument(documentId);
    }

    private IEnumerable<MetadataReference> GetDefaultReferences()
    {
        var trustedAssembliesPaths = (AppContext.GetData(Name) as string ?? string.Empty).Split(Path.PathSeparator);

        var references = new List<MetadataReference>();
        foreach (var path in trustedAssembliesPaths)
        {
            if (Path.GetFileName(path).StartsWith("System.") ||
                Path.GetFileName(path) == "mscorlib.dll" ||
                Path.GetFileName(path) == "netstandard.dll")
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        return references;
    }
}