using UnityEngine;

public class QuestArrowUI : MonoBehaviour
{
    public RectTransform arrowRect;
    public Camera targetCamera;
    public Transform playerTransform;

    [Header("Placement")]
    public float topOffset = 40f;
    public float edgePadding = 80f;
    public float hideDistance = 3f;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (arrowRect == null)
            arrowRect = GetComponent<RectTransform>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (arrowRect == null || targetCamera == null || QuestManager.Instance == null)
            return;

        NPC targetNPC = QuestManager.Instance.CurrentTargetNPC;

        if (targetNPC == null)
        {
            arrowRect.gameObject.SetActive(false);
            return;
        }

        Transform target = targetNPC.transform;

        if (playerTransform != null)
        {
            float dist = Vector3.Distance(playerTransform.position, target.position);
            if (dist < hideDistance)
            {
                arrowRect.gameObject.SetActive(false);
                return;
            }
        }

        arrowRect.gameObject.SetActive(true);

        Vector3 screenPos = targetCamera.WorldToScreenPoint(target.position);
        bool isBehind = screenPos.z < 0f;

        float x = screenPos.x;
        if (isBehind)
            x = Screen.width - x;

        x = Mathf.Clamp(x, edgePadding, Screen.width - edgePadding);

        arrowRect.position = new Vector3(x, Screen.height - topOffset, 0f);

        Vector3 camForward = targetCamera.transform.forward;
        camForward.y = 0f;

        Vector3 toTarget = target.position - targetCamera.transform.position;
        toTarget.y = 0f;

        if (camForward.sqrMagnitude > 0.001f && toTarget.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.SignedAngle(camForward.normalized, toTarget.normalized, Vector3.up);
            arrowRect.rotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }
}