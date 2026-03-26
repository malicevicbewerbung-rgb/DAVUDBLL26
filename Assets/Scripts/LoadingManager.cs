using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public TextMeshProUGUI loadingText;

    public string lmStudioUrl = "http://127.0.0.1:1234/v1/models";
    public string gameSceneName = "GameScene";

    void Start()
    {
        StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        if (loadingText != null)
            loadingText.text = "Verbinde mit KI...";

        UnityWebRequest request = UnityWebRequest.Get(lmStudioUrl);
        request.timeout = 3;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            if (loadingText != null)
                loadingText.text = "Fehler: Keine Verbindung zur KI.";
            yield break;
        }

        if (loadingText != null)
            loadingText.text = "Lade Spielszene...";

        yield return new WaitForSeconds(1f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneName);

        while (!operation.isDone)
        {
            yield return null;
        }
    }
}