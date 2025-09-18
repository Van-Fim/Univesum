using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FolderLister : MonoBehaviour
{
    public static List<string> GetDirDirs(string dir)
    {
        List<string> ret = new List<string>();
        string path = Application.dataPath + "/Resources/" + dir; // Замените на нужный путь
        DirectoryInfo dirInfo = new DirectoryInfo(path);

        if (dirInfo.Exists)
        {
            DirectoryInfo[] folders = dirInfo.GetDirectories();
            foreach (DirectoryInfo folder in folders)
            {
                ret.Add(folder.Name);
            }
        }
        else
        {
            Debug.LogWarning("Путь не найден: " + path);
        }
        return ret;
    }
}
