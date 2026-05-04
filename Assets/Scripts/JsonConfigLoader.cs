using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class JsonConfigLoader
{
    // Получаем путь к папке, где находится .exe файл
    public static string ConfigPath
    {
        get
        {
            // Application.dataPath для standalone билда указывает на папку с .exe
            // В Windows: путь/к/игре_Data
            // Поэтому поднимаемся на уровень выше для папки с .exe
            string dataPath = Application.dataPath;
            
            #if UNITY_STANDALONE_WIN
                // Для Windows: удаляем "_Data" из пути
                string executableDirectory = Path.GetDirectoryName(dataPath);
                return Path.Combine(executableDirectory, "Configs");
            #elif UNITY_EDITOR
                // В редакторе - папка Assets/Configs
                return Path.Combine(Application.dataPath, "Configs");
            #else
                // Для других платформ - просто рядом с .exe
                return Path.Combine(Path.GetDirectoryName(Application.dataPath), "Configs");
            #endif
        }
    }
    
    /// <summary>
    /// Загружает JSON из папки Configs рядом с .exe
    /// </summary>
    public static T LoadFromFile<T>(string fileName) where T : class
    {
        try
        {
            fileName = fileName.Replace('/', Path.DirectorySeparatorChar);

            // Убеждаемся, что файл имеет расширение .json
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";
            
            string fullPath = Path.Combine(ConfigPath, fileName);
            
            if (!File.Exists(fullPath))
            {
                // Debug.LogError($"JSON файл не найден: {fullPath}");
                return null;
            }
            
            string jsonContent = File.ReadAllText(fullPath);
            
            var result = JsonUtility.FromJson<T>(jsonContent);
            
            if (result == null)
            {
                Debug.LogError($"Не удалось десериализовать JSON из файла: {fullPath}");
                return null;
            }
            
            // Устанавливаем имя файла как name, если поле существует
            Type type = result.GetType();
            FieldInfo nameField = type.GetField("name", BindingFlags.Public | BindingFlags.Instance);
            if (nameField != null && nameField.FieldType == typeof(string))
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                nameField.SetValue(result, fileNameWithoutExt);
            }
            
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка загрузки JSON файла {fileName}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Загружает все JSON файлы из папки конфигов
    /// </summary>
    public static T[] LoadAllFromFolder<T>(string subFolder = null) where T : class
    {
        try
        {
            string targetPath = ConfigPath;
            if (!string.IsNullOrEmpty(subFolder))
                targetPath = Path.Combine(ConfigPath, subFolder);
            
            if (!Directory.Exists(targetPath))
            {
                Debug.LogWarning($"Папка конфигов не существует: {targetPath}");
                return new T[0];
            }
            
            var jsonFiles = Directory.GetFiles(targetPath, "*.json");
            var results = new System.Collections.Generic.List<T>();
            
            foreach (var filePath in jsonFiles)
            {
                var config = LoadFromFile<T>(filePath);
                if (config != null)
                    results.Add(config);
            }
            
            return results.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка загрузки конфигов из папки: {e.Message}");
            return new T[0];
        }
    }
    
    /// <summary>
    /// Сохраняет объект в JSON файл
    /// </summary>
    public static bool SaveToFile<T>(T data, string fileName) where T : class
    {
        try
        {
            fileName = fileName.Replace('/', Path.DirectorySeparatorChar);
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                fileName += ".json";
            
            string fullPath = Path.Combine(ConfigPath, fileName);
            string directory = Path.GetDirectoryName(fullPath);
            
            // Создаем папку, если её нет
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            string json = JsonUtility.ToJson(data, true); // true = pretty print
            File.WriteAllText(fullPath, json);
            
            Debug.Log($"Сохранен конфиг: {fullPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка сохранения JSON файла {fileName}: {e.Message}");
            return false;
        }
    }
}