namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Объект, с которым игрок взаимодействует вблизи двумя клавишами (в демо — E и Q).
    /// Ввод обрабатывает игрок, механика только выполняет действие.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Подсказка над объектом: что это и какие клавиши нажимать.</summary>
        string Hint { get; }

        void UsePrimary();
        void UseSecondary();
    }
}
