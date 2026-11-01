using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StudyConstructor.Scenario.Editor
{
    public static class FlowGenerator
    {
        public static List<(string message, MessageType type)> Generate(ScenarioRunner runner)
        {
            var results = new List<(string, MessageType)>();
            var sources = Object.FindObjectsByType<EventSource>(FindObjectsInactive.Include);
            var oldFlows = runner.Modes.Select(m => m.Flow).Where(f => f != null).Distinct().ToList();
            Undo.SetCurrentGroupName("Генерация сценариев");
            Undo.RecordObject(runner, "Генерация сценариев");

            foreach (var mode in runner.Modes.Where(m => m.Script == null))
                results.Add(($"«{mode.Name}»: не указан файл сценария (Script).", MessageType.Error));

            foreach (var file in runner.Modes.Where(m => m.Script != null).GroupBy(m => m.Script))
            {
                string modes = string.Join(", ", file.Select(m => $"«{m.Name}»"));
                var errors = new List<string>();
                var warnings = new List<string>();
                var flow = ScriptParser.Build(file.Key.text, file.Key.name, sources, errors, warnings);
                if (flow == null)
                {
                    results.Add(($"{file.Key.name} ({modes}): есть ошибки, сценарий не изменён\n{string.Join("\n", errors)}",
                        MessageType.Error));
                    continue;
                }

                flow.transform.SetParent(runner.transform, false);
                Undo.RegisterCreatedObjectUndo(flow.gameObject, "Генерация сценария");
                foreach (var mode in file)
                    mode.SetFlow(flow);

                var groups = flow.Groups().Select(g => $"  {g.name}: шагов {g.Steps().Count()}");
                string report = $"{file.Key.name} ({modes}): сгенерирован\n{string.Join("\n", groups)}";
                if (warnings.Count > 0)
                    report += $"\nПредупреждения:\n{string.Join("\n", warnings)}";
                results.Add((report, warnings.Count > 0 ? MessageType.Warning : MessageType.Info));
            }

            foreach (var old in oldFlows.Where(f => runner.Modes.All(m => m.Flow != f)))
                Undo.DestroyObjectImmediate(old.gameObject);

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(runner.gameObject.scene);
            return results;
        }
    }
}
