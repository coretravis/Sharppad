using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SharpPad.Server.Services.Execution.Analysis
{
    /// <summary>
    /// A syntax walker that scans the syntax tree for dangerous patterns using semantic analysis.
    /// </summary>
    public class SecuritySyntaxWalker(SemanticModel semanticModel) : CSharpSyntaxWalker
    {
        /// <summary>
        /// Collected warnings regarding potential security risks.
        /// </summary>
        public List<string> Warnings { get; } = new List<string>();

        private readonly SemanticModel _semanticModel = semanticModel;

        // Map of fully qualified method names to a corresponding warning.
        private static readonly Dictionary<string, string> DangerousMethods = new Dictionary<string, string>
        {
            { "System.Diagnostics.Process.Start", "Usage of Process.Start detected, which may allow spawning external processes." },
            { "System.IO.File.Open", "Usage of file I/O operations detected, which may be dangerous in untrusted code." },
            { "System.Reflection.Assembly.Load", "Usage of assembly loading detected, which may allow dynamic code execution." },
            { "System.Reflection.Assembly.LoadFrom", "Usage of assembly loading detected, which may allow dynamic code execution." },
            { "System.Activator.CreateInstance", "Usage of Activator.CreateInstance detected, which may allow dynamic type instantiation." },
            { "System.Net.Http.HttpClient", "Usage of network communication APIs detected (HttpClient), which may be unsafe." },
            { "System.Net.WebClient", "Usage of network communication APIs detected (WebClient), which may be unsafe." },
            { "Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript", "Usage of Roslyn scripting APIs detected, which may allow dynamic code execution." },
            { "System.Runtime.InteropServices.Marshal", "Usage of Marshal class detected, which may allow unsafe memory operations." },
            { "System.GCHandle", "Usage of GCHandle detected, which may allow unsafe memory pinning." },
            { "System.Threading.Thread", "Usage of threading APIs detected, which may lead to concurrency issues." },
            { "System.Threading.Tasks.Task", "Usage of threading APIs detected, which may lead to concurrency issues." },
            { "System.AppDomain", "Usage of AppDomain detected, which may allow manipulation of application domains." },
            { "System.Environment.Exit", "Usage of Environment.Exit detected, which may terminate the process." },
            { "Microsoft.Win32.Registry", "Usage of Registry detected, which may allow manipulation of the Windows registry." },
            { "System.ServiceProcess.ServiceController", "Usage of ServiceController detected, which may allow control of Windows services." },
            { "System.Net.Dns", "Usage of Dns detected, which may allow network name resolution." },
            { "System.Net.Sockets.Socket", "Usage of Socket detected, which may allow low-level network communication." },
            { "System.Net.WebRequest", "Usage of WebRequest detected, which may allow network communication." },
            { "System.Environment", "Usage of Environment class detected, which may expose system information or control." }
        };

        // List of dangerous namespaces that we want to flag when used in using directives.
        // We should probably be reading this from configuration to allow flexibility
        private static readonly List<string> DangerousNamespaces = new List<string>
        {
            "System.Net",
            "System.IO",
            "System.Threading",
            "System.Reflection",
            "System.Diagnostics",
            "System.Activator",
        };

        public override void VisitUnsafeStatement(UnsafeStatementSyntax node)
        {
            Warnings.Add("Unsafe code block detected.");
            base.VisitUnsafeStatement(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Modifiers.Any(m => m.IsKind(SyntaxKind.UnsafeKeyword)))
            {
                Warnings.Add($"Method '{node.Identifier.ValueText}' is marked as unsafe.");
            }
            base.VisitMethodDeclaration(node);
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            // Resolve the attribute symbol to check for DllImportAttribute.
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                var containingType = methodSymbol.ContainingType;
                if (containingType != null && containingType.Name == "DllImportAttribute")
                {
                    Warnings.Add("Usage of DllImport attribute detected (P/Invoke), which may allow native calls.");
                }
            }
            base.VisitAttribute(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                // Construct the fully qualified name of the method.
                string fullName = $"{methodSymbol.ContainingType}.{methodSymbol.Name}";

                if (DangerousMethods.TryGetValue(fullName, out string warning))
                {
                    Warnings.Add(warning);
                }

                // Additional check: warn if the method belongs to a dangerous namespace.
                string? containingNamespace = methodSymbol.ContainingNamespace?.ToString();
                if (containingNamespace != null)
                {
                    foreach (var dangerousNs in DangerousNamespaces)
                    {
                        if (containingNamespace.StartsWith(dangerousNs) &&
                            // Avoid duplicate warnings if we already have one for a specific API.
                            !Warnings.Contains($"Usage of namespace '{containingNamespace}' detected; operations might be restricted."))
                        {
                            Warnings.Add($"Usage of namespace '{containingNamespace}' detected; operations might be restricted.");
                        }
                    }
                }
            }
            base.VisitInvocationExpression(node);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            string namespaceName = node.Name.ToString();
            foreach (var dangerousNs in DangerousNamespaces)
            {
                if (namespaceName.StartsWith(dangerousNs))
                {
                    Warnings.Add($"Usage of namespace '{namespaceName}' detected; operations might be restricted.");
                }
            }
            base.VisitUsingDirective(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is IMethodSymbol ctorSymbol)
            {
                var typeSymbol = ctorSymbol.ContainingType;
                string fullTypeName = typeSymbol.ToString();
                // Check for dangerous type instantiation; here we check for types that include "Process".
                if (fullTypeName.Contains("Process"))
                {
                    Warnings.Add($"Instantiation of type '{fullTypeName}' detected, which may be used to start processes.");
                }
            }
            base.VisitObjectCreationExpression(node);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.Text == "dynamic")
            {
                Warnings.Add("Usage of the 'dynamic' keyword detected, which may lead to unpredictable behavior.");
            }
            base.VisitIdentifierName(node);
        }
    }
}
