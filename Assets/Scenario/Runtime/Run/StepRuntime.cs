namespace StudyConstructor.Scenario
{
    /// <summary>Состояние шага в одном прохождении. Сам <see cref="FlowStep"/> при этом не меняется.</summary>
    public class StepRuntime
    {
        public StepRuntime(FlowStep step, GroupRuntime group)
        {
            Step = step;
            Group = group;
        }

        public FlowStep Step { get; }
        public GroupRuntime Group { get; }
        public StepStatus Status { get; internal set; } = StepStatus.Inactive;
        public int Violations { get; internal set; }

        public bool IsFinished => Status is StepStatus.Completed or StepStatus.Failed or StepStatus.Skipped;
    }
}
