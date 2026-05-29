using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event System.Action OnPlayerDied;
    public event System.Action OnPlayerWon;
    public event System.Action OnPlayerSpawned;

    public void RaisePlayerDied() { OnPlayerDied?.Invoke(); }

    public Movement player;
    public Movement bot;
    public GameObject playerPrefab;
    public int attempts = 1;
    public AudioSource musicSource;
    public CameraFollow cameraFollow;

    public bool paused = true;
    public bool playing = true;
    public bool isChallengeMode;

    [Header("UI")]
    public Scrollbar progressBar;
    public Scrollbar progressBar_Bot;
    public GameObject pauseMenu;
    public GameObject winMenu;
    public GameObject loseMenu;
    public TextMeshProUGUI attemptText;
    public TextMeshProUGUI attemptTextPause;
    public TextMeshProUGUI attemptTextWin;
    public TextMeshProUGUI attemptTextLose;

    [Header("GameObjects")]
    public Transform endpoint;
    public Transform groundPlatform;
    public GameObject checkPointPrefab;

    [Header("Level")]
    public LevelGenerator levelGenerator;

    [Header("SFX")]
    public AudioClip winSfx;
    public float winFlyDuration = 0.8f;

    private bool _winning;
    private bool _gameplayStarted;

    private Vector3 _checkpointPos;
    private float _checkpointMusicTime;
    private Gamemodes _checkpointGamemode;
    private Speeds _checkpointSpeed;
    private bool _hasCheckpoint;

    public LevelInfo currentLevel => levelGenerator != null ? levelGenerator.currentLevel : null;

    private static readonly Vector3 PlayerSpawn = new Vector3(-0.5f, -2f, 0f);

    public void SetPlayerCheckpoint(Vector3 pos, float musicTime, Gamemodes gamemode, Speeds speed)
    {
        _checkpointPos = pos;
        _checkpointMusicTime = musicTime;
        _checkpointGamemode = gamemode;
        _checkpointSpeed = speed;
        _hasCheckpoint = true;
    }



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
        if (transition != null)
        {
            transition.GetComponent<Animator>().Play("TransitionOut");
        }

        string levelName = PlayerPrefs.GetString("levelToLoad");
        if (levelGenerator != null) levelGenerator.Generate(levelName);


        if (isChallengeMode && levelGenerator != null)
        {
            levelGenerator.SpawnCheckpoints(checkPointPrefab);
        }

        SpawnPlayer();
        float start = currentLevel != null ? currentLevel.startTime : 0f;

        DOVirtual.DelayedCall(3f, () =>
        {
            paused = false;
            PlayMusic(start);
            GameObject.Destroy(GameObject.Find("TransitionCanvas"));
        });
    }


    private void Update()
    {
        if (endpoint == null)
            endpoint = GameObject.FindGameObjectWithTag("End").transform;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        if (!paused && player != null && endpoint != null)
        {
            progressBar.value = (player.transform.position.x - PlayerSpawn.x) / (endpoint.position.x - PlayerSpawn.x);
        }
        if (isChallengeMode)
        {
            if (!paused && player != null && groundPlatform != null)
            {
                progressBar_Bot.value = (bot.transform.position.x - PlayerSpawn.x) / (endpoint.position.x - PlayerSpawn.x);
            }
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
            float startTime = _hasCheckpoint
                ? _checkpointMusicTime
                : (currentLevel != null ? currentLevel.startTime : 0f);
            musicSource.Stop();
            musicSource.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, musicSource.clip.length - 0.01f));
            musicSource.Play();
        }

        if (paused) SetPaused(false);
        _winning = false;
        playing = true;
    }
    public void RestartChallenge()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void LoseChallenge()
    {
        if (_winning) return;
        loseMenu.SetActive(true);
        paused = true;
        playing = false;
        if (musicSource != null)
        {
            musicSource.Pause();
        }
        attemptTextLose.text = "Attempt " + attempts;
    }
    public void TriggerWin()
    {
        if (_winning) return;
        _winning = true;
        playing = false;
        OnPlayerWon?.Invoke();
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

    private void PlayMusic(float startTime)
    {
        if (musicSource == null) return;
        AudioClip clip = levelGenerator != null ? levelGenerator.loadedMusic : null;
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, clip.length - 0.01f));
        musicSource.Play();
    }

    private void SpawnPlayer()
    {
        Vector3 spawn = _hasCheckpoint ? _checkpointPos : PlayerSpawn;
        if (playerPrefab != null)
        {
            GameObject obj = Instantiate(playerPrefab, spawn, Quaternion.identity);
            Movement m = obj.GetComponent<Movement>();
            cameraFollow.player = obj.transform;
            if (m != null)
            {
                player = m;
                if (_hasCheckpoint)
                {
                    m.CurrentGamemode = _checkpointGamemode;
                    m.CurrentSpeed = _checkpointSpeed;
                }
            }
        }
        else if (player != null)
        {
            player.transform.position = spawn;
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
            if (_hasCheckpoint)
            {
                player.CurrentGamemode = _checkpointGamemode;
                player.CurrentSpeed = _checkpointSpeed;
            }
        }
        else
        {
            Debug.LogWarning("No player prefab or scene player assigned.");
        }
        OnPlayerSpawned?.Invoke();
    }

}
