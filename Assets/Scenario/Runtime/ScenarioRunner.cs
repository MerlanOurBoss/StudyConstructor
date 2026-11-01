using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Точка входа сценария на сцене: хранит режимы и логгеры, запускает сгенерированный сценарий режима
    /// и передаёт прохождению сигналы всех механик сцены.
    /// </summary>
    [AddComponentMenu("Scenario/Scenario Runner")]
    public class ScenarioRunner : MonoBehaviour
    {
        [Tooltip("Режимы прохождения. У каждого режима свой файл сценария, правила и анализатор.")]
        [SerializeField] private List<ScenarioMode> modes = new();

        [Tooltip("Куда дополнительно выводится журнал. Журнал в памяти для просмотра и анализа ведётся всегда.")]
        [SerializeReference, SelectType] private List<ScenarioLogger> loggers = new() { new JsonFileLogger() };

        private EventSource[] sources = Array.Empty<EventSource>();

        public event Action<ScenarioRun> RunStarted;

        public IReadOnlyList<ScenarioMode> Modes => modes;
        public ScenarioRun CurrentRun { get; private set; }
        public RunRecorder Recorder { get; private set; }

        /// <summary>Анализаторы всех режимов — для повторной оценки журнала.</summary>
        public IEnumerable<ResultAnalyzer> Analyzers => modes.Select(m => m.Analyzer).Where(a => a != null);

        /// <summary>Запускает сгенерированный сценарий режима. Если его нет или он пуст, прохождение не начинается.</summary>
        public bool TryStartRun(int modeIndex, out List<string> errors)
        {
            errors = new List<string>();
            var mode = modes[modeIndex];
            if (mode.Flow == null)
            {
                errors.Add($"Сценарий режима «{mode.Name}» не сгенерирован: нажмите «Сгенерировать сценарии» в инспекторе {name}.");
                return false;
            }
            if (!mode.Flow.Groups().SelectMany(g => g.Steps()).Any())
            {
                errors.Add($"В сценарии режима «{mode.Name}» нет ни одного включённого шага.");
                return false;
            }
            if (mode.IsFlowOutdated)
                Debug.LogWarning($"Файл сценария режима «{mode.Name}» изменён после генерации — запускается старая версия.");

            Recorder?.Close();
            foreach (var source in sources)
                source.ResetState();

            CurrentRun = new ScenarioRun(mode, mode.Flow);
            Recorder = new RunRecorder(CurrentRun, loggers);
            RunStarted?.Invoke(CurrentRun);
            CurrentRun.Start();
            return true;
        }

        /// <summary>Прерывает прохождение. Журнал закрывается и остаётся доступен в <see cref="Recorder"/>.</summary>
        public void StopRun()
        {
            Recorder?.Close();
            CurrentRun = null;
        }

        private void OnEnable()
        {
            sources = FindObjectsByType<EventSource>(FindObjectsInactive.Include);
            foreach (var source in sources)
                source.Raised += OnSignal;
        }

        private void OnDisable()
        {
            foreach (var source in sources)
            {
                if (source != null)
                    source.Raised -= OnSignal;
            }
        }

        // Прерванное прохождение тоже сохраняется: анализатор предупредит, что оно не завершено.
        private void OnDestroy() => Recorder?.Close();

        private void OnSignal(ScenarioSignal signal) => CurrentRun?.Handle(signal);
    }
}
