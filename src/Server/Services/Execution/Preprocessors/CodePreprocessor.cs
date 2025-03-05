using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpPad.Server.Services.Execution.Analysis;
using System.Text;

namespace SharpPad.Server.Services.Execution.Preprocessors;

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


    public static string StripProgramWrapper(string code)
    {
        // Parse the code.
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();

        // Extract all using directives.
        var usingDirectives = root.DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(u => u.ToFullString())
            .Distinct();

        // Locate the Program class.
        var programClass = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.Text == "Program");

        if (programClass == null)
        {
            // No wrapper detected; return the original code.
            return code;
        }

        // Find the static Main method.
        var mainMethod = programClass.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == "Main" &&
                                 m.Modifiers.Any(SyntaxKind.StaticKeyword));

        // Extract the statements from Main.
        string mainBody = "";
        if (mainMethod != null && mainMethod.Body != null)
        {
            mainBody = string.Join(Environment.NewLine, mainMethod.Body.Statements.Select(s => s.ToFullString()));
        }

        // Extract any other members (e.g. helper methods) from the Program class.
        var otherMembers = programClass.Members
            .Where(m => m != mainMethod)
            .Select(m => m.ToFullString());

        string otherMembersCode = string.Join(Environment.NewLine, otherMembers);

        // Reconstruct the code as top-level code:
        // First, the using directives, then the main body, followed by the other member declarations.
        var sb = new StringBuilder();

        foreach (var u in usingDirectives)
        {
            sb.AppendLine(u);
        }
        sb.AppendLine();
        sb.AppendLine(mainBody);
        sb.AppendLine();
        sb.AppendLine(otherMembersCode);

        return sb.ToString();
    }
}
