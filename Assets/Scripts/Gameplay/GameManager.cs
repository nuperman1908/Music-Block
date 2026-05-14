using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Movement player;
    public GameObject playerPrefab;
    public int attempts = 0;
    public AudioSource music;
    public CameraFollow cameraFollow;


    [Header("Spawn")]
    public Transform container;

    [Header("Prefab Lookup")]
    public List<GameObject> mapPrefabs = new List<GameObject>();

    [Header("Auto Load")]
    public string levelToLoad;
    public bool loadOnStart;

    public LevelInfo currentLevel { get; private set; }

    private static readonly Vector3 PlayerSpawn = new Vector3(-0.5f, -2f, 0f);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 144;
        if (loadOnStart && !string.IsNullOrEmpty(levelToLoad))
        {
            LevelInit(levelToLoad);
        }
    }

    public void LevelInit()
    {
        LevelInit(levelToLoad);
    }

    public void LevelInit(string levelFileName)
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
            }
        }

        SpawnPlayer();
        LoadMusicFromPersistent(level.music, level.startTime);

        Debug.Log($"Loaded level '{level.name}' with {(level.items != null ? level.items.Count : 0)} items.");
    }

    private void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            GameObject obj = Instantiate(playerPrefab, PlayerSpawn, Quaternion.identity);
            Movement m = obj.GetComponent<Movement>();
            cameraFollow.player = obj.transform;
            if (m != null) player = m;
        }
        else if (player != null)
        {
            player.transform.position = PlayerSpawn;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }
        else
        {
            Debug.LogWarning("No player prefab or scene player assigned.");
        }
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

    private void LoadMusicFromPersistent(string musicFileName, float startTime)
    {
        if (music == null) return;
        if (string.IsNullOrEmpty(musicFileName)) return;
        string path = LevelSerializer.GetMusicPath(musicFileName);
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"Music file not found: {path}");
            return;
        }
        StartCoroutine(LoadMp3Coroutine(path, startTime));
    }

    private IEnumerator LoadMp3Coroutine(string path, float startTime)
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
            AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
            if (clip == null || music == null) yield break;
            music.clip = clip;
            music.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, clip.length - 0.01f));
            music.Play();
        }
    }
}
