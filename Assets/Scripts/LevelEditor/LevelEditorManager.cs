using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelEditorManager : MonoBehaviour
{
    public static LevelEditorManager Instance { get; private set; }

    [Header("Objects")]
    public GameObject previewLine;

    [Header("Level Info")]
    public TMP_InputField levelName;
    public TMP_InputField startTime;
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

}
