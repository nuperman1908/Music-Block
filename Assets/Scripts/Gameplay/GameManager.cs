using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Movement player;
    public GameObject playerPrefab;
    public int attempts = 1;
    public AudioSource musicSource;
    public CameraFollow cameraFollow;

    public bool paused = false;
    public bool playing = true;

    [Header("UI")]
    public Scrollbar progressBar;
    public GameObject pauseMenu;
    public GameObject winMenu;
    public TextMeshProUGUI attemptText;
    public TextMeshProUGUI attemptTextPause;
    public TextMeshProUGUI attemptTextWin;

    [Header("GameObjects")]
    public Transform endpoint;

    [Header("Spawn")]
    public Transform container;

    [Header("Prefab Lookup")]
    public List<GameObject> mapPrefabs = new List<GameObject>();

    [Header("Auto Load")]
    public string levelToLoad;
    public bool loadOnStart;

    [Header("SFX")]
    public AudioClip musicClip;
    public AudioClip winSfx;
    public float winFlyDuration = 0.8f;

    private bool _winning;
    private bool _gameplayStarted;

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
        Time.timeScale = 1f;
        GameObject transition = GameObject.Find("Transition");
        transition.GetComponent<Animator>().Play("TransitionOut");
        levelToLoad = PlayerPrefs.GetString("levelToLoad");
        LevelInit(levelToLoad);
        DOVirtual.DelayedCall(3f, () =>
        {
            GameObject.Destroy(GameObject.Find("TransitionCanvas"));
        });
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (!paused && player != null && endpoint != null)
        {
            progressBar.size = Mathf.Clamp01((player.transform.position.x - PlayerSpawn.x) / (endpoint.position.x - PlayerSpawn.x));
        }
    }

    public void MainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void TogglePause()
    {
        SetPaused(!paused);
    }

    public void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;
        if (musicSource != null)
        {
            if (paused) musicSource.Pause();
            else musicSource.UnPause();
        }

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(paused);
        }
        if (paused && attemptTextPause != null && attemptTextPause.isActiveAndEnabled)
            attemptTextPause.text = "Attempt " + attempts;
    }

    public void RestartLevel()
    {
        RestartLevel(0f);
    }

    public void RestartLevel(float delay)
    {
        if (delay > 0f)
        {
            StartCoroutine(RestartAfterDelay(delay));
        }
        else
        {
            DoRestart();
        }
    }

    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DoRestart();
    }

    private void DoRestart()
    {
        attempts++;
        if (attemptText != null)
        {
            attemptText.text = "Attempt " + attempts;
        }

        if (playerPrefab != null && player != null)
        {
            Destroy(player.gameObject);
            player = null;
        }
        SpawnPlayer();

        if (cameraFollow != null)
        {
            cameraFollow.transform.position = new Vector3(0, 0, -10);
        }

        if (musicSource != null && musicSource.clip != null)
        {
            float startTime = currentLevel != null ? currentLevel.startTime : 0f;
            musicSource.Stop();
            musicSource.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, musicSource.clip.length - 0.01f));
            musicSource.Play();
        }

        if (paused) SetPaused(false);
        _winning = false;
        playing = true;
    }

    public void TriggerWin()
    {
        if (_winning) return;
        _winning = true;
        playing = false;
        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        Movement m = player;
        Rigidbody2D rb = m != null ? m.GetComponent<Rigidbody2D>() : null;

        if (cameraFollow != null) cameraFollow.player = null;
        if (m != null) m.enabled = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        Vector3 start = m != null ? m.transform.position : Vector3.zero;
        Vector3 end = endpoint != null ? endpoint.position : start;
        float t = 0f;
        while (t < winFlyDuration && m != null)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / winFlyDuration);
            u = u * u * (3f - 2f * u);
            m.transform.position = Vector3.Lerp(start, end, u);
            yield return null;
        }

        if (m != null) m.gameObject.SetActive(false);

        if (winSfx != null && musicSource != null)
        {
            musicSource.PlayOneShot(winSfx);
        }
        yield return new WaitForSeconds(1.5f);
        if (winMenu != null) winMenu.SetActive(true);
        if (attemptTextWin != null && attemptTextWin.isActiveAndEnabled)
            attemptTextWin.text = "Attempt " + attempts;
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
        endpoint = GameObject.FindGameObjectWithTag("End").transform;
        DOVirtual.DelayedCall(2f, () =>
        {
            LoadMusicFromResources(level.music);
            SpawnPlayer();
            PlayMusic(level.startTime);
        });

        Debug.Log($"Loaded level '{level.name}' with {(level.items != null ? level.items.Count : 0)} items.");
    }

    private void LoadMusicFromResources(string musicFileName)
    {
        musicClip = null;
        if (string.IsNullOrEmpty(musicFileName)) return;

        string resourceName = System.IO.Path.GetFileNameWithoutExtension(musicFileName);
        AudioClip clip = Resources.Load<AudioClip>("Music/" + resourceName);
        if (clip == null)
        {
            Debug.LogWarning($"Music not found at Resources/Music/{resourceName}");
            return;
        }
        musicClip = clip;
    }

    private void PlayMusic(float startTime)
    {
        if (musicSource == null || musicClip == null) return;
        musicSource.clip = musicClip;
        musicSource.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, musicClip.length - 0.01f));
        musicSource.Play();
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
}
