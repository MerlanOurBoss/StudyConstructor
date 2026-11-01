using System;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Правила режима: что происходит с шагом и группой при нарушении.
    /// Порядок активации задаёт <see cref="ScenarioRun"/>, правила только принимают решения.
    /// </summary>
    [Serializable]
    public abstract class ExecutionRules
    {
        /// <summary>Статус шага после нарушения. Если вернуть текущий статус, шаг можно повторить.</summary>
        public abstract StepStatus StatusAfterViolation(StepRuntime step);

        /// <summary>Нужно ли пропустить оставшиеся шаги группы.</summary>
        public virtual bool ShouldSkipRest(GroupRuntime group) => false;
    }
}
