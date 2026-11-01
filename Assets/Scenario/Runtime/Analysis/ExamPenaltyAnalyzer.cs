using System;
using System.ComponentModel;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    [Serializable, DisplayName("Экзамен: штрафы за Failed и Skipped")]
    public class ExamPenaltyAnalyzer : ResultAnalyzer
    {
        [Tooltip("Оценка без штрафов.")]
        [SerializeField] private int maxScore = 100;

        [Tooltip("Штраф за шаг со статусом Failed.")]
        [SerializeField] private int failedPenalty = 10;

        [Tooltip("Штраф за шаг со статусом Skipped.")]
        [SerializeField] private int skippedPenalty = 5;

        protected override AnalysisReport Evaluate(RunLog log)
        {
            var report = new AnalysisReport(DisplayName) { MaxScore = maxScore };
            int penalties = 0;
            foreach (var step in RunLogReader.Summarize(log, report.Warnings))
            {
                int penalty = step.Status switch
                {
                    StepStatus.Failed => failedPenalty,
                    StepStatus.Skipped => skippedPenalty,
                    _ => 0
                };

                report.Steps.Add(new StepResult
                {
                    StepId = step.StepId, Title = step.Title, Group = step.Group, Status = step.Status,
                    Points = -penalty, Detail = penalty > 0 ? $"штраф {penalty}" : ""
                });
                penalties += penalty;
            }

            report.Score = Mathf.Max(0, maxScore - penalties);
            return report;
        }
    }
}
