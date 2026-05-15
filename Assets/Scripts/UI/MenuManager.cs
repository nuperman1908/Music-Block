using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [Header("Game Object")]
    public GameObject menuContainer;
    public GameObject transition;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
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
    public void ExitGame()
    {
        Application.Quit();
    }
    public void LvEditor()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelEditor");
    }
}
