using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LevelGenerator : MonoBehaviour
{
    public enum GenerationMode { Editor, MainLevel, CustomLevel }

    [Header("Mode")]
    public GenerationMode mode = GenerationMode.Editor;
    [Tooltip("Editor mode only. False = Create new (no level loaded). True = Edit existing level from persistent storage.")]
    public bool editExisting = false;

    [Header("Spawn")]
    public Transform container;

    public AudioSource musicSource;

    [Header("Challenge Mode")]
    public List<Transform> checkPoints = new List<Transform>();

    public void SpawnCheckpoints(GameObject checkPointPrefab)
    {
        if (checkPointPrefab == null) return;
        foreach (Transform cp in checkPoints)
        {
            if (cp == null) continue;
            Instantiate(checkPointPrefab, cp.position, Quaternion.identity);
        }
    }

    [Header("Prefab Lookup")]
    public List<GameObject> mapPrefabs = new List<GameObject>();
    [Tooltip("CustomLevel mode only. Replaces the editor's endpoint prefab with the gameplay version.")]
    public GameObject endPointGameplayPrefab;

    [Header("Auto Load")]
    public string levelToLoad;
    public bool loadOnStart;

    public AudioClip loadedMusic { get; private set; }
    public LevelInfo currentLevel { get; private set; }

    private void Start()
    {
        loadOnStart = PlayerPrefs.GetInt("LoadOnStart") == 1 ? true : false;
        editExisting = loadOnStart;
        if (!loadOnStart) return;
        Generate();
    }

    public void Generate()
    {
        levelToLoad = PlayerPrefs.GetString("levelToLoad");
        Generate(levelToLoad);
    }

    public void Generate(string levelFileName)
    {
        levelToLoad = levelFileName;
        switch (mode)
        {
            case GenerationMode.Editor:
                if (!editExisting) return;
                LoadFromPersistent(levelFileName);
                break;
            case GenerationMode.MainLevel:
                LoadFromResources(levelFileName);
                break;
            case GenerationMode.CustomLevel:
                LoadFromPersistent(levelFileName);
                break;
        }

    }

    public void InitLevel(string levelFileName)
    {
        Generate(levelFileName);
    }

    private void LoadFromResources(string levelFileName)
    {
        if (!EnsureContainer()) return;
        if (string.IsNullOrEmpty(levelFileName)) return;

        string resourcePath = "Level/" + Path.GetFileNameWithoutExtension(levelFileName);
        TextAsset json = Resources.Load<TextAsset>(resourcePath);
        if (json == null)
        {
            Debug.LogError($"Level JSON not found at Resources/{resourcePath}.json");
            return;
        }

        LevelInfo level = JsonUtility.FromJson<LevelInfo>(json.text);
        if (level == null)
        {
            Debug.LogError("Failed to parse level JSON.");
            return;
        }

        BuildLevel(level);
        LoadMusicFromResources(level.music);
        musicSource.clip = loadedMusic;
    }

    private void LoadFromPersistent(string levelFileName)
    {
        if (!EnsureContainer()) return;
        if (string.IsNullOrEmpty(levelFileName)) return;

        string path = FindPersistentLevelPath(levelFileName);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogError($"Custom level not found in persistent storage: {levelFileName}");
            return;
        }

        LevelInfo level = JsonUtility.FromJson<LevelInfo>(File.ReadAllText(path));
        if (level == null)
        {
            Debug.LogError("Failed to parse level JSON.");
            return;
        }

        BuildLevel(level);
        LoadMusicFromPersistent(level.music);
    }

    private static string FindPersistentLevelPath(string identifier)
    {
        string dir = Path.Combine(Application.persistentDataPath, "levels");
        if (!Directory.Exists(dir)) return null;

        string asFile = Path.Combine(dir, Path.GetFileNameWithoutExtension(identifier) + ".json");
        if (File.Exists(asFile)) return asFile;

        foreach (string file in Directory.GetFiles(dir, "*.json"))
        {
            LevelInfo info = JsonUtility.FromJson<LevelInfo>(File.ReadAllText(file));
            if (info != null && info.name == identifier) return file;
        }
        return null;
    }

    private bool EnsureContainer()
    {
        if (container == null)
        {
            Debug.LogError("Container is not assigned.");
            return false;
        }
        return true;
    }

    private void BuildLevel(LevelInfo level)
    {
        currentLevel = level;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }

        if (mode == GenerationMode.Editor)
        {
            ObjSelectionButton.ClearPlaced();
            LevelEditorManager mgr = LevelEditorManager.Instance;
            if (mgr != null)
            {
                if (mgr.levelName != null) mgr.levelName.text = level.name;
                if (mgr.startTime != null) mgr.startTime.text = level.startTime.ToString("0.00");
                if (mgr.musicText != null) mgr.musicText.text = level.music;
            }
        }

        if (level.items != null)
        {
            foreach (Item item in level.items)
            {
                GameObject prefab = ResolvePrefab(item.prefabName);
                if (prefab == null)
                {
                    Debug.LogWarning($"Prefab not found: {item.prefabName}");
                    continue;
                }
                GameObject obj = Instantiate(prefab,
                    new Vector3(item.x, item.y, 0f),
                    Quaternion.Euler(0f, 0f, item.zRotate),
                    container);
                ApplyAlpha(obj.transform, item.alpha);
                if (mode == GenerationMode.Editor) ObjSelectionButton.RegisterPlaced(obj);
            }
        }

        Debug.Log($"Loaded level '{level.name}' with {(level.items != null ? level.items.Count : 0)} items.");
    }

    private GameObject ResolvePrefab(string prefabName)
    {
        if ((mode == GenerationMode.CustomLevel || mode == GenerationMode.MainLevel) && endPointGameplayPrefab != null && IsEndPointPrefab(prefabName))
        {
            return endPointGameplayPrefab;
        }
        return FindPrefab(prefabName);
    }

    private static bool IsEndPointPrefab(string prefabName)
    {
        return !string.IsNullOrEmpty(prefabName) &&
               prefabName.IndexOf("EndGamePlay", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private GameObject FindPrefab(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return null;
        foreach (GameObject p in mapPrefabs)
        {
            if (p != null && p.name == prefabName) return p;
        }
        return null;
    }

    private static void ApplyAlpha(Transform t, float alpha)
    {
        SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void LoadMusicFromResources(string musicFileName)
    {
        loadedMusic = null;
        if (string.IsNullOrEmpty(musicFileName)) return;

        string resourceName = Path.GetFileNameWithoutExtension(musicFileName);
        AudioClip clip = Resources.Load<AudioClip>("Music/" + resourceName);
        if (clip == null)
        {
            Debug.LogWarning($"Music not found at Resources/Music/{resourceName}");
            return;
        }
        loadedMusic = clip;
    }

    private void LoadMusicFromPersistent(string musicFileName)
    {
        if (string.IsNullOrEmpty(musicFileName)) return;
        string path = LevelSerializer.GetMusicPath(musicFileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Music file not found: {path}");
            return;
        }
        StartCoroutine(LoadMp3Coroutine(path));
    }

    private IEnumerator LoadMp3Coroutine(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
                yield break;
            }
            loadedMusic = DownloadHandlerAudioClip.GetContent(uwr);
        }
    }
}
