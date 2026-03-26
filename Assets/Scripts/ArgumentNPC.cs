using UnityEngine;

public class ArgumentNPC : MonoBehaviour
{
    [Header("Identity")]
    public string npcName = "NPC";

    [TextArea(2, 4)]
    public string personalityDescription = "neutral";

    [Header("Truth Role")]
    public bool tellsTruth = true;

    [Header("Look Settings")]
    public Transform lookTarget;
    public float lookSpeed = 4f;
    public bool onlyRotateY = true;

    [Header("World Text")]
    public NPCWorldText worldText;

    void Start()
    {
        if (lookTarget == null)
            lookTarget = transform;

        if (worldText == null)
            worldText = GetComponentInChildren<NPCWorldText>();
    }

    public void LookAtTarget(Transform target)
    {
        if (target == null)
            return;

        Transform rotateTarget = lookTarget != null ? lookTarget : transform;

        Vector3 dir = target.position - rotateTarget.position;

        if (onlyRotateY)
            dir.y = 0f;

        if (dir.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);
        rotateTarget.rotation = Quaternion.Slerp(
            rotateTarget.rotation,
            targetRotation,
            lookSpeed * Time.deltaTime
        );
    }

    public void SayLine(string text, float duration = 3f)
    {
        if (worldText != null)
            worldText.ShowText(text, duration);
    }
}