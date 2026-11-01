using System;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    public enum OutOfOrderPolicy
    {
        [InspectorName("Считать нарушением")] Violation,
        [InspectorName("Игнорировать (шаг неактивен)")] Ignore
    }

    /// <summary>
    /// Режим прохождения: как одни и те же шаги разбиты на группы, в каком порядке активируются
    /// и как обрабатываются нарушения.
    /// </summary>
    [Serializable]
    public class ScenarioMode
    {
        [SerializeField] private string name = "Тренировка";

        [Tooltip("Как обрабатываются нарушения: повтор шага (тренировка) или Failed с порогом пропуска группы (экзамен).")]
        [SerializeReference, SelectType] private ExecutionRules rules = new TrainingRules();

        [Tooltip("Что делать, если пользователь выполняет действие шага, очередь которого ещё не наступила.")]
        [SerializeField] private OutOfOrderPolicy outOfOrder = OutOfOrderPolicy.Violation;

        [Tooltip("Текстовый файл сценария (.txt): шаги exec, блоки start_parallel / start_continuous и группы start_group.")]
        [SerializeField] private TextAsset script;

        [Tooltip("Галочка — группы start_group из файла не учитываются: весь сценарий идёт одной группой (тренировка). " +
                 "Так один файл подходит и тренировке, и экзамену.")]
        [SerializeField] private bool ignoreGroups;

        [Tooltip("Сценарий, сгенерированный из файла кнопкой «Сгенерировать сценарии». Режимы с одним файлом используют " +
                 "один и тот же сценарий. Шаги можно двигать в Hierarchy.")]
        [SerializeField] private ScenarioFlow flow;

        [Tooltip("Как оценивается результат после прохождения. Журнал можно заново оценить любым анализатором.")]
        [SerializeReference, SelectType] private ResultAnalyzer analyzer = new TrainingScoreAnalyzer();

        public ScenarioMode() { }

        internal ScenarioMode(string name, ExecutionRules rules, ResultAnalyzer analyzer, TextAsset script = null)
        {
            this.name = name;
            this.rules = rules;
            this.analyzer = analyzer;
            this.script = script;
        }

        internal void SetOutOfOrder(OutOfOrderPolicy policy) => outOfOrder = policy;
        internal void SetIgnoreGroups(bool ignore) => ignoreGroups = ignore;
        internal void SetFlow(ScenarioFlow generated) => flow = generated;

        public string Name => name;
        public ExecutionRules Rules => rules;
        public ResultAnalyzer Analyzer => analyzer;
        public OutOfOrderPolicy OutOfOrder => outOfOrder;
        public bool IgnoreGroups => ignoreGroups;
        public TextAsset Script => script;
        public ScenarioFlow Flow => flow;

        /// <summary>Файл сценария изменился после генерации — нужно сгенерировать заново.</summary>
        public bool IsFlowOutdated => flow != null && script != null && flow.SourceText != script.text;
    }
}
