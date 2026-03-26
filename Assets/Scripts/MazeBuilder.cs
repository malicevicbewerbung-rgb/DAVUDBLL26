using System.Collections.Generic;
using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
    MazeData MaDa;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject exitPrefab;
    public GameObject npcPrefab;

    [Header("NPC Dialogue")]
    public List<DialogueData> npcDialogues = new List<DialogueData>();

    [Header("Build Settings")]
    public Transform buildParent;

    [Header("Argument NPC Pair")]
    public GameObject aggressiveNpcPrefab;
    public GameObject scaredNpcPrefab;
    public bool spawnArgumentPair = true;

    int spawnedNpcCount = 0;
    private List<DialogueData> shuffledDialogues = new List<DialogueData>();
    private int dialogueIndex = 0;

    void Awake()
    {
        MaDa = GetComponent<MazeData>();
    }

    public void BuildMaze()
    {
        ClearOldMaze();

        if (buildParent == null)
            buildParent = transform;

        spawnedNpcCount = 0;
        dialogueIndex = 0;
        PrepareDialoguePool();

        HashSet<Vector2Int> reservedCells = new HashSet<Vector2Int>();

        for (int x = 0; x < MaDa.w; x++)
        {
            for (int y = 0; y < MaDa.h; y++)
            {
                Vector3 pos = GetWorldPos(x, y);
                CellData cell = MaDa.cells[x, y];

                if (floorPrefab != null)
                    Instantiate(floorPrefab, pos, Quaternion.identity, buildParent);

                if (cell.upw && wallPrefab != null)
                {
                    Instantiate(
                        wallPrefab,
                        pos + new Vector3(0f, 0f, MaDa.cellSize / 2f),
                        Quaternion.identity,
                        buildParent
                    );
                }

                if (cell.rightw && wallPrefab != null)
                {
                    Instantiate(
                        wallPrefab,
                        pos + new Vector3(MaDa.cellSize / 2f, 0f, 0f),
                        Quaternion.Euler(0f, 90f, 0f),
                        buildParent
                    );
                }

                if (x == 0 && cell.linksw && wallPrefab != null)
                {
                    Instantiate(
                        wallPrefab,
                        pos + new Vector3(-MaDa.cellSize / 2f, 0f, 0f),
                        Quaternion.Euler(0f, 90f, 0f),
                        buildParent
                    );
                }

                if (y == 0 && cell.downw && wallPrefab != null)
                {
                    Instantiate(
                        wallPrefab,
                        pos + new Vector3(0f, 0f, -MaDa.cellSize / 2f),
                        Quaternion.identity,
                        buildParent
                    );
                }

                if (cell.isExit && exitPrefab != null)
                    Instantiate(exitPrefab, pos, Quaternion.identity, buildParent);
            }
        }

        // Streitpaar zuerst spawnen
        if (TrySpawnArgumentPair(reservedCells))
        {
            spawnedNpcCount += 2;
            Debug.Log("Streit-NPCs wurden gespawnt.");
        }
        else
        {
            Debug.LogWarning("Streit-NPCs konnten nicht gespawnt werden.");
        }

        // Normale NPCs spawnen
        for (int i = 0; i < MaDa.npcSpawnCells.Count; i++)
        {
            Vector2Int cellPos = MaDa.npcSpawnCells[i];

            if (reservedCells.Contains(cellPos))
                continue;

            SpawnNormalNPC(cellPos);
            spawnedNpcCount++;
        }

        if (RiddleManager.Instance != null)
            RiddleManager.Instance.SetTotalRiddles(spawnedNpcCount);

        Debug.Log("Gesamt gespawnte NPCs: " + spawnedNpcCount);
    }

    void SpawnNormalNPC(Vector2Int cellPos)
    {
        if (npcPrefab == null)
            return;

        Vector3 pos = GetWorldPos(cellPos.x, cellPos.y);

        GameObject npcObj = Instantiate(npcPrefab, pos, Quaternion.identity, buildParent);

        InteractPrompt prompt = npcObj.GetComponent<InteractPrompt>();
        if (prompt == null)
            prompt = npcObj.AddComponent<InteractPrompt>();

        CapsuleCollider cap = npcObj.GetComponent<CapsuleCollider>();
        if (cap == null)
            cap = npcObj.AddComponent<CapsuleCollider>();

        cap.isTrigger = true;
        cap.radius = 3f;
        cap.height = 6f;
        cap.center = new Vector3(0f, 1.5f, 0f);

        NPC npc = npcObj.GetComponent<NPC>();
        if (npc != null)
        {
            DialogueData nextDialogueTemplate = GetNextDialogue();

            if (nextDialogueTemplate != null)
            {
                DialogueData runtimeDialogue = ScriptableObject.Instantiate(nextDialogueTemplate);
                npc.dialogueData = runtimeDialogue;
                npc.interactPrompt = prompt;

                if (!string.IsNullOrWhiteSpace(runtimeDialogue.npcName))
                    npc.npcName = runtimeDialogue.npcName;
            }
        }
    }

    bool TrySpawnArgumentPair(HashSet<Vector2Int> reservedCells)
    {
        if (!spawnArgumentPair)
        {
            Debug.Log("spawnArgumentPair ist deaktiviert.");
            return false;
        }

        if (aggressiveNpcPrefab == null || scaredNpcPrefab == null)
        {
            Debug.LogWarning("Aggressive oder Scared Prefab fehlt.");
            return false;
        }

        if (MaDa.argumentCellA.x < 0 || MaDa.argumentCellB.x < 0)
        {
            Debug.LogWarning("argumentCellA oder argumentCellB wurde nicht gesetzt.");
            return false;
        }

        Vector2Int cellA = MaDa.argumentCellA;
        Vector2Int cellB = MaDa.argumentCellB;

        reservedCells.Add(cellA);
        reservedCells.Add(cellB);

        Vector3 posA = GetWorldPos(cellA.x, cellA.y) + Vector3.up * 1f;
        Vector3 posB = GetWorldPos(cellB.x, cellB.y) + Vector3.up * 1f;

        GameObject npcAObj = Instantiate(aggressiveNpcPrefab, posA, Quaternion.identity, buildParent);
        GameObject npcBObj = Instantiate(scaredNpcPrefab, posB, Quaternion.identity, buildParent);

        ArgumentNPC npcA = npcAObj.GetComponent<ArgumentNPC>();
        ArgumentNPC npcB = npcBObj.GetComponent<ArgumentNPC>();

        if (npcA == null || npcB == null)
        {
            Debug.LogWarning("ArgumentNPC fehlt auf einem der Streit-Prefabs.");
            return false;
        }

        NPCArgumentController controller = new GameObject("ArgumentController").AddComponent<NPCArgumentController>();
        controller.transform.SetParent(buildParent, false);
        controller.npcA = npcA;
        controller.npcB = npcB;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            controller.player = player.transform;

        ArgumentInteraction interactA = npcAObj.GetComponent<ArgumentInteraction>();
        if (interactA == null)
            interactA = npcAObj.AddComponent<ArgumentInteraction>();
        interactA.ownerNPC = npcA;
        interactA.argumentController = controller;

        ArgumentInteraction interactB = npcBObj.GetComponent<ArgumentInteraction>();
        if (interactB == null)
            interactB = npcBObj.AddComponent<ArgumentInteraction>();
        interactB.ownerNPC = npcB;
        interactB.argumentController = controller;

        return true;
    }

    void PrepareDialoguePool()
    {
        shuffledDialogues.Clear();

        for (int i = 0; i < npcDialogues.Count; i++)
        {
            if (npcDialogues[i] != null)
                shuffledDialogues.Add(npcDialogues[i]);
        }

        for (int i = 0; i < shuffledDialogues.Count; i++)
        {
            int j = Random.Range(i, shuffledDialogues.Count);
            DialogueData temp = shuffledDialogues[i];
            shuffledDialogues[i] = shuffledDialogues[j];
            shuffledDialogues[j] = temp;
        }
    }

    DialogueData GetNextDialogue()
    {
        if (shuffledDialogues.Count == 0)
        {
            Debug.LogWarning("Keine DialogueData in MazeBuilder gesetzt.");
            return null;
        }

        DialogueData result = shuffledDialogues[dialogueIndex % shuffledDialogues.Count];
        dialogueIndex++;
        return result;
    }

    Vector3 GetWorldPos(int x, int y)
    {
        return new Vector3(x * MaDa.cellSize, 0f, y * MaDa.cellSize);
    }

    void ClearOldMaze()
    {
        if (buildParent == null)
            return;

        for (int i = buildParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(buildParent.GetChild(i).gameObject);
    }
}