namespace StudyConstructor.Scenario
{
    /// <summary>Типы записей журнала. Тип — строка, поэтому новые типы можно добавлять без изменения контракта.</summary>
    public static class LogTypes
    {
        public const string RunStarted = "RunStarted";
        /// <summary>Шаг входит в прохождение. Нужен, чтобы знать полный список шагов (максимальный балл, пропуски).</summary>
        public const string StepRegistered = "StepRegistered";
        public const string GroupStarted = "GroupStarted";
        /// <summary>Шаг сменил статус; result — новый <see cref="StepStatus"/>.</summary>
        public const string StepStatus = "StepStatus";
        /// <summary>Нарушение на шаге; result — причина.</summary>
        public const string Violation = "Violation";
        public const string RunFinished = "RunFinished";
    }

    /// <summary>Ключи дополнительных данных записи.</summary>
    public static class LogKeys
    {
        public const string Mode = "mode";
        public const string Group = "group";
        public const string Title = "title";
        /// <summary>Статус шага в момент нарушения: Inactive означает действие вне очереди.</summary>
        public const string StepState = "stepState";
    }
}
