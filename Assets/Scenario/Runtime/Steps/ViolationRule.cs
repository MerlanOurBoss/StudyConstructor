using System;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>Запрещённое действие (команда forbid): любое действие с объектом, пока шаг активен, — нарушение.</summary>
    [Serializable]
    public class ViolationRule
    {
        [Tooltip("Объект с механикой, любое действие с которым запрещено.")]
        [SerializeField] private EventSource source;

        [Tooltip("Сообщение, которое увидит пользователь при нарушении.")]
        [SerializeField] private string message;

        public ViolationRule() { }

        public ViolationRule(EventSource source, string message)
        {
            this.source = source;
            this.message = message;
        }

        public string Message => message;

        public bool Matches(ScenarioSignal signal) => source != null && signal.Source == source;
    }
}
