using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Переводит события прохождения в записи журнала: хранит их в <see cref="Log"/> и передаёт логгерам.
    /// Исполнение о журнале не знает, поэтому логгеры заменяются независимо от него.
    /// </summary>
    public class RunRecorder
    {
        private readonly IReadOnlyList<ScenarioLogger> loggers;
        private readonly float startTime = Time.time;

        public RunRecorder(ScenarioRun run, IReadOnlyList<ScenarioLogger> loggers)
        {
            this.loggers = loggers;
            Log = new RunLog { mode = run.Mode.Name, startedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
            foreach (var logger in loggers)
                logger?.Begin(Log);

            Record(Entry(LogTypes.RunStarted).With(LogKeys.Mode, run.Mode.Name));
            foreach (var step in run.Steps)
            {
                Record(Entry(LogTypes.StepRegistered, step.Step.Id)
                    .With(LogKeys.Group, step.Group.Name)
                    .With(LogKeys.Title, step.Step.Title));
            }

            run.GroupStarted += group =>
                Record(Entry(LogTypes.GroupStarted).With(LogKeys.Group, group.Name));
            run.StepChanged += step =>
                Record(Entry(LogTypes.StepStatus, step.Step.Id, step.Status.ToString()));
            run.ViolationRegistered += (step, reason) =>
                Record(Entry(LogTypes.Violation, step.Step.Id, reason).With(LogKeys.StepState, step.Status.ToString()));
            run.Finished += () =>
            {
                Record(Entry(LogTypes.RunFinished));
                Close();
            };
        }

        public RunLog Log { get; }
        public bool IsClosed { get; private set; }

        /// <summary>Завершает журнал. Вызывается по окончании прохождения или при его прерывании.</summary>
        public void Close()
        {
            if (IsClosed)
                return;

            IsClosed = true;
            foreach (var logger in loggers)
                logger?.End(Log);
        }

        private LogEntry Entry(string type, string stepId = null, string result = null)
        {
            return new LogEntry(Time.time - startTime, type, stepId, result);
        }

        private void Record(LogEntry entry)
        {
            Log.entries.Add(entry);
            foreach (var logger in loggers)
                logger?.Write(entry);
        }
    }
}
