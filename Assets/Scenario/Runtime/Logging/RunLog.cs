using System;
using System.Collections.Generic;

namespace StudyConstructor.Scenario
{
    /// <summary>Журнал одного прохождения. Сохраняется в JSON и анализируется отдельно от исполнения.</summary>
    [Serializable]
    public class RunLog
    {
        public string mode;
        public string startedAt;
        public List<LogEntry> entries = new();
    }
}
