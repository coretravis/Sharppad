using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SharpPad.Server.Services.Execution.Analysis;

/// <summary>
/// A service to perform in-depth static analysis on C# code.
/// </summary>
public class StaticAnalysisService : IStaticAnalysisService
{
    public StaticAnalysisResult AnalyzeCode(string code)
    {
        var result = new StaticAnalysisResult();

        // Parse the code into a syntax tree.
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        // Create a compilation for the syntax tree.
        var compilation = CSharpCompilation.Create("AnalysisCompilation",
            syntaxTrees: new[] { tree },
            references: new[]
            {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            });

        // Retrieve the semantic model from the compilation.
        var semanticModel = compilation.GetSemanticModel(tree);

        // Walk the syntax tree with our expanded security walker.
        var walker = new SecuritySyntaxWalker(semanticModel);
        walker.Visit(root);

        // Collect warnings.
        result.Warnings.AddRange(walker.Warnings);
        result.IsSafe = walker.Warnings.Count == 0;
        return result;
    }

}
