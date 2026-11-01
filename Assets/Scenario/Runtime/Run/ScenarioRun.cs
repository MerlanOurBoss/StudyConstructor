using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Одно прохождение сценария в выбранном режиме: идёт по сгенерированной иерархии (группы → блоки → шаги),
    /// активирует шаги, передаёт им сигналы и меняет статусы. Решения о нарушениях принимает <see cref="ExecutionRules"/>.
    /// </summary>
    public class ScenarioRun
    {
        private readonly ExecutionRules rules;
        private readonly List<GroupRuntime> groups = new();
        private readonly List<StepRuntime> steps = new();
        private readonly Dictionary<FlowStep, StepRuntime> runtimes = new();
        private int groupIndex;

        public ScenarioRun(ScenarioMode mode, ScenarioFlow flow)
        {
            Mode = mode;
            rules = mode.Rules;
            if (rules == null)
            {
                Debug.LogWarning($"В режиме «{mode.Name}» не выбраны правила исполнения — используются правила тренировки.");
                rules = new TrainingRules();
            }

            // Без групп (тренировка по общему файлу) все группы файла идут по очереди как одна общая группа.
            var flowGroups = flow.Groups().ToList();
            if (mode.IgnoreGroups)
                AddGroup(new GroupRuntime("Все шаги", flowGroups));
            else
                flowGroups.ForEach(g => AddGroup(new GroupRuntime(g.name, new FlowNode[] { g })));
        }

        public event Action<GroupRuntime> GroupStarted;
        public event Action<StepRuntime> StepChanged;
        public event Action<StepRuntime, string> ViolationRegistered;
        public event Action Finished;

        public ScenarioMode Mode { get; }
        public IReadOnlyList<GroupRuntime> Groups => groups;
        public IReadOnlyList<StepRuntime> Steps => steps;
        public IEnumerable<StepRuntime> ActiveSteps => steps.Where(s => s.Status == StepStatus.Active);
        public bool IsFinished { get; private set; }

        public void Start()
        {
            foreach (var step in steps)
                step.Step.ResetProgress();
            Advance();
        }

        public void Handle(ScenarioSignal signal)
        {
            if (IsFinished)
                return;

            // Одно действие — не больше одного нарушения, даже если оно запрещено сразу нескольким активным шагам.
            bool handled = false;
            bool violated = false;
            foreach (var step in ActiveSteps.ToList())
            {
                var check = step.Step.Check(signal);
                if (!check.IsRelevant)
                    continue;

                handled = true;
                if (check.Kind == StepCheckKind.Matched)
                    SetStatus(step, StepStatus.Completed);
                else if (check.Kind == StepCheckKind.Violated && !violated)
                {
                    RegisterViolation(step, check.Reason);
                    violated = true;
                }
            }

            // Действие не подошло ни одному активному шагу, но это действие шага, чья очередь не наступила.
            // Нарушение засчитывается ближайшему такому шагу.
            if (!handled && Mode.OutOfOrder == OutOfOrderPolicy.Violation)
            {
                var early = steps.FirstOrDefault(s => s.Status == StepStatus.Inactive && s.Step.IsOwnAction(signal));
                if (early != null)
                    RegisterViolation(early, $"Действие выполнено раньше времени: {early.Step.Title}");
            }

            Advance();
        }

        private void AddGroup(GroupRuntime group)
        {
            foreach (var step in group.Roots.SelectMany(r => r.Steps()))
            {
                var stepRuntime = new StepRuntime(step, group);
                group.Add(stepRuntime);
                runtimes.Add(step, stepRuntime);
                steps.Add(stepRuntime);
            }
            groups.Add(group);
        }

        private void RegisterViolation(StepRuntime step, string reason)
        {
            step.Violations++;
            step.Step.ResetProgress();
            ViolationRegistered?.Invoke(step, reason);

            var status = rules.StatusAfterViolation(step);
            if (status != step.Status)
                SetStatus(step, status);
        }

        private void Advance()
        {
            while (groupIndex < groups.Count)
            {
                var group = groups[groupIndex];
                if (!group.Started)
                {
                    group.Started = true;
                    GroupStarted?.Invoke(group);
                }

                if (rules.ShouldSkipRest(group))
                {
                    foreach (var step in group.Steps.Where(s => !s.IsFinished))
                        SetStatus(step, StepStatus.Skipped);
                }

                var next = group.Roots.FirstOrDefault(r => !IsDone(r));
                if (next != null)
                {
                    Activate(next);
                    return;
                }

                groupIndex++;
            }

            if (!IsFinished)
            {
                IsFinished = true;
                Finished?.Invoke();
            }
        }

        private bool IsDone(FlowNode node) =>
            node is FlowStep leaf ? runtimes[leaf].IsFinished : node.Children().All(IsDone);

        /// <summary>
        /// Активирует шаги, до которых дошла очередь: в параллельном блоке — во всех незавершённых детях,
        /// в последовательном (и в группе) — только в первом незавершённом.
        /// </summary>
        private void Activate(FlowNode node)
        {
            if (node is FlowStep leaf)
            {
                var step = runtimes[leaf];
                if (step.Status == StepStatus.Inactive)
                {
                    step.Step.ResetProgress();
                    SetStatus(step, StepStatus.Active);
                }
                return;
            }

            if (node.IsParallel)
            {
                foreach (var child in node.Children().Where(c => !IsDone(c)).ToList())
                    Activate(child);
                return;
            }

            var next = node.Children().FirstOrDefault(c => !IsDone(c));
            if (next != null)
                Activate(next);
        }

        private void SetStatus(StepRuntime step, StepStatus status)
        {
            step.Status = status;
            StepChanged?.Invoke(step);
        }
    }
}
