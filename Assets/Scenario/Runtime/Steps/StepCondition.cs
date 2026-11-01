using System;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Условие выполнения шага. Создаётся при генерации из команды сценария (см. <see cref="ScriptParser"/>)
    /// и хранится в <see cref="FlowStep"/>. Новый вид шага — новое условие с [Serializable] и новая команда в разборе.
    /// </summary>
    [Serializable]
    public abstract class StepCondition
    {
        public abstract StepCheck Check(ScenarioSignal signal);

        /// <summary>Сбрасывает прогресс условия из нескольких действий перед прохождением или новой попыткой.</summary>
        public virtual void Reset() { }
    }
}
