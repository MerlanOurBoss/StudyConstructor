using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Действие на объекте сцены: какая механика сработала и с каким значением
    /// (для заглушки "1" — открыли, "0" — закрыли). Шаги сами решают, относится ли действие к ним.
    /// </summary>
    public readonly struct ScenarioSignal
    {
        public readonly EventSource Source;
        public readonly string Value;

        /// <summary>Объект, вызвавший событие (вошедший в триггер, столкнувшийся). Может быть null.</summary>
        public readonly GameObject Other;

        public ScenarioSignal(EventSource source, string value, GameObject other = null)
        {
            Source = source;
            Value = value;
            Other = other;
        }
    }
}
