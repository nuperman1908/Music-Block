using SimpleFileBrowser;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MusicSelector : MonoBehaviour
{
    [SerializeField] private string dialogTitle = "Select Music";
    [SerializeField] private string submitButtonText = "Load";
    public UnityEvent<string> onMp3Selected;
    private TextMeshProUGUI buttonText;
    public static string SelectedPath { get; private set; }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OpenDialog);
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Music", ".mp3"));
        FileBrowser.SetDefaultFilter(".mp3");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
    }

    private void OpenDialog()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(
            FileBrowser.PickMode.Files, false, null, null, dialogTitle, submitButtonText);

        if (!FileBrowser.Success || FileBrowser.Result == null || FileBrowser.Result.Length == 0)
            yield break;

        string path = FileBrowser.Result[0];
        SelectedPath = path;
        Debug.Log("Selected mp3: " + path);
        buttonText.text = System.IO.Path.GetFileName(path);
        onMp3Selected?.Invoke(path);
        StartCoroutine(LoadMp3Coroutine(path));
    }
    IEnumerator LoadMp3Coroutine(string path)
    {
        string uri = "file://" + path;

        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);

                LevelEditorManager.Instance.selectedMusic = clip;
            }
        }
    }  
}
