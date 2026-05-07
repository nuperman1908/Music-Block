using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Movement player;
    public int attempts = 0;
    public AudioSource music;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void LevelInit()
    {

    }

}
