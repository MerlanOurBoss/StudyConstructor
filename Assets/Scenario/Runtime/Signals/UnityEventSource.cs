using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Источник для любого UnityEvent: Toggle, Slider, событие анимации или свой скрипт.
    /// В списке события выберите этот компонент и метод Fire(). В сценарии: exec:EVENT*#[имя объекта]*#1.
    /// </summary>
    [AddComponentMenu("Scenario/Sources/UnityEvent Source")]
    public class UnityEventSource : EventSource
    {
        [Tooltip("Текст задания для ученика. Если пусто — «Действие „имя объекта“».")]
        [SerializeField] private string description;

        public override string Mechanic => "EVENT";

        public override string Describe(string value) =>
            string.IsNullOrEmpty(description) ? $"Действие «{name}»" : description;

        public void Fire() => Raise();
    }
}
