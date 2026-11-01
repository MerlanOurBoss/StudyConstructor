using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StudyConstructor.Scenario.Demo
{
    public class ScenarioHud : MonoBehaviour
    {
        [SerializeField] private ScenarioRunner runner;

        [Tooltip("Сколько секунд показывается сообщение о нарушении.")]
        [SerializeField] private float messageSeconds = 3f;

        [Tooltip("Вызывается перед каждым прохождением: верните сцену в исходное состояние (например, игрока на старт). " +
                 "Механики сценария сбрасываются автоматически.")]
        [SerializeField] private UnityEvent onRunStarting = new();

        [Header("Панели")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject tasksPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject messagePanel;

        [Header("Тексты")]
        [Tooltip("Ошибки запуска сценария в меню.")]
        [SerializeField] private Text menuErrors;
        [SerializeField] private Text tasksHeader;
        [SerializeField] private Text tasksText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text resultsHeader;
        [SerializeField] private Text resultsText;

        [Header("Результат")]
        [Tooltip("Кнопки анализаторов по порядку режимов: N-я кнопка вызывает SelectAnalyzer(N). Подпись ставится сама.")]
        [SerializeField] private List<Button> analyzerButtons = new();

        [Tooltip("Строка кнопок листания сохранённых журналов: видна только при просмотре файла.")]
        [SerializeField] private GameObject fileButtons;

        [SerializeField] private Color buttonColor = new(0.24f, 0.32f, 0.46f);
        [SerializeField] private Color selectedColor = new(0.2f, 0.55f, 0.35f);

        private float hideMessageAt;
        private int lastModeIndex = -1;
        private string currentGroup;
        private RunLog shownLog;
        private ResultAnalyzer shownAnalyzer;
        private bool showJournal;
        private string[] files = System.Array.Empty<string>();
        private int fileIndex = -1;

        private void Awake()
        {
            messagePanel.SetActive(false);
            menuErrors.gameObject.SetActive(false);
            ShowMenu();
        }

        private void OnEnable() => runner.RunStarted += OnRunStarted;
        private void OnDisable() => runner.RunStarted -= OnRunStarted;

        private void Update()
        {
            if (messagePanel.activeSelf && Time.unscaledTime > hideMessageAt)
                messagePanel.SetActive(false);
        }
        public void StartMode(int index)
        {
            onRunStarting.Invoke();
            lastModeIndex = index;
            if (runner.TryStartRun(index, out var errors))
                return;

            menuErrors.text = $"Сценарий «{runner.Modes[index].Name}» не запущен:\n{string.Join("\n", errors)}";
            menuErrors.gameObject.SetActive(true);
            ShowMenu();
        }

        public void StopEarly()
        {
            var run = runner.CurrentRun;
            if (run == null)
                return;

            runner.StopRun();
            ShowResults(runner.Recorder.Log, run.Mode.Analyzer, fromFile: false);
        }

        public void SelectAnalyzer(int index)
        {
            shownAnalyzer = runner.Analyzers.ElementAtOrDefault(index);
            showJournal = false;
            RenderResults();
        }

        public void ShowJournal()
        {
            showJournal = true;
            RenderResults();
        }

        public void OpenSavedLogs()
        {
            files = RunLogStorage.ListFiles();
            if (files.Length == 0)
            {
                ShowMessage("Сохранённых журналов нет. Пройдите сценарий хотя бы один раз.");
                return;
            }
            OpenFile(0);
        }

        public void OpenNewerLog() => OpenFile(fileIndex - 1);
        public void OpenOlderLog() => OpenFile(fileIndex + 1);

        public void Restart()
        {
            int index = lastModeIndex;
            if (fileIndex >= 0)
                index = runner.Modes.ToList().FindIndex(m => m.Name == shownLog.mode);
            if (index >= 0)
                StartMode(index);
            else
                ShowMenu();
        }

        public void ShowMenu() => ShowOnly(menuPanel);
        private void OnRunStarted(ScenarioRun run)
        {
            menuErrors.gameObject.SetActive(false);
            currentGroup = null;
            ShowOnly(tasksPanel);
            run.GroupStarted += group =>
            {
                currentGroup = group.Name;
                RefreshTasks(run);
            };
            run.StepChanged += _ => RefreshTasks(run);
            run.ViolationRegistered += (step, reason) =>
            {
                ShowMessage($"Нарушение: {reason}");
                RefreshTasks(run);
            };
            run.Finished += () => ShowResults(runner.Recorder.Log, run.Mode.Analyzer, fromFile: false);
            RefreshTasks(run);
        }
        private void RefreshTasks(ScenarioRun run)
        {
            tasksHeader.text = string.IsNullOrEmpty(currentGroup) ? run.Mode.Name : $"{run.Mode.Name} — {currentGroup}";

            var text = new StringBuilder();
            foreach (var step in run.ActiveSteps)
            {
                text.Append("• ").Append(step.Step.Title);
                if (step.Violations > 0)
                    text.Append($"  <color=#ffb060>(нарушений: {step.Violations})</color>");
                text.AppendLine();
            }

            int done = run.Steps.Count(s => s.IsFinished);
            text.AppendLine().Append($"<color=#9aa4b5>Завершено шагов: {done} из {run.Steps.Count}</color>");
            tasksText.text = text.ToString();
        }

        private void ShowMessage(string message)
        {
            messageText.text = message;
            messagePanel.SetActive(true);
            hideMessageAt = Time.unscaledTime + messageSeconds;
        }

        private void ShowResults(RunLog log, ResultAnalyzer analyzer, bool fromFile)
        {
            shownLog = log;
            shownAnalyzer = analyzer ?? runner.Analyzers.FirstOrDefault();
            showJournal = false;
            fileButtons.SetActive(fromFile);
            if (!fromFile)
                fileIndex = -1;

            var analyzers = runner.Analyzers.ToList();
            for (int i = 0; i < analyzerButtons.Count; i++)
            {
                bool exists = i < analyzers.Count;
                analyzerButtons[i].gameObject.SetActive(exists);
                if (exists)
                    analyzerButtons[i].GetComponentInChildren<Text>().text = analyzers[i].DisplayName;
            }

            ShowOnly(resultsPanel);
            RenderResults();
        }

        private void RenderResults()
        {
            string source = fileIndex >= 0 ? $"Файл: {Path.GetFileName(files[fileIndex])}" : "Текущее прохождение";
            resultsHeader.text = $"<b>{shownLog.mode}</b>   {shownLog.startedAt}\n<size=18><color=#9aa4b5>{source}</color></size>";
            resultsText.text = showJournal ? FormatJournal(shownLog) : FormatReport();
            resultsText.rectTransform.anchoredPosition = new Vector2(0f, -10f);

            var analyzers = runner.Analyzers.ToList();
            for (int i = 0; i < analyzerButtons.Count && i < analyzers.Count; i++)
            {
                bool selected = analyzers[i] == shownAnalyzer && !showJournal;
                analyzerButtons[i].GetComponent<Image>().color = selected ? selectedColor : buttonColor;
            }
        }

        private string FormatReport()
        {
            if (shownAnalyzer == null)
                return "Анализатор не выбран. Укажите его в настройках режима (ScenarioRunner → Modes → Analyzer).";

            var report = shownAnalyzer.Analyze(shownLog);
            var text = new StringBuilder();
            text.AppendLine($"<b>{report.AnalyzerName}</b>");
            text.AppendLine($"<size=30><b>Итог: {report.Score} из {report.MaxScore}</b></size>").AppendLine();

            string group = null;
            foreach (var step in report.Steps)
            {
                if (step.Group != group)
                {
                    group = step.Group;
                    text.AppendLine($"<b>{group ?? "Без группы"}</b>");
                }
                text.AppendLine($"   <color={StatusColor(step.Status)}>{step.Status,-10}</color> {step.Title}" +
                                $"   <color=#9aa4b5>{step.Detail}</color>   {step.Points:+0;-0;0}");
            }

            if (report.Warnings.Count > 0)
            {
                text.AppendLine().AppendLine("<color=#ffcc66><b>Предупреждения</b></color>");
                foreach (var warning in report.Warnings)
                    text.AppendLine($"<color=#ffcc66>• {warning}</color>");
            }
            return text.ToString();
        }

        private static string FormatJournal(RunLog log)
        {
            var text = new StringBuilder("<b>Журнал событий</b>\n\n");
            foreach (var entry in log.entries)
            {
                text.Append($"{entry.time,7:0.0} с   <b>{entry.type}</b>   {entry.stepId}   {entry.result}");
                foreach (var field in entry.data)
                    text.Append($"   <color=#9aa4b5>{field.key}={field.value}</color>");
                text.AppendLine();
            }
            return text.ToString();
        }

        private static string StatusColor(StepStatus status) => status switch
        {
            StepStatus.Completed => "#6fdc8c",
            StepStatus.Failed => "#ff6b6b",
            StepStatus.Skipped => "#b0b0b0",
            _ => "#ffcc66"
        };

        private void OpenFile(int index)
        {
            if (files.Length == 0)
                return;

            fileIndex = Mathf.Clamp(index, 0, files.Length - 1);
            var log = RunLogStorage.Load(files[fileIndex]);
            if (log == null)
            {
                ShowMessage($"Не удалось прочитать {Path.GetFileName(files[fileIndex])}");
                return;
            }

            var analyzer = runner.Modes.FirstOrDefault(m => m.Name == log.mode)?.Analyzer;
            ShowResults(log, analyzer, fromFile: true);
        }

        private void ShowOnly(GameObject panel)
        {
            menuPanel.SetActive(panel == menuPanel);
            tasksPanel.SetActive(panel == tasksPanel);
            resultsPanel.SetActive(panel == resultsPanel);
        }
    }
}
