using UnityEngine;

/// <summary>
/// Exit trigger. Requires a minimum number of keys before the player can finish.
/// </summary>
public class MazeExit : MonoBehaviour
{
    [Header("References")]
    public WinScreen winScreen;

    [Header("Requirements")]
    public int requiredKeys = 4;

    [Header("Locked Message")]
    [TextArea(1, 2)]
    public string lockedMessage = "The exit is sealed. You need 4 keys.";

    private bool hasTriggered = false;
    private static bool lockedMessageShown = false;

    void Start()
    {
        if (winScreen == null)
        {
            WinScreen[] screens = FindObjectsOfType<WinScreen>();
            foreach (var s in screens)
            {
                if (s != null)
                {
                    winScreen = s;
                    break;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hasTriggered)
            return;

        bool hasEnoughKeys = KeyInventory.Instance != null && KeyInventory.Instance.HasKeys(requiredKeys);

        if (hasEnoughKeys)
        {
            hasTriggered = true;
            TriggerWin();
        }
        else
        {
            ShowLockedMessage();
        }
    }

    void ShowLockedMessage()
    {
        if (lockedMessageShown)
            return;

        lockedMessageShown = true;

        int currentKeys = KeyInventory.Instance != null ? KeyInventory.Instance.keyCount : 0;
        string message = $"The exit is sealed. Keys: {currentKeys}/{requiredKeys}";

        if (DialogueUI.Instance != null)
            DialogueUI.Instance.ShowLockedMessage(message);
        else
            Debug.Log(message);

        Invoke(nameof(ResetLockedFlag), 3f);
    }

    void ResetLockedFlag()
    {
        lockedMessageShown = false;
    }

    void TriggerWin()
    {
        if (winScreen != null)
            winScreen.Show();
        else
            Debug.Log("WinScreen not found.");
    }
}