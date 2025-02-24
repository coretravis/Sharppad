using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace SharpPad.Server.Services.Execution.Compiler;

public class CompilerVersionService : ICompilerVersionService
{
    public CSharpCompilationOptions AdjustCompilationOptions(CSharpCompilationOptions options, string compilerVersion)
    {
        // Map the provided compiler version to a C# language version.
        var languageVersion = MapCompilerVersionToLanguageVersion(compilerVersion);
        Console.WriteLine($"[CompilerVersionService::Info] Selected language version: {languageVersion} for compiler version '{compilerVersion}'");
        return options;
    }

    public ScriptOptions AdjustScriptOptions(ScriptOptions options, string compilerVersion)
    {
        // Map the provided compiler version to a C# language version.
        var languageVersion = MapCompilerVersionToLanguageVersion(compilerVersion);

        // Log the chosen language version.
        Console.WriteLine($"[CompilerVersionService] Selected language version for scripting: {languageVersion} for compiler version '{compilerVersion}'");

        // Update script options with the appropriate language version
        return options.WithLanguageVersion(languageVersion);
    }

    private LanguageVersion MapCompilerVersionToLanguageVersion(string compilerVersion)
    {
        // Default to the latest version if no match is found.
        LanguageVersion languageVersion = LanguageVersion.Default;

        if (string.IsNullOrWhiteSpace(compilerVersion))
        {
            Console.WriteLine("[CompilerVersionService] No compiler version specified. Using default language version.");
            return languageVersion;
        }

        // Map common .NET versions to corresponding C# language versions.
        switch (compilerVersion.Trim().ToLowerInvariant())
        {
            case "netcoreapp3.1":
                languageVersion = LanguageVersion.CSharp8;
                break;
            case ".net5.0":
                languageVersion = LanguageVersion.CSharp9;
                break;
            case ".net6.0":
                languageVersion = LanguageVersion.CSharp10;
                break;
            case ".net7.0":
                languageVersion = LanguageVersion.CSharp11;
                break;
            case ".net8.0":
                languageVersion = LanguageVersion.CSharp12;
                break;
            case ".net9.0":
                languageVersion = LanguageVersion.Latest;
                break;
            default:
                Console.WriteLine($"[CompilerVersionService::Warning] Unrecognized compiler version '{compilerVersion}'. Using default language version.");
                languageVersion = LanguageVersion.Default;
                break;
        }

        return languageVersion;
    }

    // Add a public method to get language version
    public LanguageVersion GetLanguageVersion(string compilerVersion)
    {
        return MapCompilerVersionToLanguageVersion(compilerVersion);
    }
}