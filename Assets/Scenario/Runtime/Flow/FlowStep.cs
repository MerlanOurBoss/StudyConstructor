using System.Collections.Generic;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Шаг сценария — одна команда exec. Шаг знает только, что нужно сделать и что запрещено, пока он активен;
    /// когда он активен и что будет за ошибку, решают блоки и правила режима.
    /// </summary>
    [AddComponentMenu("Scenario/Flow/Flow Step")]
    public class FlowStep : FlowNode
    {
        [Tooltip("Идентификатор в журнале, например P_PF_3[291-VW-M-19]=1.")]
        [SerializeField] private string id;

        [Tooltip("Текст задания для ученика.")]
        [SerializeField, TextArea] private string title;

        [Tooltip("Строка в файле сценария, из которой создан шаг.")]
        [SerializeField] private int line;

        [Tooltip("Что нужно сделать, чтобы шаг считался выполненным.")]
        [SerializeReference, SelectType] private StepCondition condition;

        [Tooltip("Действия, запрещённые, пока шаг активен (команда forbid).")]
        [SerializeField] private List<ViolationRule> violations = new();

        public string Id => id;
        public string Title => string.IsNullOrEmpty(title) ? name : title;
        public int Line => line;

        internal void Setup(string stepId, string stepTitle, int sourceLine, StepCondition stepCondition)
        {
            id = stepId;
            title = stepTitle;
            line = sourceLine;
            condition = stepCondition;
        }

        internal void AddViolation(ViolationRule rule) => violations.Add(rule);

        /// <summary>
        /// Проверяет действие: сначала запреты, затем условие шага.
        /// <see cref="StepCheckKind.Matched"/> означает, что шаг выполнен.
        /// </summary>
        public StepCheck Check(ScenarioSignal signal)
        {
            foreach (var rule in violations)
            {
                if (rule.Matches(signal))
                    return StepCheck.Violated(rule.Message);
            }
            return condition?.Check(signal) ?? StepCheck.Ignored;
        }

        /// <summary>Это действие самого шага (а не запрещённое): нужно для правила «действие вне очереди».</summary>
        public bool IsOwnAction(ScenarioSignal signal) => condition?.Check(signal).IsRelevant ?? false;

        public void ResetProgress() => condition?.Reset();
    }
}
