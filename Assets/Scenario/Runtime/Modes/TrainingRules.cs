using System;
using System.ComponentModel;

namespace StudyConstructor.Scenario
{
    /// <summary>Тренировка: после нарушения шаг остаётся доступным, пользователь повторяет его.</summary>
    [Serializable, DisplayName("Тренировка: повтор шага после ошибки")]
    public class TrainingRules : ExecutionRules
    {
        public override StepStatus StatusAfterViolation(StepRuntime step) => step.Status;
    }
}
