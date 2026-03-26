using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class StartMenuManager : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public Button joinButton;

    public string lmStudioUrl = "http://127.0.0.1:1234/v1/models";
    public string gameSceneName = "SampleScene";

    void Start()
    {
        statusText.text = "Drücke Join Spiel.";
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    public void OnJoinClicked()
    {
        StartCoroutine(CheckLMStudioAndStart());
    }

    IEnumerator CheckLMStudioAndStart()
    {
        statusText.text = "Verbinde mit KI...";
        joinButton.interactable = false;

        UnityWebRequest request = UnityWebRequest.Get(lmStudioUrl);
        request.timeout = 3;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            statusText.text = "Fehler: Keine Verbindung zur KI.";
            joinButton.interactable = true;
            yield break;
        }

        statusText.text = "Verbindung erfolgreich. Spiel startet...";
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("LoadingScene");
    }
}