using System;
using System.ComponentModel;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Экзамен: ошибочный шаг получает Failed без повтора. Когда нарушений в группе
    /// становится не меньше порога, незавершённые шаги группы пропускаются (Skipped).
    /// </summary>
    [Serializable, DisplayName("Экзамен: Failed и пропуск группы по порогу")]
    public class ExamRules : ExecutionRules
    {
        [Tooltip("Сколько нарушений в группе допускается, прежде чем её оставшиеся шаги будут пропущены.")]
        [SerializeField, Min(1)] private int violationThreshold = 2;

        public override StepStatus StatusAfterViolation(StepRuntime step) => StepStatus.Failed;

        public override bool ShouldSkipRest(GroupRuntime group) => group.Violations >= violationThreshold;
    }
}
