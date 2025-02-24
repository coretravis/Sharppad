using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SharpPad.Server.Services.Streaming;

public class UnsupportedApiRewriter : CSharpSyntaxRewriter
{
    // Override for expression statements, where the invocation is the entire statement.
    public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
    {
        if (node.Expression is InvocationExpressionSyntax invocation &&
            IsUnsupportedInvocation(invocation, out string methodName))
        {
            // Create a trivia list with a comment and a newline.
            var triviaList = SyntaxFactory.TriviaList(
                SyntaxFactory.Comment($"// Removed unsupported API call: Console.{methodName}()"),
                SyntaxFactory.ElasticCarriageReturnLineFeed);

            return SyntaxFactory.EmptyStatement().WithLeadingTrivia(triviaList);
        }
        return base.VisitExpressionStatement(node);
    }


    // Override for invocation expressions used in other contexts.
    public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.Expression is MemberAccessExpressionSyntax memberAccess &&
            IsUnsupportedInvocation(node, out string methodName))
        {
            if (methodName == "Clear")
            {
                // Replace Console.Clear() with a null literal if used in an expression context.
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
            else if (methodName == "ReadKey")
            {
                // Replace Console.ReadKey() with default(System.ConsoleKeyInfo)
                return SyntaxFactory.ParseExpression("default(System.ConsoleKeyInfo)");
            }
        }
        return base.VisitInvocationExpression(node);
    }

    // Helper method to detect unsupported invocations.
    private bool IsUnsupportedInvocation(InvocationExpressionSyntax node, out string methodName)
    {
        methodName = string.Empty;
        if (node.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            methodName = memberAccess.Name.Identifier.Text;
            string typeName = memberAccess.Expression.ToString();
            if (typeName == "Console" && (methodName == "Clear" || methodName == "ReadKey"))
            {
                return true;
            }
        }
        return false;
    }
}
