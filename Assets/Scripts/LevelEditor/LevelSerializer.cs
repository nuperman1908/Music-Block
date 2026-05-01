using System.IO;
using UnityEngine;

public static class LevelSerializer
{
    private static string LevelDir => Path.Combine(Application.dataPath, "levels");
    private static string MusicDir => Path.Combine(Application.dataPath, "music");

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

    public static void Save(LevelInfo level)
    {
        string json = JsonUtility.ToJson(level, prettyPrint: true);
        File.WriteAllText(GetLevelPath(level.id), json);
        Debug.Log($"Saved level {level.id}");
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