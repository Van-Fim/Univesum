using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;

public class CopyConfigsPostBuild
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("=== POST PROCESS BUILD STARTED ===");
        Debug.Log("Build target: " + target);
        Debug.Log("Built project path: " + pathToBuiltProject);

        string buildDir = Path.GetDirectoryName(pathToBuiltProject);
        Debug.Log("Build directory: " + buildDir);
        string executableDirectory = Path.GetDirectoryName(Application.dataPath);
        string source = Path.Combine(executableDirectory, "Configs");
        source = source.Replace('/', Path.DirectorySeparatorChar);

        string dest = Path.Combine(buildDir, "Configs");
        Debug.Log("Destination folder: " + dest);

        if (!Directory.Exists(source))
        {
            Debug.LogError($"SOURCE {source} FOLDER DOES NOT EXIST!");
            return;
        }

        Debug.Log("Source folder exists.");

        if (Directory.Exists(dest))
        {
            Debug.Log("Destination exists, deleting...");
            Directory.Delete(dest, true);
        }

        Debug.Log("Copying...");
        DirectoryCopy(source, dest, true);

        Debug.Log("=== POST PROCESS BUILD FINISHED ===");
    }

    static void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs)
    {
        Debug.Log("DirectoryCopy: " + sourceDir + " -> " + destDir);

        DirectoryInfo dir = new DirectoryInfo(sourceDir);
        DirectoryInfo[] dirs = dir.GetDirectories();

        Directory.CreateDirectory(destDir);
        Debug.Log("Created dest dir");

        foreach (FileInfo file in dir.GetFiles())
        {
            Debug.Log("Copy file: " + file.FullName);
            file.CopyTo(Path.Combine(destDir, file.Name), true);
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                Debug.Log("Copy subfolder: " + subdir.FullName);
                DirectoryCopy(subdir.FullName, Path.Combine(destDir, subdir.Name), true);
            }
        }
    }
}
