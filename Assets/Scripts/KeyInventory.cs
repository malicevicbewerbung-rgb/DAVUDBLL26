using UnityEngine;

public class KeyInventory : MonoBehaviour
{
    public static KeyInventory Instance { get; private set; }

    [Header("Keys")]
    public int keyCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddKeys(int amount)
    {
        if (amount <= 0)
            return;

        keyCount += amount;
        Debug.Log("Keys received. Total keys: " + keyCount);
    }

    public bool HasKeys(int amount)
    {
        return keyCount >= amount;
    }

    public bool UseKeys(int amount)
    {
        if (amount <= 0)
            return true;

        if (keyCount < amount)
            return false;

        keyCount -= amount;
        return true;
    }

    public void ResetState()
    {
        keyCount = 0;
    }
}