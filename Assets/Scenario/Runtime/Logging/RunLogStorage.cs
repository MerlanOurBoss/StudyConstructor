using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace StudyConstructor.Scenario
{
    /// <summary>Сохранение и загрузка журналов прохождений в JSON.</summary>
    public static class RunLogStorage
    {
        public static string Folder => Path.Combine(Application.persistentDataPath, "ScenarioLogs");

        /// <summary>Сохраняет журнал и возвращает путь к файлу или null при ошибке.</summary>
        public static string Save(RunLog log)
        {
            try
            {
                Directory.CreateDirectory(Folder);
                string path = Path.Combine(Folder, $"run_{DateTime.Now:yyyyMMdd_HHmmss_fff}.json");
                File.WriteAllText(path, JsonUtility.ToJson(log, true));
                return path;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Не удалось сохранить журнал: {e.Message}");
                return null;
            }
        }

        /// <summary>Загружает журнал или возвращает null с предупреждением, если файл не читается.</summary>
        public static RunLog Load(string path)
        {
            try
            {
                return JsonUtility.FromJson<RunLog>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Не удалось прочитать журнал {path}: {e.Message}");
                return null;
            }
        }

        /// <summary>Файлы журналов, новые первыми.</summary>
        public static string[] ListFiles()
        {
            return Directory.Exists(Folder)
                ? Directory.GetFiles(Folder, "run_*.json").OrderByDescending(f => f).ToArray()
                : Array.Empty<string>();
        }
    }
}
