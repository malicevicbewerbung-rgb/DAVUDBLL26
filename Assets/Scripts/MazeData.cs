using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CellData
{
    public int x;
    public int y;

    public bool lmaoverandert = false;
    public bool exists = true;

    public bool upw = true;
    public bool downw = true;
    public bool rightw = true;
    public bool linksw = true;

    public bool isStart = false;
    public bool isExit = false;
    public bool hasNpcSpawn = false;

    public int distanceFromStart = -1;
    public bool isDeadEnd = false;
    public bool isJunction = false;

    public CellData(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class MazeData : MonoBehaviour
{
    [Header("Maze Settings")]
    public int w = 20;
    public int h = 20;
    public float cellSize = 4f;

    [Header("Seed Settings")]
    public bool useRandomSeed = true;
    public int seed;

    [Header("Maze Data")]
    public CellData[,] cells;

    public Vector2Int startCell;
    public Vector2Int exitCell;
    public List<Vector2Int> npcSpawnCells = new List<Vector2Int>();

    [Header("Argument NPC Pair")]
    public Vector2Int argumentCellA = new Vector2Int(-1, -1);
    public Vector2Int argumentCellB = new Vector2Int(-1, -1);

    public void InitializeMaze()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }

        cells = new CellData[w, h];
        npcSpawnCells.Clear();

        argumentCellA = new Vector2Int(-1, -1);
        argumentCellB = new Vector2Int(-1, -1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                cells[x, y] = new CellData(x, y);
            }
        }

        startCell = new Vector2Int(0, 0);
        exitCell = new Vector2Int(w - 1, h - 1);
    }
}