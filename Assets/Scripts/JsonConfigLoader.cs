using System;
using System.Reflection;
using UnityEngine;

public static class JsonConfigLoader
{
    public static T LoadFromResources<T>(string resourcePath) where T : class
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
        if (jsonAsset == null)
        {
            Debug.LogError($"Не найден JSON-файл по пути: Resources/{resourcePath}.json");
            return null;
        }

        try
        {
            var ret = JsonUtility.FromJson<T>(jsonAsset.text);
            // Получаем тип объекта
            Type type = ret.GetType();

            // Проверяем наличие поля "name"
            FieldInfo nameField = type.GetField("name", BindingFlags.Public | BindingFlags.Instance);
            if (nameField != null && nameField.FieldType == typeof(string))
            {
                nameField.SetValue(ret, jsonAsset.name);
            }
            return ret;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка парсинга JSON: {e.Message}");
            return null;
        }
    }
}
