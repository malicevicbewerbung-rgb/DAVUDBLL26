using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("HUD Settings")]
    public float fadeSpeed = 1f;
    public float autoHideDelay = 5f;
    public int requiredKeysForExit = 4;

   public TextMeshProUGUI timerText;

    private TextMeshProUGUI controlsText;
    private TextMeshProUGUI objectiveText;
    private TextMeshProUGUI keyCounterText;

    private float hideTimer = 0f;
    private bool fadingOut = false;

    void Awake()
    {
        BuildHUD();
    }

    void BuildHUD()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        if (GetComponent<CanvasScaler>() == null)
            gameObject.AddComponent<CanvasScaler>();

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        // Timer oben links
        GameObject timerObj = CreateUIObject("TimerText", transform);
        RectTransform timerRect = timerObj.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0f, 1f);
        timerRect.anchorMax = new Vector2(0f, 1f);
        timerRect.pivot = new Vector2(0f, 1f);
        timerRect.anchoredPosition = new Vector2(20f, -20f);
        timerRect.sizeDelta = new Vector2(220f, 40f);

        timerText = timerObj.AddComponent<TextMeshProUGUI>();
        timerText.text = "Time: 05:00";
        timerText.fontSize = 20;
        timerText.color = Color.red;
        timerText.alignment = TextAlignmentOptions.Left;

        // Controls unten links
        GameObject ctrlObj = CreateUIObject("ControlsText", transform);
        RectTransform ctrlRect = ctrlObj.GetComponent<RectTransform>();
        ctrlRect.anchorMin = new Vector2(0f, 0f);
        ctrlRect.anchorMax = new Vector2(0f, 0f);
        ctrlRect.pivot = new Vector2(0f, 0f);
        ctrlRect.anchoredPosition = new Vector2(20f, 20f);
        ctrlRect.sizeDelta = new Vector2(350f, 120f);

        controlsText = ctrlObj.AddComponent<TextMeshProUGUI>();
        controlsText.text = "WASD - Move\nMouse - Look\nShift - Run\nR - Crouch\nESC - Release cursor";
        controlsText.fontSize = 16;
        controlsText.color = new Color(1f, 1f, 1f, 0.75f);
        controlsText.alignment = TextAlignmentOptions.Left;

        // Objective oben Mitte
        GameObject objObj = CreateUIObject("ObjectiveText", transform);
        RectTransform objRect = objObj.GetComponent<RectTransform>();
        objRect.anchorMin = new Vector2(0.5f, 1f);
        objRect.anchorMax = new Vector2(0.5f, 1f);
        objRect.pivot = new Vector2(0.5f, 1f);
        objRect.anchoredPosition = new Vector2(0f, -20f);
        objRect.sizeDelta = new Vector2(700f, 80f);

        objectiveText = objObj.AddComponent<TextMeshProUGUI>();
        objectiveText.text = "Find the Exit";
        objectiveText.fontSize = 24;
        objectiveText.color = new Color(0.3f, 1f, 0.5f, 1f);
        objectiveText.alignment = TextAlignmentOptions.Center;
        objectiveText.enableWordWrapping = true;
        objectiveText.overflowMode = TextOverflowModes.Ellipsis;

        // Keys oben rechts
        GameObject keyObj = CreateUIObject("KeyCounter", transform);
        RectTransform keyRect = keyObj.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(1f, 1f);
        keyRect.anchorMax = new Vector2(1f, 1f);
        keyRect.pivot = new Vector2(1f, 1f);
        keyRect.anchoredPosition = new Vector2(-20f, -20f);
        keyRect.sizeDelta = new Vector2(220f, 40f);

        keyCounterText = keyObj.AddComponent<TextMeshProUGUI>();
        keyCounterText.text = $"Keys: 0/{requiredKeysForExit}";
        keyCounterText.fontSize = 18;
        keyCounterText.color = new Color(0.85f, 0.75f, 0.35f, 0.9f);
        keyCounterText.alignment = TextAlignmentOptions.Right;
    }

    GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    void Update()
    {
        UpdateKeyCounter();
        FadeControls();
    }

    void UpdateKeyCounter()
    {
        if (keyCounterText == null)
            return;

        int keys = KeyInventory.Instance != null ? KeyInventory.Instance.keyCount : 0;
        keyCounterText.text = $"Keys: {keys}/{requiredKeysForExit}";
    }

    void FadeControls()
    {
        if (!fadingOut)
        {
            hideTimer += Time.deltaTime;
            if (hideTimer >= autoHideDelay)
                fadingOut = true;
        }

        if (fadingOut && controlsText != null)
        {
            float alpha = controlsText.color.a - fadeSpeed * Time.deltaTime;
            alpha = Mathf.Max(0f, alpha);

            controlsText.color = new Color(
                controlsText.color.r,
                controlsText.color.g,
                controlsText.color.b,
                alpha
            );
        }
    }

    public void ShowObjective(string text)
    {
        if (objectiveText != null)
            objectiveText.text = text;
    }

    public void SetRequiredKeys(int amount)
    {
        requiredKeysForExit = Mathf.Max(0, amount);
        UpdateKeyCounter();
    }

    public void ResetControlsFade()
    {
        hideTimer = 0f;
        fadingOut = false;

        if (controlsText != null)
        {
            controlsText.color = new Color(
                controlsText.color.r,
                controlsText.color.g,
                controlsText.color.b,
                0.75f
            );
        }
    }
}