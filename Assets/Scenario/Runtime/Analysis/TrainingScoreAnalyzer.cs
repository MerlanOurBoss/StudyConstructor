using System;
using System.ComponentModel;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    [Serializable, DisplayName("Тренировка: баллы за попытки")]
    public class TrainingScoreAnalyzer : ResultAnalyzer
    {
        [Tooltip("Баллы за шаг, выполненный с первой попытки. Максимальная оценка = это значение × число шагов.")]
        [SerializeField] private int firstAttemptPoints = 10;

        [Tooltip("Баллы за шаг, выполненный со второй попытки.")]
        [SerializeField] private int secondAttemptPoints = 5;

        [Tooltip("Баллы за шаг, выполненный с третьей и последующих попыток.")]
        [SerializeField] private int laterAttemptPoints;

        protected override AnalysisReport Evaluate(RunLog log)
        {
            var report = new AnalysisReport(DisplayName);
            foreach (var step in RunLogReader.Summarize(log, report.Warnings))
            {
                int points = 0;
                string detail = "не выполнен";
                if (step.Status == StepStatus.Completed)
                {
                    int attempt = step.Violations + 1;
                    points = attempt switch
                    {
                        1 => firstAttemptPoints,
                        2 => secondAttemptPoints,
                        _ => laterAttemptPoints
                    };
                    detail = $"выполнен с попытки {attempt}";
                }

                report.Steps.Add(new StepResult
                {
                    StepId = step.StepId, Title = step.Title, Group = step.Group,
                    Status = step.Status, Points = points, Detail = detail
                });
                report.Score += points;
                report.MaxScore += firstAttemptPoints;
            }
            return report;
        }
    }
}
