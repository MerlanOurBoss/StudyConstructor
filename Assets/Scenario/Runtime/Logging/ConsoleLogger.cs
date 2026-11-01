using System;
using System.ComponentModel;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>Выводит каждую запись журнала в консоль Unity.</summary>
    [Serializable, DisplayName("Консоль Unity")]
    public class ConsoleLogger : ScenarioLogger
    {
        public override void Write(LogEntry entry)
        {
            Debug.Log($"[Scenario] {entry.time,6:0.0}s  {entry.type}  {entry.stepId}  {entry.result}");
        }
    }
}
