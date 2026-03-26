using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreen : MonoBehaviour
{
    [Header("UI References (assigned automatically)")]
    public GameObject overlayObject;
    public GameObject winPanel;
    public TextMeshProUGUI titleText;
    public Button restartButton;
    public Button quitButton;

    private TextMeshProUGUI restartButtonLabel;
    private TextMeshProUGUI quitButtonLabel;

    void Awake()
    {
        BuildWinScreen();
    }

    void Start()
    {
        Hide();
    }

    void BuildWinScreen()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        if (GetComponent<CanvasScaler>() == null)
            gameObject.AddComponent<CanvasScaler>();

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        // Semi-transparent overlay
        overlayObject = CreateUIObject("Overlay", transform);
        Image overlay = overlayObject.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.7f);

        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        // Win Panel
        winPanel = CreateUIObject("WinPanel", transform);
        RectTransform panelRect = winPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(450f, 300f);

        Image panelBg = winPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

        VerticalLayoutGroup vlg = winPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = winPanel.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Title
        GameObject titleObj = CreateUIObject("Title", winPanel.transform);
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.minHeight = 60f;

        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "You Escaped!";
        titleText.fontSize = 42;
        titleText.color = new Color(0.3f, 1f, 0.5f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;

        // Subtitle
        GameObject subtitleObj = CreateUIObject("Subtitle", winPanel.transform);
        LayoutElement subtitleLayout = subtitleObj.AddComponent<LayoutElement>();
        subtitleLayout.minHeight = 40f;

        TextMeshProUGUI subtitle = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitle.text = "You found your way through the maze.";
        subtitle.fontSize = 20;
        subtitle.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.enableWordWrapping = true;

        // Button Container
        GameObject btnContainer = CreateUIObject("ButtonContainer", winPanel.transform);
        LayoutElement btnContainerLayout = btnContainer.AddComponent<LayoutElement>();
        btnContainerLayout.minHeight = 60f;

        HorizontalLayoutGroup hlg = btnContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Restart Button
        restartButton = CreateButton(
            "RestartButton",
            btnContainer.transform,
            "Play Again",
            new Vector2(160f, 50f),
            new Color(0.2f, 0.6f, 0.3f, 1f),
            out restartButtonLabel
        );

        // Quit Button
        quitButton = CreateButton(
            "QuitButton",
            btnContainer.transform,
            "Quit",
            new Vector2(160f, 50f),
            new Color(0.6f, 0.2f, 0.2f, 1f),
            out quitButtonLabel
        );

        restartButton.onClick.AddListener(OnRestart);
        quitButton.onClick.AddListener(OnQuit);

        Hide();
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
        label.fontSize = 20;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;

        return button;
    }

    public void Show()
    {
        if (overlayObject != null)
            overlayObject.SetActive(true);

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        if (overlayObject != null)
            overlayObject.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    void OnRestart()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (RiddleManager.Instance != null)
            RiddleManager.Instance.ResetState();

        NPC[] npcs = FindObjectsOfType<NPC>();
        foreach (NPC npc in npcs)
            npc.ResetState();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnQuit()
    {
        Time.timeScale = 1f;
        Debug.Log("Game is exiting...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}