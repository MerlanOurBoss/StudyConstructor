using System;
using System.Collections.Generic;
using System.Linq;

namespace StudyConstructor.Scenario
{
    public class StepSummary
    {
        public string StepId;
        public string Title;
        public string Group;
        public StepStatus Status = StepStatus.Inactive;
        public int Violations;
    }
    public static class RunLogReader
    {
        public static List<StepSummary> Summarize(RunLog log, List<string> warnings)
        {
            var steps = new List<StepSummary>();
            var entries = log?.entries ?? new List<LogEntry>();

            StepSummary Get(string id)
            {
                var step = steps.Find(s => s.StepId == id);
                if (step == null)
                {
                    step = new StepSummary { StepId = id, Title = id };
                    steps.Add(step);
                }
                return step;
            }

            foreach (var entry in entries.Where(e => e != null))
            {
                switch (entry.type)
                {
                    case LogTypes.StepRegistered when HasStep(entry, warnings):
                        var registered = Get(entry.stepId);
                        registered.Title = entry.Get(LogKeys.Title) ?? entry.stepId;
                        registered.Group = entry.Get(LogKeys.Group);
                        break;

                    case LogTypes.StepStatus when HasStep(entry, warnings):
                        if (Enum.TryParse(entry.result, out StepStatus status))
                            Get(entry.stepId).Status = status;
                        else
                            warnings.Add($"Неизвестный статус «{entry.result}» у шага {entry.stepId} — запись пропущена.");
                        break;

                    case LogTypes.Violation when HasStep(entry, warnings):
                        Get(entry.stepId).Violations++;
                        break;
                }
            }

            if (entries.Count == 0)
                warnings.Add("Журнал пуст — оценивать нечего.");
            else if (!entries.Any(e => e?.type == LogTypes.StepRegistered))
                warnings.Add("В журнале нет списка шагов — учтены только шаги, встретившиеся в событиях.");

            if (entries.Count > 0 && !entries.Any(e => e?.type == LogTypes.RunFinished))
                warnings.Add("Прохождение не завершено — оценка незавершённых шагов может быть неточной.");

            return steps;
        }

        private static bool HasStep(LogEntry entry, List<string> warnings)
        {
            if (!string.IsNullOrEmpty(entry.stepId))
                return true;

            warnings.Add($"Запись «{entry.type}» на {entry.time:0.0} с без идентификатора шага — пропущена.");
            return false;
        }
    }
}
