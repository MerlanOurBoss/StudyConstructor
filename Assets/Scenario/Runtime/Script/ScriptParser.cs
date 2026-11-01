using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StudyConstructor.Scenario
{
    /// <summary>
    /// Генерация сценария из текста в иерархию объектов (<see cref="ScenarioFlow"/> → <see cref="FlowGroup"/> →
    /// <see cref="FlowBlock"/> → <see cref="FlowStep"/>).
    /// <code>
    /// // комментарий (можно и в конце строки), строки на # и \\ — тоже комментарии
    /// forbid:P_TC_1*#[Опасная зона]*#1*#Сообщение — запрет (значение необязательно); вне групп — на весь сценарий,
    ///                                       в блоке — пока блок активен
    /// start_group:Группа 1                 — группа (необязательно; без групп весь файл — одна группа)
    /// start_parallel                       — всё внутри доступно одновременно
    ///     set_action_number:1.1            — номер действия для следующих шагов блока
    ///     exec:P_PF_3*#[291-VW-M-19]*#1    — механика*#объект*#значение (значение можно не писать)
    ///     start_continuous                 — всё внутри по очереди; блоки вкладываются друг в друга
    ///         exec:P_PF_3*#291-VW-M-20*#0*#if:[291-VW-M-19]=1   — только когда 19 открыта
    ///     stop_continuous
    /// stop_parallel
    /// stop_group
    /// </code>
    /// Объект ищется по имени среди механик сцены. При ошибках ничего не создаётся.
    /// </summary>
    public static class ScriptParser
    {
        private const string ExecPrefix = "exec:";
        private const string ForbidPrefix = "forbid:";
        private const string ConditionPrefix = "if:";
        private const string Separator = "*#";
        private const string GroupStart = "start_group";
        private const string ActionNumber = "set_action_number:";
        private const string ActionName = "set_action_name:";

        // Команды формата NPZ, которые пока не реализованы и на порядок шагов не влияют: пропускаются с предупреждением.
        private static readonly string[] SkippedCommands =
            { "goto:", "set_step_number:", "set_operation_number:", "set_operation_name:" };

        // Блочные команды NPZ: без их поддержки сценарий исполнялся бы неверно, поэтому это ошибка.
        private static readonly string[] UnsupportedBlocks =
        {
            "start_condition:", "stop_condition", "start_true", "stop_true", "start_false", "stop_false",
            "start_global_check:", "stop_global_check"
        };

        /// <summary>Открытый блок или группа: куда добавлять шаги, какой номер/название им давать и что запрещено.</summary>
        private class Frame
        {
            public readonly Transform Node;
            public readonly string Kind;
            public readonly int Line;
            public readonly List<ViolationRule> Forbids = new();
            public string Number;
            public string Name;

            public Frame(Transform node, string kind, int line, Frame parent)
            {
                Node = node;
                Kind = kind;
                Line = line;
                Number = parent?.Number;
                Name = parent?.Name;
            }
        }

        /// <summary>Создаёт иерархию сценария. Если есть ошибки, возвращает null и ничего не оставляет на сцене.</summary>
        public static ScenarioFlow Build(string text, string flowName, IReadOnlyCollection<EventSource> sources,
            List<string> errors, List<string> warnings)
        {
            int errorsBefore = errors.Count;
            var flow = new GameObject(flowName).AddComponent<ScenarioFlow>();
            flow.SetSource(text);

            var blocks = new Stack<Frame>();
            var globalForbids = new List<ViolationRule>();
            var ids = new Dictionary<string, int>();
            string[] lines = (text ?? "").Split('\n');
            bool usesGroups = lines.Any(l => IsGroupStart(StripComment(l)));
            Frame group = null;
            Frame wholeFile = null;

            Frame Current(int n)
            {
                if (blocks.Count > 0)
                    return blocks.Peek();
                if (group != null)
                    return group;
                if (usesGroups)
                {
                    errors.Add(Error(n, "команда вне группы: в файле с группами всё должно быть между start_group и stop_group"));
                    return null;
                }
                return wholeFile ??= new Frame(CreateNode<FlowGroup>(flow.transform, "Все шаги").transform, "group", 0, null);
            }

            void CloseBlocks()
            {
                foreach (var frame in blocks)
                {
                    errors.Add(Error(frame.Line, $"start_{frame.Kind} не закрыт (нет stop_{frame.Kind})"));
                    ApplyForbids(frame);
                }
                blocks.Clear();
            }

            for (int i = 0; i < lines.Length; i++)
            {
                int n = i + 1;
                string code = StripComment(lines[i]);
                if (code.Length == 0)
                    continue;

                if (IsGroupStart(code))
                {
                    if (blocks.Count > 0)
                        errors.Add(Error(n, "start_group нельзя ставить внутри блока"));
                    else if (group != null)
                        errors.Add(Error(n, $"группа «{group.Node.name}» не закрыта (нет stop_group)"));
                    else
                    {
                        string name = code.Length > GroupStart.Length ? code.Substring(GroupStart.Length + 1).Trim() : "";
                        int index = flow.transform.childCount + 1;
                        var node = CreateNode<FlowGroup>(flow.transform, name.Length > 0 ? name : $"Группа {index}");
                        group = new Frame(node.transform, "group", n, null);
                    }
                    continue;
                }

                switch (code)
                {
                    case "start_parallel":
                    case "start_continuous":
                        var parent = Current(n);
                        if (parent == null)
                            break;
                        bool parallel = code == "start_parallel";
                        var block = CreateNode<FlowBlock>(parent.Node, parallel ? "Одновременно" : "По очереди");
                        block.Setup(parallel);
                        blocks.Push(new Frame(block.transform, code.Substring("start_".Length), n, parent));
                        break;

                    case "stop_parallel":
                    case "stop_continuous":
                        string kind = code.Substring("stop_".Length);
                        if (blocks.Count == 0)
                            errors.Add(Error(n, $"{code} без start_{kind}"));
                        else if (blocks.Peek().Kind != kind)
                            errors.Add(Error(n, $"{code}, а открыт start_{blocks.Peek().Kind} (строка {blocks.Peek().Line})"));
                        else
                            ApplyForbids(blocks.Pop());
                        break;

                    case "stop_group":
                        if (group == null)
                            errors.Add(Error(n, "stop_group без start_group"));
                        CloseBlocks();
                        if (group != null)
                            ApplyForbids(group);
                        group = null;
                        break;

                    default:
                        if (code.StartsWith(ExecPrefix))
                        {
                            var target = Current(n);
                            if (target != null)
                                CreateStep(code, n, target, sources, ids, errors);
                        }
                        else if (code.StartsWith(ForbidPrefix))
                        {
                            var rule = ParseForbid(code, n, sources, errors);
                            if (rule != null)
                                (blocks.Count > 0 ? blocks.Peek().Forbids : group?.Forbids ?? globalForbids).Add(rule);
                        }
                        else if (code.StartsWith(ActionNumber) || code.StartsWith(ActionName))
                        {
                            var target = Current(n);
                            if (target == null)
                                break;
                            if (code.StartsWith(ActionNumber))
                                target.Number = code.Substring(ActionNumber.Length).Trim();
                            else
                                target.Name = code.Substring(ActionName.Length).Trim();
                        }
                        else if (UnsupportedBlocks.Any(code.StartsWith))
                            errors.Add(Error(n, $"команда «{code}» пока не поддерживается"));
                        else if (SkippedCommands.Any(code.StartsWith))
                            warnings.Add(Error(n, $"команда «{code}» пока не поддерживается и пропущена"));
                        else
                            errors.Add(Error(n, $"неизвестная команда «{code}»"));
                        break;
                }
            }

            CloseBlocks();
            if (group != null)
                errors.Add($"Группа «{group.Node.name}» не закрыта (нет stop_group в конце файла)");
            if (wholeFile != null)
                ApplyForbids(wholeFile);

            var steps = flow.GetComponentsInChildren<FlowStep>(true);
            if (steps.Length == 0)
                errors.Add("В сценарии нет ни одного шага exec.");
            foreach (var step in steps)
            foreach (var rule in globalForbids)
                step.AddViolation(rule);

            if (errors.Count == errorsBefore)
                return flow;

            Object.DestroyImmediate(flow.gameObject);
            return null;
        }

        /// <summary>exec:механика*#объект[*#значение][*#if:[объект]=значение]</summary>
        private static void CreateStep(string code, int n, Frame frame, IReadOnlyCollection<EventSource> sources,
            Dictionary<string, int> ids, List<string> errors)
        {
            string[] parts = Split(code, ExecPrefix);
            if (parts.Length < 2)
            {
                errors.Add(Error(n, "ожидается exec:<механика>*#[<объект>]*#<значение>"));
                return;
            }

            var source = FindSource(parts[0], parts[1], n, sources, errors);
            if (source == null)
                return;

            string value = null;
            string conditionText = null;
            foreach (string part in parts.Skip(2))
            {
                if (part.StartsWith(ConditionPrefix) && conditionText == null)
                    conditionText = part.Substring(ConditionPrefix.Length).Trim();
                else if (value == null && !part.StartsWith(ConditionPrefix))
                    value = part;
                else
                {
                    errors.Add(Error(n, $"лишний параметр «{part}»"));
                    return;
                }
            }

            value ??= source.DefaultValue;
            if (!source.IsValidValue(value))
            {
                errors.Add(Error(n, $"значение «{value}» не подходит для механики {source.Mechanic}"));
                return;
            }

            EventSource conditionSource = null;
            string conditionValue = null;
            if (conditionText != null && !TryParseCondition(conditionText, n, sources, errors, out conditionSource, out conditionValue))
                return;

            // Одно и то же действие может встречаться в сценарии несколько раз — id должен быть уникальным.
            string id = $"{source.Mechanic}[{source.name}]={value}";
            ids[id] = ids.TryGetValue(id, out int count) ? count + 1 : 1;
            if (ids[id] > 1)
                id += $"#{ids[id]}";

            string title = source.Describe(value);
            if (conditionSource != null)
                title += $" (когда {conditionSource.DescribeState(conditionValue)})";
            if (!string.IsNullOrEmpty(frame.Name))
                title = $"{frame.Name}: {title}";
            if (!string.IsNullOrEmpty(frame.Number))
                title = $"{frame.Number} {title}";

            var condition = new ActionCondition(source, value, conditionSource, conditionValue);
            CreateNode<FlowStep>(frame.Node, title).Setup(id, title, n, condition);
        }

        /// <summary>forbid:механика*#объект[*#значение][*#сообщение] — без значения запрещено любое действие.</summary>
        private static ViolationRule ParseForbid(string code, int n, IReadOnlyCollection<EventSource> sources, List<string> errors)
        {
            string[] parts = Split(code, ForbidPrefix);
            if (parts.Length < 2 || parts.Length > 4)
            {
                errors.Add(Error(n, "ожидается forbid:<механика>*#[<объект>]*#<значение>*#<сообщение>"));
                return null;
            }

            var source = FindSource(parts[0], parts[1], n, sources, errors);
            if (source == null)
                return null;

            var rest = parts.Skip(2).ToList();
            string value = rest.Count > 0 && source.IsValidValue(rest[0]) ? rest[0] : null;
            if (value != null)
                rest.RemoveAt(0);
            if (rest.Count > 1)
            {
                errors.Add(Error(n, $"значение «{rest[0]}» не подходит для механики {source.Mechanic}"));
                return null;
            }

            string message = rest.Count > 0 && rest[0].Length > 0
                ? rest[0]
                : $"Запрещено: {(value != null ? source.Describe(value) : $"любое действие с объектом {source.name}")}";
            return new ViolationRule(source, value, message);
        }

        /// <summary>Условие вида [объект]=значение: объект должен быть механикой с состоянием.</summary>
        private static bool TryParseCondition(string text, int n, IReadOnlyCollection<EventSource> sources,
            List<string> errors, out EventSource source, out string value)
        {
            source = null;
            value = null;
            int equals = text.LastIndexOf('=');
            if (equals <= 0)
            {
                errors.Add(Error(n, $"условие «if:{text}» должно выглядеть так: if:[<объект>]=<значение>"));
                return false;
            }

            string objectName = Unbracket(text.Substring(0, equals).Trim());
            value = text.Substring(equals + 1).Trim();
            var matches = sources.Where(s => s != null && s.name == objectName && s.CurrentValue != null).ToList();
            if (matches.Count != 1)
            {
                errors.Add(Error(n, matches.Count == 0
                    ? $"для условия нужен объект [{objectName}] с состоянием (например, заглушка) — на сцене такого нет"
                    : $"на сцене несколько объектов [{objectName}] с состоянием — имена должны быть уникальными"));
                return false;
            }

            source = matches[0];
            if (source.IsValidValue(value))
                return true;

            errors.Add(Error(n, $"значение «{value}» в условии не подходит для механики {source.Mechanic}"));
            return false;
        }

        private static EventSource FindSource(string mechanicText, string objectText, int n,
            IReadOnlyCollection<EventSource> sources, List<string> errors)
        {
            string mechanic = mechanicText.Trim();
            string objectName = Unbracket(objectText.Trim());
            var matches = sources.Where(s => s != null && s.Mechanic == mechanic && s.name == objectName).ToList();
            if (matches.Count == 1)
                return matches[0];

            errors.Add(Error(n, matches.Count > 1
                ? $"на сцене несколько объектов [{objectName}] с механикой {mechanic} — имена должны быть уникальными"
                : sources.Any(s => s != null && s.Mechanic == mechanic)
                    ? $"на сцене нет объекта [{objectName}] с механикой {mechanic}"
                    : $"механика {mechanic} не найдена ни на одном объекте сцены"));
            return null;
        }

        private static void ApplyForbids(Frame frame)
        {
            if (frame.Forbids.Count == 0)
                return;

            foreach (var step in frame.Node.GetComponentsInChildren<FlowStep>(true))
            foreach (var rule in frame.Forbids)
                step.AddViolation(rule);
        }

        private static string[] Split(string code, string prefix) =>
            code.Substring(prefix.Length).Split(new[] { Separator }, StringSplitOptions.None).Select(p => p.Trim()).ToArray();

        private static T CreateNode<T>(Transform parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        /// <summary>Убирает комментарии: строки на # и \\, а также всё после //.</summary>
        private static string StripComment(string line)
        {
            string code = line.Trim();
            if (code.StartsWith("#") || code.StartsWith("\\\\"))
                return "";

            int comment = code.IndexOf("//", StringComparison.Ordinal);
            return comment >= 0 ? code.Substring(0, comment).Trim() : code;
        }

        private static string Unbracket(string value) =>
            value.Length >= 2 && value[0] == '[' && value[value.Length - 1] == ']'
                ? value.Substring(1, value.Length - 2).Trim()
                : value;

        private static bool IsGroupStart(string code) => code == GroupStart || code.StartsWith(GroupStart + ":");

        private static string Error(int line, string message) => $"Строка {line}: {message}";
    }
}
