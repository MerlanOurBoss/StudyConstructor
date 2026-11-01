using UnityEngine;
using UnityEngine.UI;

namespace StudyConstructor.Scenario
{
    /// <summary>Нажатие UI-кнопки. В сценарии: exec:BUTTON*#[имя кнопки]*#1.</summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Scenario/Sources/Button Source")]
    public class ButtonSource : EventSource
    {
        public override string Mechanic => "BUTTON";
        public override string Describe(string value) => $"Нажать кнопку «{name}»";

        private void OnEnable() => GetComponent<Button>().onClick.AddListener(OnClick);
        private void OnDisable() => GetComponent<Button>().onClick.RemoveListener(OnClick);

        private void OnClick() => Raise();
    }
}
