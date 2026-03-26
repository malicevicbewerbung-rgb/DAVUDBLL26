using TMPro;
using UnityEngine;

public class NPCWorldText : MonoBehaviour
{
    public TextMeshPro textMesh;
    public float heightOffset = 2.5f;

    private float hideTimer = 0f;

    void Awake()
    {
        if (textMesh == null)
        {
            GameObject textObj = new GameObject("WorldText");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = new Vector3(0f, heightOffset, 0f);

            textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.text = "";
            textMesh.fontSize = 3f;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.color = Color.white;
            textMesh.enableWordWrapping = true;
            textMesh.rectTransform.sizeDelta = new Vector2(6f, 2f);
        }
    }

    void Update()
    {
        if (Camera.main != null && textMesh != null)
            textMesh.transform.forward = Camera.main.transform.forward;

        if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;

            if (hideTimer <= 0f && textMesh != null)
                textMesh.text = "";
        }
    }

    public void ShowText(string text, float duration = 3f)
    {
        if (textMesh == null)
            return;

        textMesh.text = text;
        hideTimer = duration;
    }

    public void Clear()
    {
        if (textMesh != null)
            textMesh.text = "";

        hideTimer = 0f;
    }
}