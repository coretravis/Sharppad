namespace SharpPad.Server.Services.Execution.Analysis;

public interface IStaticAnalysisService
{
    StaticAnalysisResult AnalyzeCode(string code);
}
