using System.Collections.Generic;
using UnityEngine;

public class RiddleManager : MonoBehaviour
{
    public static RiddleManager Instance { get; private set; }

    private readonly HashSet<string> discoveredRiddles = new HashSet<string>();
    private readonly HashSet<string> solvedRiddles = new HashSet<string>();

    public int TotalRiddlesInLevel { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetTotalRiddles(int total)
    {
        TotalRiddlesInLevel = Mathf.Max(0, total);
    }

    public void RegisterRiddle(string npcName)
    {
        if (string.IsNullOrWhiteSpace(npcName))
            return;

        discoveredRiddles.Add(npcName.Trim());
    }

    public bool IsSolved(string npcName)
    {
        if (string.IsNullOrWhiteSpace(npcName))
            return false;

        return solvedRiddles.Contains(npcName.Trim());
    }

    public bool Solve(string npcName)
    {
        if (string.IsNullOrWhiteSpace(npcName))
            return false;

        string key = npcName.Trim();

        discoveredRiddles.Add(key);

        if (solvedRiddles.Contains(key))
            return false;

        solvedRiddles.Add(key);

        Debug.Log("Solved riddle of: " + key);

        if (QuestManager.Instance != null)
            QuestManager.Instance.CompleteCurrentQuestAndAdvance();

        return true;
    }

    public int SolvedCount => solvedRiddles.Count;

    public void ResetState()
    {
        discoveredRiddles.Clear();
        solvedRiddles.Clear();
        TotalRiddlesInLevel = 0;
    }
}