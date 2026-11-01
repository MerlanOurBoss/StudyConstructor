namespace StudyConstructor.Scenario
{
    public enum StepCheckKind
    {
        /// <summary>Сигнал не относится к условию.</summary>
        Ignored,
        /// <summary>Сигнал продвинул составное условие, но оно ещё не выполнено.</summary>
        Progress,
        /// <summary>Условие выполнено.</summary>
        Matched,
        /// <summary>Действие относится к условию, но выполнено неправильно.</summary>
        Violated
    }

    /// <summary>Реакция условия или шага на сигнал.</summary>
    public readonly struct StepCheck
    {
        public readonly StepCheckKind Kind;

        /// <summary>Причина нарушения для пользователя и журнала. Только для <see cref="StepCheckKind.Violated"/>.</summary>
        public readonly string Reason;

        private StepCheck(StepCheckKind kind, string reason)
        {
            Kind = kind;
            Reason = reason;
        }

        public static StepCheck Ignored => new(StepCheckKind.Ignored, null);
        public static StepCheck Progress => new(StepCheckKind.Progress, null);
        public static StepCheck Matched => new(StepCheckKind.Matched, null);
        public static StepCheck Violated(string reason) => new(StepCheckKind.Violated, reason);

        /// <summary>Сигнал имеет отношение к шагу (нужно для правила «действие вне очереди»).</summary>
        public bool IsRelevant => Kind != StepCheckKind.Ignored;
    }
}
