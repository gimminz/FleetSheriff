using System.IO;
using UnityEngine;

public static class SavePath
{
    public static string SaveDir => Path.Combine(Application.persistentDataPath, "Save");

    public static string LoadoutJsonPath => Path.Combine(SaveDir, "Loadout.json");

    public static void EnsureDir()
    {
        if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);
    }
}
