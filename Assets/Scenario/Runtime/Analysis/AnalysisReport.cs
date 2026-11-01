using System.Collections.Generic;

namespace StudyConstructor.Scenario
{
    public class AnalysisReport
    {
        public AnalysisReport(string analyzerName) => AnalyzerName = analyzerName;

        public string AnalyzerName { get; }
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public List<StepResult> Steps { get; } = new();
        public List<string> Warnings { get; } = new();
    }

    public class StepResult
    {
        public string StepId;
        public string Title;
        public string Group;
        public StepStatus Status;
        public int Points;
        public string Detail;
    }
}
