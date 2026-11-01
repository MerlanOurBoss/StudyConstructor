using System;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Запрещённое действие (команда forbid): действие с объектом, пока шаг активен, — нарушение.
    /// Если значение указано, запрещено только оно (например, вход в зону, но не выход).
    /// </summary>
    [Serializable]
    public class ViolationRule
    {
        [Tooltip("Объект с механикой, действие с которым запрещено.")]
        [SerializeField] private EventSource source;

        [Tooltip("Запрещённое значение. Если пусто — запрещено любое действие с объектом.")]
        [SerializeField] private string value;

        [Tooltip("Сообщение, которое увидит пользователь при нарушении.")]
        [SerializeField] private string message;

        public ViolationRule() { }

        public ViolationRule(EventSource source, string value, string message)
        {
            this.source = source;
            this.value = value;
            this.message = message;
        }

        public string Message => message;

        public bool Matches(ScenarioSignal signal) =>
            source != null && signal.Source == source && (string.IsNullOrEmpty(value) || signal.Value == value);
    }
}
