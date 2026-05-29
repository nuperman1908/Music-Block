using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [Header("Game Object")]
    public GameObject menuContainer;
    public GameObject transition;
    public GameObject customContainer;
    public GameObject customLevelPrefab;
    public GameObject customPlayEditSelector;


    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        PopulateCustomLevels();
    }

    private void PopulateCustomLevels()
    {
        if (customContainer == null || customLevelPrefab == null) return;

        Transform parent = customContainer.transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }

        string dir = Path.Combine(Application.persistentDataPath, "levels");
        if (!Directory.Exists(dir)) return;

        foreach (string file in Directory.GetFiles(dir, "*.json"))
        {
            LevelInfo level = JsonUtility.FromJson<LevelInfo>(File.ReadAllText(file));
            if (level == null) continue;

            GameObject btn = Instantiate(customLevelPrefab, parent);
            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = level.name;

            Button button = btn.GetComponent<Button>();
            if (button != null)
            {
                string levelName = level.name;
                button.onClick.AddListener(() => OnCustomLevelClicked(levelName));
            }
        }
    }

    private void OnCustomLevelClicked(string levelName)
    {
        PlayerPrefs.SetString("levelToLoad", levelName);
        PlayerPrefs.Save();
        if (customPlayEditSelector != null) customPlayEditSelector.SetActive(true);
    }
    public void MenuMove(float xPosition)
    {
        menuContainer.transform.DOLocalMoveX(xPosition, 1f).SetEase(Ease.OutBounce);
    }
    public void StartGameSingle(string levelName)
    {
        transition.SetActive(true);
        DontDestroyOnLoad(transition);
        DOVirtual.DelayedCall(2f, () =>
        {
            PlayerPrefs.SetString("levelToLoad", levelName);
            UnityEngine.SceneManagement.SceneManager.LoadScene("SingleGameplay");
        });
    }

    public void StartGameChallenge(bool isHard)
    {
        transition.SetActive(true);
        DontDestroyOnLoad(transition);
        DOVirtual.DelayedCall(2f, () =>
        {
            PlayerPrefs.SetInt("ChallengeMode", isHard ? 1 : 0);
            UnityEngine.SceneManagement.SceneManager.LoadScene("ChallengeGameplay");
        });
    }

    public void StartGameCustom()
    {
        transition.SetActive(true);
        DontDestroyOnLoad(transition);
        DOVirtual.DelayedCall(2f, () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CustomGameplay");
        });
    }
    public void StartEditor(int load)
    {
        PlayerPrefs.SetInt("LoadOnStart", load);
        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelEditor");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    public void LvEditor()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelEditor");
    }
}
