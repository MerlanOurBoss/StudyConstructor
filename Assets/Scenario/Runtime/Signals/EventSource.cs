using System;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Механика на объекте сцены. В тексте сценария на неё ссылаются так:
    /// <c>exec:&lt;Mechanic&gt;*#[&lt;имя объекта&gt;]*#&lt;значение&gt;</c>.
    /// Чтобы добавить свою механику, унаследуйтесь, задайте код и вызывайте <see cref="Raise"/>.
    /// </summary>
    public abstract class EventSource : MonoBehaviour
    {
        public event Action<ScenarioSignal> Raised;

        /// <summary>Код механики в тексте сценария, например P_PF_3.</summary>
        public abstract string Mechanic { get; }

        /// <summary>Значение, если в команде exec оно не указано.</summary>
        public virtual string DefaultValue => "1";

        public virtual bool IsValidValue(string value) => value == "1";

        /// <summary>Текст задания для ученика, например «Открыть заглушку 291-VW-M-19».</summary>
        public virtual string Describe(string value) => $"{Mechanic} [{name}] = {value}";

        /// <summary>
        /// Текущее состояние объекта (для заглушки "1" — открыта) или null, если состояния у механики нет.
        /// По нему проверяется условие шага: exec:...*#if:[объект]=значение.
        /// </summary>
        public virtual string CurrentValue => null;

        /// <summary>Состояние словами, например «заглушка 291-VW-M-08 открыта».</summary>
        public virtual string DescribeState(string value) => $"{name} = {value}";

        /// <summary>Возвращает объект в исходное состояние перед новым прохождением.</summary>
        public virtual void ResetState() { }

        protected void Raise(string value = "1", GameObject other = null)
        {
            Raised?.Invoke(new ScenarioSignal(this, value, other));
        }
    }
}
