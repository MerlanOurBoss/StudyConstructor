using System.Collections.Generic;
using System.Linq;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Группа в одном прохождении: её шаги и узлы иерархии, которые выполняются по очереди.
    /// Обычно это одна <see cref="FlowGroup"/>; если режим игнорирует группы — все группы файла.
    /// </summary>
    public class GroupRuntime
    {
        private readonly List<StepRuntime> steps = new();

        public GroupRuntime(string name, IReadOnlyList<FlowNode> roots)
        {
            Name = name;
            Roots = roots;
        }

        public string Name { get; }
        public IReadOnlyList<StepRuntime> Steps => steps;
        public int Violations => steps.Sum(s => s.Violations);

        internal IReadOnlyList<FlowNode> Roots { get; }
        internal bool Started { get; set; }

        internal void Add(StepRuntime step) => steps.Add(step);
    }
}
