using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SharpPad.Server.Services.Streaming;

public static class CodePreprocessor
{
    /// <summary>
    /// Processes the user-provided code to remove unsupported API calls.
    /// </summary>
    /// <param name="code">The original code submitted by the user.</param>
    /// <returns>The transformed code with unsupported calls removed or replaced.</returns>
    public static string PreprocessUserCode(string code)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        SyntaxNode root = tree.GetRoot();
        var rewriter = new UnsupportedApiRewriter();
        SyntaxNode newRoot = rewriter.Visit(root);
        return newRoot.ToFullString();
    }
}
