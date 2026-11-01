using System;
using System.ComponentModel;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>Сохраняет журнал прохождения в JSON-файл, чтобы его можно было заново проанализировать.</summary>
    [Serializable, DisplayName("JSON-файл")]
    public class JsonFileLogger : ScenarioLogger
    {
        public override void End(RunLog log)
        {
            string path = RunLogStorage.Save(log);
            if (path != null)
                Debug.Log($"Журнал прохождения сохранён: {path}");
        }
    }
}
