using System.Collections.Generic;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Корень сгенерированного сценария одного режима. Создаётся кнопкой «Сгенерировать сценарии»
    /// в инспекторе ScenarioRunner из текстового файла; дети — группы (<see cref="FlowGroup"/>).
    /// </summary>
    [AddComponentMenu("Scenario/Flow/Scenario Flow")]
    public class ScenarioFlow : MonoBehaviour
    {
        [SerializeField, HideInInspector] private string sourceText;

        /// <summary>Текст, из которого сгенерирован сценарий: по нему видно, что файл изменился после генерации.</summary>
        public string SourceText => sourceText;

        internal void SetSource(string text) => sourceText = text;

        public IEnumerable<FlowGroup> Groups()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf && child.TryGetComponent(out FlowGroup group))
                    yield return group;
            }
        }
    }
}
