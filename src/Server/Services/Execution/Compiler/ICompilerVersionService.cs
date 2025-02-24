using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;

namespace SharpPad.Server.Services.Execution.Compiler;

public interface ICompilerVersionService
{
    CSharpCompilationOptions AdjustCompilationOptions(CSharpCompilationOptions options, string compilerVersion);
    ScriptOptions AdjustScriptOptions(ScriptOptions options, string compilerVersion);
    LanguageVersion GetLanguageVersion(string compilerVersion);
}