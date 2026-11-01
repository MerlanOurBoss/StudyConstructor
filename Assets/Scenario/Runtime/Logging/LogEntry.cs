using System;
using System.Collections.Generic;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Запись журнала. Минимальный общий контракт: время, тип, шаг, результат.
    /// Дополнительные данные — в <see cref="data"/>: новые поля и типы записей не ломают
    /// существующую обработку, потому что читатели пропускают незнакомое.
    /// </summary>
    [Serializable]
    public class LogEntry
    {
        /// <summary>Секунды от начала прохождения.</summary>
        public float time;
        public string type;
        public string stepId;
        public string result;
        public List<LogField> data = new();

        public LogEntry() { }

        public LogEntry(float time, string type, string stepId = null, string result = null)
        {
            this.time = time;
            this.type = type;
            this.stepId = stepId;
            this.result = result;
        }

        public string Get(string key) => data?.Find(f => f.key == key)?.value;

        public LogEntry With(string key, string value)
        {
            data.Add(new LogField { key = key, value = value });
            return this;
        }
    }

    [Serializable]
    public class LogField
    {
        public string key;
        public string value;
    }
}
