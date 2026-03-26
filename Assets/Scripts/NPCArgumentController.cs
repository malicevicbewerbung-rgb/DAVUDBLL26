using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NPCArgumentController : MonoBehaviour
{
    [Header("NPC References")]
    public ArgumentNPC npcA;
    public ArgumentNPC npcB;

    [Header("Player")]
    public Transform player;
    public float playerInterruptDistance = 5f;

    [Header("LLM")]
    public string llmUrl = "http://127.0.0.1:1234/v1/chat/completions";
    public string modelName = "google/gemma-3-4b";
    [Range(0f, 1.5f)]
    public float temperature = 0.9f;

    [Header("Conversation")]
    public float delayBetweenLines = 7f;
    public int maxExchanges = 6;
    public bool autoStart = true;

    [Header("Reward")]
    public int keyReward = 1;

    [Header("Runtime")]
    public bool isArguing = false;
    public bool playerInterrupted = false;
    public bool rewardGiven = false;

    private int exchangeCount = 0;
    private bool npcATurn = true;
    private Coroutine argumentRoutine;

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }

    [System.Serializable]
    private class ChatResponse
    {
        public Choice[] choices;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (autoStart)
            StartArgument();
    }

    void Update()
    {
        if (npcA != null && npcB != null)
        {
            npcA.LookAtTarget(npcB.transform);
            npcB.LookAtTarget(npcA.transform);
        }

        if (player == null)
            return;

        if (isArguing)
        {
            float distA = npcA != null ? Vector3.Distance(player.position, npcA.transform.position) : 999f;
            float distB = npcB != null ? Vector3.Distance(player.position, npcB.transform.position) : 999f;

            if (Mathf.Min(distA, distB) <= playerInterruptDistance)
            {
                playerInterrupted = true;
                StopArgument();

                if (npcA != null)
                    npcA.SayLine("Was willst du?", 2.5f);

                if (npcB != null)
                    npcB.SayLine("Bitte... hilf mir...", 2.5f);
            }
        }
    }

    public void StartArgument()
    {
        if (npcA == null || npcB == null)
        {
            Debug.LogWarning("NPCArgumentController: NPC references missing.");
            return;
        }

        if (argumentRoutine != null)
            StopCoroutine(argumentRoutine);

        playerInterrupted = false;
        exchangeCount = 0;
        npcATurn = true;
        isArguing = true;

        argumentRoutine = StartCoroutine(ArgumentLoop());
    }

    public void StopArgument()
    {
        isArguing = false;

        if (argumentRoutine != null)
        {
            StopCoroutine(argumentRoutine);
            argumentRoutine = null;
        }
    }

    IEnumerator ArgumentLoop()
    {
        while (isArguing && exchangeCount < maxExchanges)
        {
            yield return StartCoroutine(GenerateAndSpeakLine());

            exchangeCount++;

            if (!isArguing)
                yield break;

            npcATurn = !npcATurn;
            yield return new WaitForSeconds(delayBetweenLines);
        }

        isArguing = false;
        argumentRoutine = null;
    }

    IEnumerator GenerateAndSpeakLine()
    {
        ArgumentNPC speaker = npcATurn ? npcA : npcB;
        ArgumentNPC listener = npcATurn ? npcB : npcA;

        if (speaker == null || listener == null)
            yield break;

        string randomSeed = Random.Range(1000, 999999).ToString();

        string systemPrompt =
            "Du schreibst einen Streitdialog für zwei NPCs in einem düsteren Spiel. " +
            "Ein NPC ist aggressiv und dominant. " +
            "Der andere ist ängstlich und eingeschüchtert. " +
            "Schreibe genau EINE kurze deutsche Dialogzeile für den Sprecher. " +
            "Nur die Zeile, keine Namen, keine Erklärungen.";

        string userPrompt =
            "Sprecher: " + speaker.npcName + "\n" +
            "Persönlichkeit Sprecher: " + speaker.personalityDescription + "\n" +
            "Zuhörer: " + listener.npcName + "\n" +
            "Persönlichkeit Zuhörer: " + listener.personalityDescription + "\n" +
            "Runde: " + exchangeCount + "\n" +
            "Seed: " + randomSeed + "\n" +
            "Kontext: Die beiden streiten über Schuld, Misstrauen und einen verlorenen Schlüssel.";

        string json = "{"
            + "\"model\":\"" + EscapeJson(modelName) + "\","
            + "\"temperature\":" + temperature.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
            + "\"messages\":["
            + "{\"role\":\"system\",\"content\":\"" + EscapeJson(systemPrompt) + "\"},"
            + "{\"role\":\"user\",\"content\":\"" + EscapeJson(userPrompt) + "\"}"
            + "]"
            + "}";

        UnityWebRequest request = new UnityWebRequest(llmUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Argument LLM failed: " + request.error);
            yield break;
        }

        string rawJson = request.downloadHandler.text;
        ChatResponse response = JsonUtility.FromJson<ChatResponse>(rawJson);

        if (response == null || response.choices == null || response.choices.Length == 0 || response.choices[0].message == null)
        {
            Debug.LogWarning("Argument LLM response invalid.");
            yield break;
        }

        string line = response.choices[0].message.content.Trim();
        speaker.SayLine(line, delayBetweenLines - 0.3f);
        Debug.Log(speaker.npcName + ": " + line);
    }

    public void AccuseNPC(ArgumentNPC accusedNPC)
    {
        if (accusedNPC == null || rewardGiven)
            return;

        bool correct = !accusedNPC.tellsTruth;

        if (correct)
        {
            rewardGiven = true;

            if (KeyInventory.Instance != null)
                KeyInventory.Instance.AddKeys(keyReward);

            accusedNPC.SayLine("Verdammt... du hast mich durchschaut.", 3f);

            ArgumentNPC other = accusedNPC == npcA ? npcB : npcA;
            if (other != null)
                other.SayLine("Danke... du hast die Wahrheit erkannt.", 3f);
        }
        else
        {
            accusedNPC.SayLine("Ich habe die Wahrheit gesagt!", 3f);

            ArgumentNPC other = accusedNPC == npcA ? npcB : npcA;
            if (other != null)
                other.SayLine("Nein! Du beschuldigst die falsche Person!", 3f);
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