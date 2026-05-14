using System.IO;
using UnityEngine;

public static class LevelSerializer
{
    private static string LevelDir => Path.Combine(Application.persistentDataPath, "levels");
    private static string MusicDir => Path.Combine(Application.persistentDataPath, "music");

    private static string GetLevelPath(int levelId)
    {
        Directory.CreateDirectory(LevelDir);
        return Path.Combine(LevelDir, $"level_{levelId}.json");
    }

    public static string GetMusicPath(string musicFileName)
    {
        Directory.CreateDirectory(MusicDir);
        return Path.Combine(MusicDir, musicFileName);
    }

    public static string SaveMusicFile(string sourcePath)
    {
        if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
        {
            Debug.LogWarning($"Music source not found: {sourcePath}");
            return null;
        }
        Directory.CreateDirectory(MusicDir);
        string fileName = Path.GetFileName(sourcePath);
        string destPath = Path.Combine(MusicDir, fileName);
        if (Path.GetFullPath(sourcePath) != Path.GetFullPath(destPath))
        {
            File.Copy(sourcePath, destPath, overwrite: true);
        }
        Debug.Log($"Saved music to {destPath}");
        return fileName;
    }

    public static void Save(LevelInfo level)
    {
        string json = JsonUtility.ToJson(level, prettyPrint: true);
        File.WriteAllText(GetLevelPath(level.id), json);
        Debug.Log($"Saved level {level.id} to {GetLevelPath(level.id)}");
    }

    public static LevelInfo Load(int levelId)
    {
        string path = GetLevelPath(levelId);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Level {levelId} not found");
            return null;
        }
        return JsonUtility.FromJson<LevelInfo>(File.ReadAllText(path));
    }

    public static bool Delete(int levelId)
    {
        string path = GetLevelPath(levelId);
        if (!File.Exists(path)) return false;
        File.Delete(path);
        return true;
    }
}
