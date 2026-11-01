using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Зона P_TC_1 (Collider с галочкой Is Trigger). В сценарии: exec:P_TC_1*#[имя зоны]*#1 — войти, #0 — выйти.
    /// Состояние «игрок в зоне» можно проверять условием: if:[имя зоны]=1.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Scenario/Mechanics/Zone (P_TC_1)")]
    public class ZoneMechanic : EventSource
    {
        [Tooltip("Реагировать только на объекты с этим тегом. Если пусто — на любые объекты.")]
        [SerializeField] private string requiredTag = "Player";

        private int inside;

        public override string Mechanic => "P_TC_1";

        public override bool IsValidValue(string value) => value is "0" or "1";

        // Выйти из зоны, когда нужно в неё войти, — не ошибка: игрок просто ходит по площадке.
        public override bool WrongValueIsViolation => false;

        public override string CurrentValue => inside > 0 ? "1" : "0";

        public override string Describe(string value) =>
            value == "1" ? $"Зайти в зону «{name}»" : $"Выйти из зоны «{name}»";

        public override string DescribeState(string value) =>
            value == "1" ? $"вы в зоне «{name}»" : $"вы вне зоны «{name}»";

        private void OnTriggerEnter(Collider other)
        {
            if (!Fits(other))
                return;

            inside++;
            Raise("1", other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Fits(other) || inside == 0)
                return;

            inside--;
            Raise("0", other.gameObject);
        }

        private bool Fits(Collider other) => string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag);

        private void Reset() => GetComponent<Collider>().isTrigger = true;
    }
}
