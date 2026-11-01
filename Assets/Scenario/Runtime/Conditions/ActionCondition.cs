using System;
using System.ComponentModel;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    [Serializable, DisplayName("Действие механики")]
    public class ActionCondition : StepCondition
    {
        [Tooltip("Объект с механикой, на котором нужно выполнить действие.")]
        [SerializeField] private EventSource source;

        [Tooltip("Требуемое значение, например 1 — открыть, 0 — закрыть.")]
        [SerializeField] private string value;

        [Tooltip("Необязательно: действие засчитывается, только если этот объект в состоянии Condition Value.")]
        [SerializeField] private EventSource conditionSource;

        [SerializeField] private string conditionValue;

        public ActionCondition() { }

        public ActionCondition(EventSource source, string value, EventSource conditionSource = null,
            string conditionValue = null)
        {
            this.source = source;
            this.value = value;
            this.conditionSource = conditionSource;
            this.conditionValue = conditionValue;
        }

        public override StepCheck Check(ScenarioSignal signal)
        {
            if (source == null || signal.Source != source)
                return StepCheck.Ignored;

            if (signal.Value != value)
                return source.WrongValueIsViolation
                    ? StepCheck.Violated($"Неверное действие: {source.Describe(signal.Value)}. Нужно: {source.Describe(value)}")
                    : StepCheck.Ignored;

            if (conditionSource != null && conditionSource.CurrentValue != conditionValue)
                return StepCheck.Violated(
                    $"{source.Describe(value)} можно, только когда {conditionSource.DescribeState(conditionValue)}");

            return StepCheck.Matched;
        }
    }
}
