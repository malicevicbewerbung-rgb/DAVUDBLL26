using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LLMClient : MonoBehaviour
{
    [Header("Scene Change")]
    public string nextSceneName = "NextScene1";
    public bool enableSceneChangeByInput = true;

    [Header("Intro")]
    public bool playIntroOnSceneStart = true;

    [TextArea(2, 4)]
    public string introLine = "Wo bin ich? Was mache ich hier?";

    [TextArea(2, 4)]
    public string introLine2 = "...";

    public string introVoiceId = "K5ZVtkkBnuPY6YqXs70E";

    [Header("UI")]
    public TextMeshProUGUI npcText;
    public TMP_InputField playerInput;
    public Button sendButton;

    [Header("Animator")]
    public Animator animator;
    public string thinkingParameter = "isThinking";
    public string talkingParameter = "isTalking";

    [Header("Audio / ElevenLabs")]
    public AudioSource audioSource;
    public string elevenApiKey = "PASTE_YOUR_NEW_ELEVENLABS_API_KEY_HERE";
    public string voiceId = "gpfkPsCDQR1A1crX3dah";
    public string elevenModelId = "eleven_flash_v2_5";

    [Header("NPC Infos")]
    public string npcName = "Der Wächter";

    [TextArea(2, 4)]
    public string personality = "spooky, geheimnisvoll, ruhig, unheimlich, ernst";

    [TextArea(3, 6)]
    public string backgroundInfo = "Der Wächter ist eine düstere Figur am Eingang des Labyrinths. Er kennt die Regeln des Spiels und spricht, als wäre das Labyrinth lebendig und gefährlich.";

    [TextArea(3, 6)]
    public string knowledge = "Das Spiel ist ein Exploration- und Survival-Spiel in einem labyrinthartigen Ort. Der Spieler muss Schlüssel finden, neue Bereiche öffnen, Gefahren überleben und mit NPCs interagieren. Manche NPCs helfen, andere sind misstrauisch oder feindselig.";

    [TextArea(2, 4)]
    public string speakingStyle = "spooky, poetisch, düster, leicht bedrohlich, aber verständlich";

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
    }

    [Serializable]
    public class ChatResponse
    {
        public Choice[] choices;
    }

    private Coroutine speechRoutine;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (npcText != null)
            npcText.text = "";

        if (sendButton != null)
            sendButton.onClick.AddListener(SendPlayerMessage);

        SetIdleState();

        if (playIntroOnSceneStart)
            StartCoroutine(PlayIntroSequence());
    }

    void Update()
    {
        if (playerInput != null && playerInput.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            SendPlayerMessage();
        }
    }

    bool ShouldLoadNextScene(string text)
    {
        string lower = text.ToLower().Trim();

        return lower.Contains("ich bin einverstanden") ||
               lower.Contains("geh weiter") ||
               lower.Contains("weiter") ||
               lower.Contains("okay") ||
               lower.Contains("ok") ||
               lower.Contains("los") ||
               lower.Contains("verstanden");
    }

    public void SendPlayerMessage()
    {
        if (playerInput == null || string.IsNullOrWhiteSpace(playerInput.text))
            return;

        string playerMessage = playerInput.text.Trim();
        playerInput.text = "";

        if (enableSceneChangeByInput && ShouldLoadNextScene(playerMessage))
        {
            if (npcText != null)
                npcText.text = "Dann geh weiter...";

            StartCoroutine(LoadNextSceneAfterShortDelay());
            return;
        }

        if (npcText != null)
            npcText.text = npcName + " denkt nach...";

        if (speechRoutine != null)
            StopCoroutine(speechRoutine);

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        StartCoroutine(SendRequest(playerMessage));
    }

    IEnumerator SendRequest(string playerMessage)
    {
        SetThinkingState();

        string url = "http://127.0.0.1:1234/v1/chat/completions";

        string systemPrompt =
            "Du bist ein NPC in einem Unity-Spiel.\n" +
            "Name: " + npcName + "\n" +
            "Persönlichkeit: " + personality + "\n" +
            "Hintergrund: " + backgroundInfo + "\n" +
            "Wissen über die Spielwelt: " + knowledge + "\n" +
            "Sprechstil: " + speakingStyle + "\n" +
            "Bleibe immer in deiner Rolle. " +
            "Antworte auf Deutsch. " +
            "Antworte spooky, geheimnisvoll und passend zur düsteren Atmosphäre. " +
            "Beantworte die Frage des Spielers klar, aber in einer unheimlichen Art. " +
            "Antworte in 2 bis 5 kurzen Sätzen.";

        string json = "{"
            + "\"model\":\"google/gemma-3-4b\","
            + "\"messages\":["
            + "{\"role\":\"system\",\"content\":\"" + EscapeJson(systemPrompt) + "\"},"
            + "{\"role\":\"user\",\"content\":\"" + EscapeJson(playerMessage) + "\"}"
            + "]"
            + "}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            ChatResponse response = JsonUtility.FromJson<ChatResponse>(responseText);

            if (response != null &&
                response.choices != null &&
                response.choices.Length > 0 &&
                response.choices[0].message != null)
            {
                string finalText = response.choices[0].message.content;

                if (npcText != null)
                    npcText.text = finalText;

                SetTalkingState();
                speechRoutine = StartCoroutine(PlayElevenLabsAndWait(finalText, voiceId));
            }
            else
            {
                if (npcText != null)
                    npcText.text = "Die Dunkelheit schweigt...";

                SetIdleState();
            }
        }
        else
        {
            Debug.LogError("LM Studio Fehler: " + request.error);

            if (npcText != null)
                npcText.text = "Ich kann im Moment nicht sprechen...";

            SetIdleState();
        }
    }

    IEnumerator PlayIntroSequence()
    {
        yield return new WaitForSeconds(1f);

        if (!string.IsNullOrWhiteSpace(introLine))
        {
            if (npcText != null)
                npcText.text = introLine;

            SetTalkingState();
            yield return StartCoroutine(PlayElevenLabsAndWait(introLine, introVoiceId));
        }

        if (!string.IsNullOrWhiteSpace(introLine2))
        {
            yield return new WaitForSeconds(0.3f);

            if (npcText != null)
                npcText.text = introLine2;

            SetTalkingState();
            yield return StartCoroutine(PlayElevenLabsAndWait(introLine2, introVoiceId));
        }

        SetIdleState();
    }

    IEnumerator PlayElevenLabsAndWait(string text, string currentVoiceId)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Keine AudioSource verbunden.");
            yield return StartCoroutine(StopTalkingAfterDelay(text));
            yield break;
        }

        if (string.IsNullOrWhiteSpace(elevenApiKey) || elevenApiKey == "PASTE_YOUR_NEW_ELEVENLABS_API_KEY_HERE")
        {
            Debug.LogWarning("ElevenLabs API Key fehlt.");
            yield return StartCoroutine(StopTalkingAfterDelay(text));
            yield break;
        }

        if (string.IsNullOrWhiteSpace(currentVoiceId) || currentVoiceId.Contains("PASTE"))
        {
            Debug.LogWarning("Voice ID fehlt.");
            yield return StartCoroutine(StopTalkingAfterDelay(text));
            yield break;
        }

        string url = "https://api.elevenlabs.io/v1/text-to-speech/" + currentVoiceId + "?output_format=mp3_44100_128";

        string body = "{"
            + "\"text\":\"" + EscapeJson(text) + "\","
            + "\"model_id\":\"" + EscapeJson(elevenModelId) + "\""
            + "}";

        UnityWebRequest ttsRequest = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);

        ttsRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        ttsRequest.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

        ttsRequest.SetRequestHeader("Content-Type", "application/json");
        ttsRequest.SetRequestHeader("xi-api-key", elevenApiKey);

        yield return ttsRequest.SendWebRequest();

        if (ttsRequest.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(ttsRequest);

            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();

                while (audioSource.isPlaying)
                    yield return null;
            }
            else
            {
                Debug.LogWarning("ElevenLabs AudioClip ist null.");
                yield return StartCoroutine(StopTalkingAfterDelay(text));
            }
        }
        else
        {
            Debug.LogError("ElevenLabs TTS Fehler: " + ttsRequest.error);
            yield return StartCoroutine(StopTalkingAfterDelay(text));
        }

        SetIdleState();
    }

    IEnumerator StopTalkingAfterDelay(string text)
    {
        float duration = Mathf.Clamp(text.Length * 0.05f, 2f, 6f);
        yield return new WaitForSeconds(duration);
        SetIdleState();
    }

    IEnumerator LoadNextSceneAfterShortDelay()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(nextSceneName);
    }

    void SetIdleState()
    {
        if (animator != null)
        {
            animator.SetBool(thinkingParameter, false);
            animator.SetBool(talkingParameter, false);
        }
    }

    void SetThinkingState()
    {
        if (animator != null)
        {
            animator.SetBool(thinkingParameter, true);
            animator.SetBool(talkingParameter, false);
        }
    }

    void SetTalkingState()
    {
        if (animator != null)
        {
            animator.SetBool(thinkingParameter, false);
            animator.SetBool(talkingParameter, true);
        }
    }

    string EscapeJson(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "");
    }
}