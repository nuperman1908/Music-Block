using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LevelGenerator : MonoBehaviour
{
    [Header("Spawn")]
    public Transform container;

    [Header("Prefab Lookup")]
    public List<GameObject> mapPrefabs = new List<GameObject>();

    [Header("Auto Load")]
    public string levelToLoad;
    public bool loadOnStart;

    public AudioClip loadedMusic { get; private set; }
    public LevelInfo currentLevel { get; private set; }

    private void Start()
    {
        if (loadOnStart && !string.IsNullOrEmpty(levelToLoad))
        {
            InitLevel(levelToLoad);
        }
    }

    public void InitLevel(string levelFileName)
    {
        if (container == null)
        {
            Debug.LogError("Container is not assigned.");
            return;
        }

        string resourcePath = "Level/" + System.IO.Path.GetFileNameWithoutExtension(levelFileName);
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
        currentLevel = level;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
        ObjSelectionButton.ClearPlaced();

        LevelEditorManager mgr = LevelEditorManager.Instance;
        if (mgr != null)
        {
            if (mgr.levelName != null) mgr.levelName.text = level.name;
            if (mgr.startTime != null) mgr.startTime.text = level.startTime.ToString("0.00");
            if (mgr.musicText != null) mgr.musicText.text = level.music;
        }

        if (level.items != null)
        {
            foreach (Item item in level.items)
            {
                GameObject prefab = FindPrefab(item.prefabName);
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
                ObjSelectionButton.RegisterPlaced(obj);
            }
        }

        LoadMusicFromPersistent(level.music);

        Debug.Log($"Loaded level '{level.name}' with {(level.items != null ? level.items.Count : 0)} items.");
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

    private void LoadMusicFromPersistent(string musicFileName)
    {
        if (string.IsNullOrEmpty(musicFileName)) return;
        string path = LevelSerializer.GetMusicPath(musicFileName);
        if (!System.IO.File.Exists(path))
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
