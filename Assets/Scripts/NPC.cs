using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NPC : MonoBehaviour
{
    [Header("Identity")]
    public string npcName = "???";

    [Header("Dialogue")]
    public DialogueData dialogueData;

    [Header("Interact Prompt Reference")]
    public InteractPrompt interactPrompt;

    [Header("LLM")]
    public string llmUrl = "http://127.0.0.1:1234/v1/chat/completions";
    public string modelName = "google/gemma-3-4b";
    [Range(0f, 1.5f)]
    public float llmTemperature = 0.9f;

    [Header("Look At Player")]
    public bool lookAtPlayerWhenNearby = true;
    public float lookSpeed = 5f;
    public bool onlyRotateY = true;
    public Transform lookTarget;

    [Header("Runtime Riddle State")]
    public bool hasGeneratedRiddle = false;

    private bool isPlayerInRange = false;
    private bool isLoadingLLM = false;
    private PlayerMovement currentPlayerMovement;
    private Transform playerTransform;

    private Coroutine llmRoutine;
    private UnityWebRequest activeRequest;

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
        if (interactPrompt == null)
            interactPrompt = GetComponent<InteractPrompt>();

        if (lookTarget == null)
            lookTarget = transform;

        if (dialogueData != null && !string.IsNullOrWhiteSpace(dialogueData.npcName))
            npcName = dialogueData.npcName;

        RefreshPrompt();

        if (QuestManager.Instance != null)
            QuestManager.Instance.RegisterNPC(this);
    }

    void Update()
    {
        HandleLookAtPlayer();

        if (!isPlayerInRange)
            return;

        if (DialogueUI.Instance == null)
            return;

        if (DialogueUI.Instance.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.E) && !isLoadingLLM)
            OpenDialogue();

        RefreshPrompt();
    }

    void HandleLookAtPlayer()
    {
        if (!lookAtPlayerWhenNearby || !isPlayerInRange || playerTransform == null || lookTarget == null)
            return;

        Vector3 targetDirection = playerTransform.position - lookTarget.position;

        if (onlyRotateY)
            targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized);
        lookTarget.rotation = Quaternion.Slerp(
            lookTarget.rotation,
            targetRotation,
            lookSpeed * Time.deltaTime
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInRange = true;
        currentPlayerMovement = other.GetComponent<PlayerMovement>();
        playerTransform = other.transform;

        if (currentPlayerMovement != null && DialogueUI.Instance != null)
            DialogueUI.Instance.SetPlayerMovement(currentPlayerMovement);

        RefreshPrompt();

        if (interactPrompt != null)
            interactPrompt.Show();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInRange = false;
        currentPlayerMovement = null;
        playerTransform = null;

        if (interactPrompt != null)
            interactPrompt.Hide();
    }

    void OpenDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning($"NPC {npcName} has no DialogueData assigned!");
            return;
        }

        if (DialogueUI.Instance == null)
        {
            Debug.LogWarning($"NPC {npcName} could not find DialogueUI instance.");
            return;
        }

        if (interactPrompt != null)
            interactPrompt.Hide();

        bool solved = dialogueData.hasRiddle &&
                      RiddleManager.Instance != null &&
                      RiddleManager.Instance.IsSolved(dialogueData.npcName);

        if (dialogueData.useLLMRiddle && dialogueData.hasRiddle && !solved)
        {
            if (!hasGeneratedRiddle)
            {
                llmRoutine = StartCoroutine(RequestLLMRiddleAndOpenDialogue());
                return;
            }

            DialogueUI.Instance.OpenDialogue(dialogueData);
            return;
        }

        DialogueUI.Instance.OpenDialogue(dialogueData);
    }

    IEnumerator RequestLLMRiddleAndOpenDialogue()
    {
        isLoadingLLM = true;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ClearCancelRequest();
            DialogueUI.Instance.ShowLoadingMessage(npcName, "Der NPC denkt über ein Rätsel nach...");
        }

        string randomSeed = Random.Range(1000, 999999).ToString();

        string systemPrompt =
            "Du bist ein mystischer NPC in einem düsteren Labyrinthspiel. " +
            "Erstelle genau ein einzigartiges kurzes Rätsel auf Deutsch mit einer klaren, einfachen Antwort. " +
            "Das Rätsel darf sich nicht wiederholen. " +
            "Nutze unterschiedliche Themen wie Schatten, Zeit, Erinnerung, Natur, Licht, Tod, Stille oder Feuer. " +
            "Antworte exakt in diesem Format:\n" +
            "FRAGE: <rätsel>\n" +
            "ANTWORT: <lösung>\n" +
            "HINWEIS: <hinweis>";

        string userPrompt =
            "NPC Name: " + npcName + "\n" +
            "Seed: " + randomSeed + "\n" +
            "Erzeuge ein einzigartiges atmosphärisches Rätsel mit einer einfachen eindeutigen Lösung.";

        string json = "{"
            + "\"model\":\"" + EscapeJson(modelName) + "\","
            + "\"temperature\":" + llmTemperature.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
            + "\"messages\":["
            + "{\"role\":\"system\",\"content\":\"" + EscapeJson(systemPrompt) + "\"},"
            + "{\"role\":\"user\",\"content\":\"" + EscapeJson(userPrompt) + "\"}"
            + "]"
            + "}";

        activeRequest = new UnityWebRequest(llmUrl, "POST");
        activeRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        activeRequest.downloadHandler = new DownloadHandlerBuffer();
        activeRequest.SetRequestHeader("Content-Type", "application/json");

        var op = activeRequest.SendWebRequest();

        while (!op.isDone)
        {
            if (DialogueUI.Instance != null && DialogueUI.Instance.WasCancelRequested())
            {
                activeRequest.Abort();
                isLoadingLLM = false;
                activeRequest = null;
                llmRoutine = null;
                yield break;
            }

            yield return null;
        }

        if (DialogueUI.Instance != null && DialogueUI.Instance.WasCancelRequested())
        {
            isLoadingLLM = false;
            activeRequest = null;
            llmRoutine = null;
            yield break;
        }

        isLoadingLLM = false;

        if (activeRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("LLM request failed: " + activeRequest.error);

            if (DialogueUI.Instance != null && !DialogueUI.Instance.WasCancelRequested())
                DialogueUI.Instance.ShowLockedMessage("Der NPC schweigt...");

            activeRequest = null;
            llmRoutine = null;
            yield break;
        }

        string rawJson = activeRequest.downloadHandler.text;
        ChatResponse response = JsonUtility.FromJson<ChatResponse>(rawJson);

        if (response == null ||
            response.choices == null ||
            response.choices.Length == 0 ||
            response.choices[0].message == null)
        {
            Debug.LogWarning("LLM response invalid.");

            if (DialogueUI.Instance != null && !DialogueUI.Instance.WasCancelRequested())
                DialogueUI.Instance.ShowLockedMessage("Das Rätsel konnte nicht geformt werden.");

            activeRequest = null;
            llmRoutine = null;
            yield break;
        }

        string content = response.choices[0].message.content;
        ParseLLMResponseIntoDialogueData(content);

        activeRequest = null;
        llmRoutine = null;

        if (DialogueUI.Instance != null && !DialogueUI.Instance.WasCancelRequested())
            DialogueUI.Instance.OpenDialogue(dialogueData);
    }

    void ParseLLMResponseIntoDialogueData(string content)
    {
        string question = "";
        string answer = "";
        string hint = "";

        string[] lines = content.Split('\n');

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.StartsWith("FRAGE:"))
                question = line.Substring("FRAGE:".Length).Trim();
            else if (line.StartsWith("ANTWORT:"))
                answer = line.Substring("ANTWORT:".Length).Trim();
            else if (line.StartsWith("HINWEIS:"))
                hint = line.Substring("HINWEIS:".Length).Trim();
        }

        if (string.IsNullOrWhiteSpace(question))
            question = "Ich habe kein Rätsel für dich...";

        dialogueData.riddleQuestion = question;
        dialogueData.riddleHint = hint;
        dialogueData.acceptedAnswers.Clear();

        if (!string.IsNullOrWhiteSpace(answer))
            dialogueData.acceptedAnswers.Add(answer);

        hasGeneratedRiddle = true;
    }

    string EscapeJson(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "");
    }

    public void RefreshPrompt()
    {
        if (interactPrompt == null)
            return;

        bool solved = dialogueData != null
            && dialogueData.hasRiddle
            && RiddleManager.Instance != null
            && RiddleManager.Instance.IsSolved(dialogueData.npcName);

        bool isQuestTarget = QuestManager.Instance != null && QuestManager.Instance.CurrentTargetNPC == this;

        string newPromptText;

        if (solved)
            newPromptText = $"[Solved] {npcName}";
        else if (isQuestTarget)
            newPromptText = $"[E] Talk to {npcName} [Quest]";
        else
            newPromptText = $"[E] Talk to {npcName}";

        interactPrompt.SetPromptText(newPromptText);

        if (!isPlayerInRange)
            interactPrompt.Hide();
    }

    public void ResetState()
    {
        if (llmRoutine != null)
        {
            StopCoroutine(llmRoutine);
            llmRoutine = null;
        }

        if (activeRequest != null)
        {
            activeRequest.Abort();
            activeRequest = null;
        }

        isPlayerInRange = false;
        currentPlayerMovement = null;
        playerTransform = null;
        isLoadingLLM = false;
        hasGeneratedRiddle = false;
        RefreshPrompt();
    }
}