using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    MazeData MaDa;
    MazeAnalyzer analyzer;
    MazePlacer placer;
    MazeBuilder builder;
    PlayerSpawner spawner;

    System.Random rng;
    Stack<Vector2Int> stack = new Stack<Vector2Int>();

    GameObject hudObject;
    GameObject dialogueObject;
    GameObject winScreenObject;
    GameObject riddleMgrObject;
    GameObject keyInventoryObject;
    GameObject questManagerObject;
    GameObject questArrowObject;

    void Awake()
    {
        MaDa = GetComponent<MazeData>();
        analyzer = GetComponent<MazeAnalyzer>();
        placer = GetComponent<MazePlacer>();
        builder = GetComponent<MazeBuilder>();
        spawner = GetComponent<PlayerSpawner>();
    }

    void Start()
    {
        GenerateFullMazeSystem();
    }

    public void GenerateFullMazeSystem()
    {
        MaDa.InitializeMaze();
        rng = new System.Random(MaDa.seed);

        GenerateMaze();

        analyzer.AnalyzeMaze();
        placer.PlaceFeatures(MaDa.seed);
        analyzer.AnalyzeMaze();

        SpawnUIManager();
        builder.BuildMaze();
        spawner.SpawnPlayerAtStart();

        if (QuestManager.Instance != null)
            QuestManager.Instance.SetNextAvailableTarget();
    }

    void SpawnUIManager()
    {
        // if (FindObjectOfType<GameTimer>() == null)
        // {
        //     GameObject timerObj = new GameObject("GameTimer");
        //     timerObj.AddComponent<GameTimer>();
        // }

        if (FindObjectOfType<HUD>() == null)
        {
            hudObject = new GameObject("HUD");
            hudObject.AddComponent<Canvas>();
            hudObject.AddComponent<HUD>();
        }

        if (RiddleManager.Instance == null)
        {
            riddleMgrObject = new GameObject("RiddleManager");
            riddleMgrObject.AddComponent<RiddleManager>();
        }

        if (KeyInventory.Instance == null)
        {
            keyInventoryObject = new GameObject("KeyInventory");
            keyInventoryObject.AddComponent<KeyInventory>();
        }

        if (DialogueUI.Instance == null)
        {
            dialogueObject = new GameObject("DialogueUI");
            dialogueObject.AddComponent<DialogueUI>();
        }

        if (QuestManager.Instance == null)
        {
            questManagerObject = new GameObject("QuestManager");
            questManagerObject.AddComponent<QuestManager>();
        }

        if (FindObjectOfType<QuestArrowUI>() == null)
        {
            questArrowObject = new GameObject("QuestArrowUI");
            questArrowObject.AddComponent<Canvas>();
            questArrowObject.AddComponent<QuestArrowUI>();
        }

        if (FindObjectOfType<WinScreen>() == null)
        {
            winScreenObject = new GameObject("WinScreen");
            WinScreen ws = winScreenObject.AddComponent<WinScreen>();

            MazeExit[] exits = FindObjectsOfType<MazeExit>();
            foreach (var exit in exits)
                exit.winScreen = ws;
        }
        else
        {
            WinScreen ws = FindObjectOfType<WinScreen>();
            MazeExit[] exits = FindObjectsOfType<MazeExit>();
            foreach (var exit in exits)
                exit.winScreen = ws;
        }
    }

    void GenerateMaze()
    {
        Vector2Int current = new Vector2Int(0, 0);
        MaDa.cells[current.x, current.y].lmaoverandert = true;

        stack.Clear();
        stack.Push(current);

        while (stack.Count > 0)
        {
            current = stack.Peek();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                Vector2Int next = neighbors[rng.Next(neighbors.Count)];
                RemoveWall(current, next);
                MaDa.cells[next.x, next.y].lmaoverandert = true;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int current)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        int x = current.x;
        int y = current.y;

        if (y + 1 < MaDa.h && !MaDa.cells[x, y + 1].lmaoverandert)
            neighbors.Add(new Vector2Int(x, y + 1));

        if (y - 1 >= 0 && !MaDa.cells[x, y - 1].lmaoverandert)
            neighbors.Add(new Vector2Int(x, y - 1));

        if (x + 1 < MaDa.w && !MaDa.cells[x + 1, y].lmaoverandert)
            neighbors.Add(new Vector2Int(x + 1, y));

        if (x - 1 >= 0 && !MaDa.cells[x - 1, y].lmaoverandert)
            neighbors.Add(new Vector2Int(x - 1, y));

        return neighbors;
    }

    void RemoveWall(Vector2Int current, Vector2Int next)
    {
        if (next.x == current.x + 1)
        {
            MaDa.cells[current.x, current.y].rightw = false;
            MaDa.cells[next.x, next.y].linksw = false;
        }
        else if (next.x == current.x - 1)
        {
            MaDa.cells[current.x, current.y].linksw = false;
            MaDa.cells[next.x, next.y].rightw = false;
        }
        else if (next.y == current.y + 1)
        {
            MaDa.cells[current.x, current.y].upw = false;
            MaDa.cells[next.x, next.y].downw = false;
        }
        else if (next.y == current.y - 1)
        {
            MaDa.cells[current.x, current.y].downw = false;
            MaDa.cells[next.x, next.y].upw = false;
        }
    }
}