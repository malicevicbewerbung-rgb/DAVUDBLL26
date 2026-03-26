using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractPrompt : MonoBehaviour
{
    [Header("Prompt Settings")]
    public string promptText = "[E] Talk";

    [Header("Position")]
    public float extraHeight = 0.5f;
    public Vector2 promptSize = new Vector2(220f, 50f);
    public float worldScale = 0.01f;

    private GameObject promptObject;
    private TextMeshProUGUI promptLabel;
    private bool isVisible = false;

    private Renderer[] cachedRenderers;

    void Start()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>();
        BuildPrompt();
        Hide();
        UpdatePromptPosition();
    }

    void LateUpdate()
    {
        if (promptObject == null)
            return;

        UpdatePromptPosition();

        if (Camera.main != null && isVisible)
        {
            promptObject.transform.forward = Camera.main.transform.forward;
        }
    }

    void BuildPrompt()
    {
        if (promptObject != null)
            return;

        promptObject = new GameObject("InteractPrompt", typeof(RectTransform));
        promptObject.transform.SetParent(transform, false);
        promptObject.transform.localRotation = Quaternion.identity;
        promptObject.transform.localScale = Vector3.one * worldScale;

        Canvas canvas = promptObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        if (promptObject.GetComponent<GraphicRaycaster>() == null)
            promptObject.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = promptObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = promptSize;

        GameObject bgObj = new GameObject("Background", typeof(RectTransform));
        bgObj.transform.SetParent(promptObject.transform, false);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.55f);

        GameObject textObj = new GameObject("PromptText", typeof(RectTransform));
        textObj.transform.SetParent(promptObject.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        promptLabel = textObj.AddComponent<TextMeshProUGUI>();
        promptLabel.text = promptText;
        promptLabel.fontSize = 18f;
        promptLabel.color = Color.white;
        promptLabel.alignment = TextAlignmentOptions.Center;
        promptLabel.fontStyle = FontStyles.Bold;
        promptLabel.enableWordWrapping = false;
        promptLabel.overflowMode = TextOverflowModes.Ellipsis;
    }

    void UpdatePromptPosition()
    {
        if (promptObject == null)
            return;

        float topY = transform.position.y + 2f;

        if (cachedRenderers != null && cachedRenderers.Length > 0)
        {
            Bounds combinedBounds = cachedRenderers[0].bounds;

            for (int i = 1; i < cachedRenderers.Length; i++)
            {
                combinedBounds.Encapsulate(cachedRenderers[i].bounds);
            }

            topY = combinedBounds.max.y + extraHeight;
        }

        Vector3 worldPos = new Vector3(transform.position.x, topY, transform.position.z);
        promptObject.transform.position = worldPos;
    }

    public void SetPromptText(string newText)
    {
        promptText = newText;

        if (promptLabel != null)
            promptLabel.text = promptText;
    }

    public void Show()
    {
        if (promptObject == null)
            BuildPrompt();

        UpdatePromptPosition();

        if (promptLabel != null)
            promptLabel.text = promptText;

        promptObject.SetActive(true);
        isVisible = true;
    }

    public void Hide()
    {
        if (promptObject == null)
            return;

        promptObject.SetActive(false);
        isVisible = false;
    }
}