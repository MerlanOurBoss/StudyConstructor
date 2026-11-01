using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Столкновение с объектом (нужны Collider и Rigidbody хотя бы у одного из объектов).
    /// В сценарии: exec:COLLISION*#[имя объекта]*#1.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Scenario/Sources/Collision Source")]
    public class CollisionSource : EventSource
    {
        [Tooltip("Реагировать только на объекты с этим тегом. Если пусто — на любые объекты.")]
        [SerializeField] private string requiredTag;

        public override string Mechanic => "COLLISION";
        public override string Describe(string value) => $"Коснуться объекта «{name}»";

        private void OnCollisionEnter(Collision collision)
        {
            var other = collision.gameObject;
            if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
                Raise("1", other);
        }
    }
}
