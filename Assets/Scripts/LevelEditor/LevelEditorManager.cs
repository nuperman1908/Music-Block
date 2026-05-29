using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelEditorManager : MonoBehaviour
{
    public static LevelEditorManager Instance { get; private set; }

    [Header("Objects")]
    public GameObject previewLine;
    public GameObject container;
    public GameObject settingMenu;

    [Header("Level Info")]
    public TMP_InputField levelName;
    public TMP_InputField startTime;
    public TextMeshProUGUI musicText;
    public AudioClip selectedMusic;
    public AudioSource previewSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void MusicPreview()
    {
        if (selectedMusic == null) return;
        if (previewSource.isPlaying)
        {
            previewLine.SetActive(false);
            previewSource.Stop();
        }
        previewLine.SetActive(true);
        float start = 0f;
        if (startTime != null && !string.IsNullOrEmpty(startTime.text))
        {
            float.TryParse(startTime.text, out start);
        }

        start = Mathf.Clamp(start, 0f, Mathf.Max(0f, selectedMusic.length - 0.01f));

        previewSource.Stop();
        previewSource.clip = selectedMusic;
        previewSource.time = start;
        previewSource.Play();
    }
    public void StopMusicPreview()
    {
        if (previewSource.isPlaying)
        {
            previewSource.Stop();
        }
        previewLine.SetActive(false);
    }

    public void SaveLevel()
    {
        if (container == null)
        {
            Debug.LogError("Container is not assigned.");
            return;
        }

        string name = levelName != null ? levelName.text : "";
        if (string.IsNullOrWhiteSpace(name)) name = "Untitled";

        float start = 0f;
        if (startTime != null && !string.IsNullOrEmpty(startTime.text))
        {
            float.TryParse(startTime.text, out start);
        }

        string musicFileName = LevelSerializer.SaveMusicFile(MusicSelector.SelectedPath);

        LevelInfo level = new LevelInfo
        {
            id = Mathf.Abs(name.GetHashCode()),
            name = name,
            difficulty = 0,
            music = musicFileName,
            startTime = start,
            items = new List<Item>()
        };

        Transform parent = container.transform;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            level.items.Add(new Item
            {
                id = 0,
                prefabName = StripCloneSuffix(child.name),
                x = child.position.x,
                y = child.position.y,
                zRotate = child.eulerAngles.z,
                alpha = GetAlpha(child),
                groupId = 0
            });
        }

        LevelSerializer.Save(level);
    }

    public void SaveAndPlay()
    {
        SaveLevel();
        PlayerPrefs.SetString("levelToLoad", levelName.text);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("CustomGameplay");
    }
    public void SaveAndQuit()
    {
        SaveLevel();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
    public void BackMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
    private static string StripCloneSuffix(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        int idx = name.IndexOf("(Clone)");
        return idx >= 0 ? name.Substring(0, idx) : name;
    }

    private static float GetAlpha(Transform t)
    {
        SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.color.a : 1f;
    }
}
