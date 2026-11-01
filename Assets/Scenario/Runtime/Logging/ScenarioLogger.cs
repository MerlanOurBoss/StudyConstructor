using System;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Куда выводится журнал (файл, консоль, сервер...). Журнал в памяти ведёт <see cref="RunRecorder"/>,
    /// логгер только получает записи. Новый логгер — наследник с [Serializable], он сразу появится в списке Inspector.
    /// </summary>
    [Serializable]
    public abstract class ScenarioLogger
    {
        public virtual void Begin(RunLog log) { }
        public virtual void Write(LogEntry entry) { }
        public virtual void End(RunLog log) { }
    }
}
