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
            return JsonUtility.FromJson<T>(jsonAsset.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка парсинга JSON: {e.Message}");
            return null;
        }
    }
}
