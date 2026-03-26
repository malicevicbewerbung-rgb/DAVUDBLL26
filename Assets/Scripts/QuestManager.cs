using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public string currentQuestTitle = "Find the next NPC";
    public string currentQuestDescription = "Talk to the marked NPC.";
    public NPC CurrentTargetNPC { get; private set; }

    private readonly List<NPC> registeredNPCs = new List<NPC>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        UpdateHUDObjective();
    }

    public void RegisterNPC(NPC npc)
    {
        if (npc == null)
            return;

        if (!registeredNPCs.Contains(npc))
            registeredNPCs.Add(npc);

        Debug.Log("Registered NPC: " + npc.npcName);

        if (CurrentTargetNPC == null)
            SetNextAvailableTarget();
    }

   public void MarkCurrentQuestAsReached()
{
}
    public void CompleteCurrentQuestAndAdvance()
    {
        Debug.Log("Quest completed, switching to next NPC...");
        SetNextAvailableTarget();
        RefreshAllNPCPrompts();
    }

    public void SetNextAvailableTarget()
    {
        CleanupNPCList();

        NPC next = null;

        for (int i = 0; i < registeredNPCs.Count; i++)
        {
            NPC npc = registeredNPCs[i];

            if (npc == null || npc.dialogueData == null)
                continue;

            bool solved = npc.dialogueData.hasRiddle &&
                          RiddleManager.Instance != null &&
                          RiddleManager.Instance.IsSolved(npc.dialogueData.npcName);

            if (!solved)
            {
                next = npc;
                break;
            }
        }

        CurrentTargetNPC = next;

        if (registeredNPCs.Count == 0)
        {
            currentQuestTitle = "Loading...";
            currentQuestDescription = "Searching for NPCs...";
            Debug.Log("No NPCs registered yet.");
        }
        else if (CurrentTargetNPC == null)
        {
            currentQuestTitle = "All riddles solved";
            currentQuestDescription = "Head to the exit.";
            Debug.Log("No more unsolved NPCs found.");
        }
        else
        {
            currentQuestTitle = "Find the next NPC";
            currentQuestDescription = "Talk to " + CurrentTargetNPC.npcName + ".";
            Debug.Log("New target NPC: " + CurrentTargetNPC.npcName);
        }

        UpdateHUDObjective();
    }

    void RefreshAllNPCPrompts()
    {
        for (int i = 0; i < registeredNPCs.Count; i++)
        {
            if (registeredNPCs[i] != null)
                registeredNPCs[i].RefreshPrompt();
        }
    }

    void CleanupNPCList()
    {
        for (int i = registeredNPCs.Count - 1; i >= 0; i--)
        {
            if (registeredNPCs[i] == null)
                registeredNPCs.RemoveAt(i);
        }
    }

    void UpdateHUDObjective()
    {
        HUD hud = FindObjectOfType<HUD>();
        if (hud != null)
            hud.ShowObjective(currentQuestTitle + "\n" + currentQuestDescription);
    }
}