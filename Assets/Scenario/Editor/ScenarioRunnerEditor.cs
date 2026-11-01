using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudyConstructor.Scenario.Editor
{
    [CustomEditor(typeof(ScenarioRunner))]
    public class ScenarioRunnerEditor : UnityEditor.Editor
    {
        private List<(string message, MessageType type)> results = new();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            var runner = (ScenarioRunner)target;
            foreach (var mode in runner.Modes)
            {
                if (mode.Flow == null)
                    EditorGUILayout.HelpBox($"«{mode.Name}»: сценарий ещё не сгенерирован.", MessageType.Warning);
                else if (mode.IsFlowOutdated)
                    EditorGUILayout.HelpBox($"«{mode.Name}»: файл изменён после генерации — сгенерируйте заново.",
                        MessageType.Warning);
            }

            if (GUILayout.Button("Сгенерировать сценарии", GUILayout.Height(30)))
                results = FlowGenerator.Generate(runner);

            foreach (var (message, type) in results)
                EditorGUILayout.HelpBox(message, type);
        }
    }
}
