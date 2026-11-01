using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Вход в зону (Collider с галочкой Is Trigger) или выход из неё.
    /// В сценарии: exec:ZONE*#[имя зоны]*#1.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Scenario/Sources/Trigger Source")]
    public class TriggerSource : EventSource
    {
        public enum Moment { Enter, Exit }

        [Tooltip("Когда срабатывает: при входе в зону или при выходе из неё.")]
        [SerializeField] private Moment fireOn = Moment.Enter;

        [Tooltip("Реагировать только на объекты с этим тегом. Если пусто — на любые объекты.")]
        [SerializeField] private string requiredTag = "Player";

        public override string Mechanic => "ZONE";

        public override string Describe(string value) =>
            fireOn == Moment.Enter ? $"Зайти в зону «{name}»" : $"Выйти из зоны «{name}»";

        private void OnTriggerEnter(Collider other)
        {
            if (fireOn == Moment.Enter)
                TryRaise(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (fireOn == Moment.Exit)
                TryRaise(other.gameObject);
        }

        private void TryRaise(GameObject other)
        {
            if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
                Raise("1", other);
        }

        private void Reset() => GetComponent<Collider>().isTrigger = true;
    }
}
