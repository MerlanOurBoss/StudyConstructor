using System;
using System.ComponentModel;
using System.Reflection;

namespace StudyConstructor.Scenario
{
    [Serializable]
    public abstract class ResultAnalyzer
    {
        public string DisplayName => GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? GetType().Name;
        public AnalysisReport Analyze(RunLog log)
        {
            try
            {
                return Evaluate(log ?? new RunLog());
            }
            catch (Exception e)
            {
                var report = new AnalysisReport(DisplayName);
                report.Warnings.Add($"Анализ не выполнен: {e.Message}");
                return report;
            }
        }

        protected abstract AnalysisReport Evaluate(RunLog log);
    }
}
