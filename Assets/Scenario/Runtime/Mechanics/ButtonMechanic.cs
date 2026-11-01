using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Кнопка на сцене P_BT_1: подойти и нажать (в демо — E). В сценарии: exec:P_BT_1*#[имя объекта].
    /// Для объекта нужен Collider, чтобы игрок мог до него дотянуться.
    /// </summary>
    [AddComponentMenu("Scenario/Mechanics/Button (P_BT_1)")]
    public class ButtonMechanic : EventSource, IInteractable
    {
        [Tooltip("Подвижная часть: вдавливается при нажатии. Если пусто — сам объект.")]
        [SerializeField] private Transform cap;

        [Tooltip("Насколько вдавливается подвижная часть (в её локальных координатах).")]
        [SerializeField] private Vector3 pressOffset = new(0f, -0.05f, 0f);

        [Tooltip("Подсвечивается при нажатии. Если пусто — Renderer самого объекта.")]
        [SerializeField] private Renderer indicator;

        [SerializeField] private Color pressedColor = new(1f, 0.85f, 0.3f);
        [SerializeField] private float pressSeconds = 0.25f;

        private Vector3 restPosition;
        private Color restColor;
        private float releaseAt = -1f;

        public override string Mechanic => "P_BT_1";
        public override string Describe(string value) => $"Нажать кнопку {name}";

        public string Hint => $"Кнопка {name}\nE — нажать";

        public void UsePrimary() => Press();
        public void UseSecondary() { }

        public void Press()
        {
            if (cap != null)
                cap.localPosition = restPosition + pressOffset;
            if (indicator != null)
                indicator.material.color = pressedColor;
            releaseAt = Time.time + pressSeconds;
            Raise();
        }

        private void Awake()
        {
            if (cap == null)
                cap = transform;
            if (indicator == null)
                indicator = GetComponent<Renderer>();

            restPosition = cap.localPosition;
            if (indicator != null)
                restColor = indicator.material.color;
        }

        private void Update()
        {
            if (releaseAt < 0f || Time.time < releaseAt)
                return;

            releaseAt = -1f;
            cap.localPosition = restPosition;
            if (indicator != null)
                indicator.material.color = restColor;
        }
    }
}
