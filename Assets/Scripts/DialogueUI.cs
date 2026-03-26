using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References (built at runtime)")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI continueHint;
    public Button closeButton;

    public GameObject inputArea;
    public TMP_InputField riddleInput;
    public Button submitButton;
    public Button hintButton;
    public TextMeshProUGUI hintText;

    private TextMeshProUGUI hintButtonLabel;
    private TextMeshProUGUI submitButtonLabel;
    private TextMeshProUGUI closeButtonLabel;

    private DialogueData currentDialogue;
    private int currentLineIndex = -1;

    private string state = "idle";
    private string pendingHint = "";
    private bool cancelRequested = false;

    private PlayerMovement playerMovement;

    private static DialogueUI instance;
    public static DialogueUI Instance => instance;

    public bool IsOpen => state != "idle";

    public bool IsTypingInInput =>
        riddleInput != null &&
        inputArea != null &&
        inputArea.activeSelf &&
        riddleInput.isFocused;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildDialogueUI();
    }

    void BuildDialogueUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        if (GetComponent<CanvasScaler>() == null)
            gameObject.AddComponent<CanvasScaler>();

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        dialoguePanel = CreateUIObject("DialoguePanel", transform);
        RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 20f);
        panelRect.sizeDelta = new Vector2(700f, 320f);

        Image panelBg = dialoguePanel.AddComponent<Image>();
        panelBg.color = new Color(0.06f, 0.06f, 0.1f, 0.97f);

        VerticalLayoutGroup vlg = dialoguePanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 20, 14);
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = dialoguePanel.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        GameObject nameObj = CreateUIObject("NPCName", dialoguePanel.transform);
        LayoutElement nameLayout = nameObj.AddComponent<LayoutElement>();
        nameLayout.minHeight = 24f;

        npcNameText = nameObj.AddComponent<TextMeshProUGUI>();
        npcNameText.text = "???";
        npcNameText.fontSize = 15;
        npcNameText.color = new Color(0.55f, 0.85f, 1f, 1f);
        npcNameText.alignment = TextAlignmentOptions.Center;
        npcNameText.fontStyle = FontStyles.Bold;

        GameObject textObj = CreateUIObject("DialogueText", dialoguePanel.transform);
        LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
        textLayout.minHeight = 70f;
        textLayout.preferredHeight = 100f;

        dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        dialogueText.text = "";
        dialogueText.fontSize = 22;
        dialogueText.color = new Color(0.92f, 0.92f, 0.92f, 1f);
        dialogueText.alignment = TextAlignmentOptions.Center;
        dialogueText.enableWordWrapping = true;
        dialogueText.overflowMode = TextOverflowModes.Overflow;

        inputArea = CreateUIObject("InputArea", dialoguePanel.transform);

        VerticalLayoutGroup inputVlg = inputArea.AddComponent<VerticalLayoutGroup>();
        inputVlg.spacing = 8;
        inputVlg.childAlignment = TextAnchor.UpperCenter;
        inputVlg.childControlWidth = true;
        inputVlg.childControlHeight = false;
        inputVlg.childForceExpandWidth = false;
        inputVlg.childForceExpandHeight = false;

        LayoutElement inputLayoutElement = inputArea.AddComponent<LayoutElement>();
        inputLayoutElement.minHeight = 140f;
        inputLayoutElement.preferredHeight = 150f;

        GameObject hintTextObj = CreateUIObject("HintText", inputArea.transform);
        LayoutElement hintTextLayout = hintTextObj.AddComponent<LayoutElement>();
        hintTextLayout.minHeight = 24f;

        hintText = hintTextObj.AddComponent<TextMeshProUGUI>();
        hintText.text = "";
        hintText.fontSize = 15;
        hintText.color = new Color(0.85f, 0.75f, 0.35f, 1f);
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.fontStyle = FontStyles.Italic;
        hintText.enableWordWrapping = true;

        GameObject inputObj = CreateUIObject("RiddleInput", inputArea.transform);
        LayoutElement inputObjLayout = inputObj.AddComponent<LayoutElement>();
        inputObjLayout.preferredWidth = 400f;
        inputObjLayout.preferredHeight = 44f;

        Image inputBgImg = inputObj.AddComponent<Image>();
        inputBgImg.color = new Color(0.18f, 0.18f, 0.25f, 1f);

        riddleInput = inputObj.AddComponent<TMP_InputField>();
        riddleInput.interactable = false;
        riddleInput.characterLimit = 64;
        riddleInput.targetGraphic = inputBgImg;

        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.sizeDelta = new Vector2(400f, 44f);

        GameObject textArea = CreateUIObject("TextArea", inputObj.transform);
        RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10f, 6f);
        textAreaRect.offsetMax = new Vector2(-10f, -6f);

        GameObject inputTextObj = CreateUIObject("Text", textArea.transform);
        RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.offsetMin = Vector2.zero;
        inputTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
        inputText.text = "";
        inputText.color = Color.white;
        inputText.fontSize = 20;
        inputText.alignment = TextAlignmentOptions.Center;
        inputText.enableWordWrapping = false;

        GameObject placeholderObj = CreateUIObject("Placeholder", textArea.transform);
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Type your answer...";
        placeholderText.color = new Color(0.45f, 0.45f, 0.5f, 1f);
        placeholderText.fontSize = 20;
        placeholderText.alignment = TextAlignmentOptions.Center;
        placeholderText.fontStyle = FontStyles.Italic;

        riddleInput.textViewport = textAreaRect;
        riddleInput.textComponent = inputText;
        riddleInput.placeholder = placeholderText;

        GameObject buttonRow = CreateUIObject("ButtonRow", inputArea.transform);
        HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 12f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = false;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        LayoutElement rowElement = buttonRow.AddComponent<LayoutElement>();
        rowElement.minHeight = 45f;
        rowElement.preferredHeight = 45f;

        hintButton = CreateButton(
            "HintButton",
            buttonRow.transform,
            "Need a hint?",
            new Vector2(200f, 36f),
            new Color(0.3f, 0.3f, 0.15f, 0.8f),
            out hintButtonLabel
        );
        hintButton.interactable = false;

        submitButton = CreateButton(
            "SubmitButton",
            buttonRow.transform,
            "Submit Answer",
            new Vector2(200f, 40f),
            new Color(0.2f, 0.5f, 0.8f, 1f),
            out submitButtonLabel
        );
        submitButton.interactable = false;

        submitButton.onClick.AddListener(OnSubmitAnswer);
        hintButton.onClick.AddListener(OnShowHint);

        inputArea.SetActive(false);

        GameObject hintContinueObj = CreateUIObject("ContinueHint", dialoguePanel.transform);
        LayoutElement hintContLayout = hintContinueObj.AddComponent<LayoutElement>();
        hintContLayout.minHeight = 24f;

        continueHint = hintContinueObj.AddComponent<TextMeshProUGUI>();
        continueHint.text = "Press E to continue";
        continueHint.fontSize = 14;
        continueHint.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        continueHint.alignment = TextAlignmentOptions.Right;

        closeButton = CreateButton(
            "CloseButton",
            dialoguePanel.transform,
            "Close",
            new Vector2(120f, 36f),
            new Color(0.5f, 0.2f, 0.2f, 1f),
            out closeButtonLabel
        );
        closeButton.onClick.AddListener(Close);

        dialoguePanel.SetActive(false);
    }

    GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    Button CreateButton(
        string objectName,
        Transform parent,
        string labelText,
        Vector2 size,
        Color backgroundColor,
        out TextMeshProUGUI label
    )
    {
        GameObject buttonObj = CreateUIObject(objectName, parent);

        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;
        layout.minWidth = size.x;
        layout.minHeight = size.y;

        Image bg = buttonObj.AddComponent<Image>();
        bg.color = backgroundColor;

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = bg;

        GameObject textObj = CreateUIObject("Label", buttonObj.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        label = textObj.AddComponent<TextMeshProUGUI>();
        label.text = labelText;
        label.color = Color.white;
        label.fontSize = 16;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;

        return button;
    }

    void Update()
    {
        if (state == "idle")
            return;

        bool typingInInput = IsTypingInInput;

        if (!typingInInput && Input.GetKeyDown(KeyCode.E))
        {
            if (state == "dialogue")
                AdvanceDialogue();
            else if (state == "feedback")
                HandleFeedbackAdvance();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cancelRequested = true;
            Close();
            return;
        }

        if (state == "riddle" && typingInInput && Input.GetKeyDown(KeyCode.Return))
            OnSubmitAnswer();
    }

    public bool WasCancelRequested()
    {
        return cancelRequested;
    }

    public void ClearCancelRequest()
    {
        cancelRequested = false;
    }

    public void OpenDialogue(DialogueData data)
    {
        if (data == null)
        {
            Debug.LogWarning("OpenDialogue called with null DialogueData.");
            return;
        }

        cancelRequested = false;
        currentDialogue = data;
        currentLineIndex = -1;
        state = "dialogue";

        if (data.hasRiddle && RiddleManager.Instance != null)
            RiddleManager.Instance.RegisterRiddle(data.npcName);

        if (QuestManager.Instance != null && QuestManager.Instance.CurrentTargetNPC != null)
        {
            if (QuestManager.Instance.CurrentTargetNPC.dialogueData != null &&
                QuestManager.Instance.CurrentTargetNPC.dialogueData.npcName == data.npcName)
            {
                QuestManager.Instance.MarkCurrentQuestAsReached();
            }
        }

        npcNameText.text = data.npcName;
        dialogueText.color = new Color(0.92f, 0.92f, 0.92f, 1f);
        hintText.text = "";
        inputArea.SetActive(false);
        dialoguePanel.SetActive(true);

        if (playerMovement != null)
            playerMovement.DisablePlayerControl();

        if (data.lines != null && data.lines.Count > 0)
            AdvanceDialogue();
        else
            EnterRiddleOrClose();
    }

    public void ShowLoadingMessage(string npcName, string message)
    {
        if (dialoguePanel == null)
            return;

        cancelRequested = false;
        state = "loading";
        currentDialogue = null;
        currentLineIndex = -1;

        npcNameText.text = npcName;
        dialogueText.text = message;
        dialogueText.color = new Color(0.85f, 0.85f, 1f, 1f);
        continueHint.text = "Press ESC to cancel";
        hintText.text = "";
        inputArea.SetActive(false);
        dialoguePanel.SetActive(true);

        if (playerMovement != null)
            playerMovement.DisablePlayerControl();

        CancelInvoke();
    }

    void AdvanceDialogue()
    {
        if (currentDialogue == null)
        {
            Close();
            return;
        }

        currentLineIndex++;

        if (currentDialogue.lines != null && currentLineIndex < currentDialogue.lines.Count)
        {
            dialogueText.text = $"\"{currentDialogue.lines[currentLineIndex].text}\"";
            continueHint.text = "Press E to continue";
            return;
        }

        currentLineIndex = -1;
        EnterRiddleOrClose();
    }

    void EnterRiddleOrClose()
    {
        bool shouldEnterRiddle =
            currentDialogue != null &&
            currentDialogue.hasRiddle &&
            RiddleManager.Instance != null &&
            !RiddleManager.Instance.IsSolved(currentDialogue.npcName);

        if (shouldEnterRiddle)
            EnterRiddleMode();
        else
            Close();
    }

    void EnterRiddleMode()
    {
        state = "riddle";

        dialogueText.text = $"\"{currentDialogue.riddleQuestion}\"";
        dialogueText.color = new Color(0.92f, 0.92f, 0.92f, 1f);
        continueHint.text = "Press ESC to close";

        pendingHint = currentDialogue.riddleHint;
        hintText.text = "";
        riddleInput.text = "";
        riddleInput.interactable = true;
        submitButton.interactable = true;
        hintButton.interactable = true;

        if (hintButtonLabel != null)
            hintButtonLabel.text = "Need a hint?";

        Image hintButtonImage = hintButton.GetComponent<Image>();
        if (hintButtonImage != null)
            hintButtonImage.color = new Color(0.3f, 0.3f, 0.15f, 0.8f);

        inputArea.SetActive(true);
        riddleInput.ActivateInputField();
        riddleInput.Select();
    }

    void OnShowHint()
    {
        if (string.IsNullOrEmpty(pendingHint))
            return;

        hintText.text = $"Hint: {pendingHint}";
        hintButton.interactable = false;

        if (hintButtonLabel != null)
            hintButtonLabel.text = "Hint shown";

        Image hintButtonImage = hintButton.GetComponent<Image>();
        if (hintButtonImage != null)
            hintButtonImage.color = new Color(0.2f, 0.2f, 0.1f, 0.5f);
    }

    void OnSubmitAnswer()
    {
        if (state != "riddle")
            return;

        string answer = riddleInput.text.Trim();
        if (string.IsNullOrEmpty(answer))
            return;

        bool correct = IsAnswerCorrect(answer);

        if (correct)
        {
            if (RiddleManager.Instance != null)
                RiddleManager.Instance.Solve(currentDialogue.npcName);

            if (KeyInventory.Instance != null)
                KeyInventory.Instance.AddKeys(currentDialogue.keysReward);

            ShowFeedback(currentDialogue.correctAnswerResponse, true);
        }
        else
        {
            ShowFeedback(currentDialogue.wrongAnswerResponse, false);
        }
    }

    bool IsAnswerCorrect(string answer)
    {
        if (currentDialogue == null || currentDialogue.acceptedAnswers == null || currentDialogue.acceptedAnswers.Count == 0)
            return false;

        string lower = answer.ToLowerInvariant();

        foreach (string accepted in currentDialogue.acceptedAnswers)
        {
            if (!string.IsNullOrWhiteSpace(accepted) &&
                accepted.Trim().ToLowerInvariant() == lower)
            {
                return true;
            }
        }

        return false;
    }

    void ShowFeedback(string message, bool correct)
    {
        state = "feedback";

        dialogueText.text = $"\"{message}\"";
        dialogueText.color = correct
            ? new Color(0.4f, 1f, 0.5f, 1f)
            : new Color(1f, 0.45f, 0.45f, 1f);

        continueHint.text = correct
            ? "The reward is yours..."
            : "Press E to try again";

        riddleInput.interactable = false;
        submitButton.interactable = false;
        hintButton.interactable = false;

        if (correct)
        {
            Invoke(nameof(Close), 2.5f);
        }
        else
        {
            riddleInput.text = "";
            riddleInput.ActivateInputField();
            riddleInput.Select();
        }
    }

    void HandleFeedbackAdvance()
    {
        if (currentDialogue == null)
        {
            Close();
            return;
        }

        bool solved =
            RiddleManager.Instance != null &&
            RiddleManager.Instance.IsSolved(currentDialogue.npcName);

        if (solved)
            Close();
        else
            EnterRiddleMode();
    }

    public void Close()
    {
        state = "idle";
        currentDialogue = null;
        currentLineIndex = -1;
        pendingHint = "";

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (playerMovement != null)
            playerMovement.EnablePlayerControl();

        CancelInvoke();
    }

    public void SetPlayerMovement(PlayerMovement pm)
    {
        playerMovement = pm;
    }

    public void ShowLockedMessage(string message)
    {
        if (dialoguePanel == null)
            return;

        state = "feedback";
        currentDialogue = null;
        currentLineIndex = -1;

        npcNameText.text = "Sealed";
        dialogueText.text = message;
        dialogueText.color = new Color(1f, 0.6f, 0.4f, 1f);
        continueHint.text = "";
        hintText.text = "";
        inputArea.SetActive(false);
        dialoguePanel.SetActive(true);

        if (playerMovement != null)
            playerMovement.DisablePlayerControl();

        CancelInvoke();
        Invoke(nameof(Close), 2.5f);
    }
}