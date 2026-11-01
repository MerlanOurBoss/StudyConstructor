using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Заглушка P_PF_3. В сценарии: exec:P_PF_3*#[имя объекта]*#1 — открыть, #0 — закрыть.
    /// Каждое нажатие — действие, даже если заглушка уже в этом состоянии.
    /// </summary>
    [AddComponentMenu("Scenario/Mechanics/Plug (P_PF_3)")]
    public class PlugMechanic : EventSource, IInteractable
    {
        [Tooltip("Открыта ли заглушка в начале прохождения.")]
        [SerializeField] private bool startOpen;

        [Tooltip("Подвижная часть заглушки: поворачивается при открытии. Можно не указывать.")]
        [SerializeField] private Transform cap;

        [Tooltip("Угол поворота подвижной части в открытом состоянии.")]
        [SerializeField] private Vector3 openRotation = new(90f, 0f, 0f);

        [Tooltip("Окрашивается в цвет состояния. Можно не указывать.")]
        [SerializeField] private Renderer indicator;

        [SerializeField] private Color openColor = new(0.3f, 0.85f, 0.4f);
        [SerializeField] private Color closedColor = new(0.85f, 0.25f, 0.2f);

        private Quaternion closedRotation;
        private bool isOpen;

        public override string Mechanic => "P_PF_3";
        public bool IsOpen => isOpen;

        public string Hint => $"Заглушка {name} — {(isOpen ? "открыта" : "закрыта")}\nE — открыть    Q — закрыть";

        public override bool IsValidValue(string value) => value is "0" or "1";

        public override string CurrentValue => isOpen ? "1" : "0";

        public override string DescribeState(string value) =>
            value == "1" ? $"заглушка {name} открыта" : $"заглушка {name} закрыта";

        public override string Describe(string value) =>
            value == "1" ? $"Открыть заглушку {name}" : $"Закрыть заглушку {name}";

        public void UsePrimary() => SetOpen(true);
        public void UseSecondary() => SetOpen(false);

        public void SetOpen(bool open)
        {
            isOpen = open;
            UpdateVisual();
            Raise(open ? "1" : "0");
        }

        public override void ResetState()
        {
            isOpen = startOpen;
            UpdateVisual();
        }

        private void Awake()
        {
            if (cap != null)
                closedRotation = cap.localRotation;
            ResetState();
        }

        private void UpdateVisual()
        {
            if (cap != null)
                cap.localRotation = isOpen ? closedRotation * Quaternion.Euler(openRotation) : closedRotation;
            if (indicator != null)
                indicator.material.color = isOpen ? openColor : closedColor;
        }
    }
}
