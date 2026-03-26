using System.Collections.Generic;
using UnityEngine;

public class MazeAnalyzer : MonoBehaviour
{
    MazeData MaDa;

    void Awake()
    {
        MaDa = GetComponent<MazeData>();
    }

    public void AnalyzeMaze()
    {
        MarkCellTypes();
        CalculateDistancesFromStart();
    }

    void MarkCellTypes()
    {
        for (int x = 0; x < MaDa.w; x++)
        {
            for (int y = 0; y < MaDa.h; y++)
            {
                CellData cell = MaDa.cells[x, y];
                int openCount = GetOpenNeighborCount(new Vector2Int(x, y));

                cell.isDeadEnd = openCount == 1;
                cell.isJunction = openCount >= 3;
            }
        }
    }

    public int GetOpenNeighborCount(Vector2Int pos)
    {
        int count = 0;
        CellData cell = MaDa.cells[pos.x, pos.y];

        if (!cell.upw && pos.y + 1 < MaDa.h) count++;
        if (!cell.downw && pos.y - 1 >= 0) count++;
        if (!cell.rightw && pos.x + 1 < MaDa.w) count++;
        if (!cell.linksw && pos.x - 1 >= 0) count++;

        return count;
    }

    void CalculateDistancesFromStart()
    {
        for (int x = 0; x < MaDa.w; x++)
        {
            for (int y = 0; y < MaDa.h; y++)
            {
                MaDa.cells[x, y].distanceFromStart = -1;
            }
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(MaDa.startCell);
        MaDa.cells[MaDa.startCell.x, MaDa.startCell.y].distanceFromStart = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int currentDistance = MaDa.cells[current.x, current.y].distanceFromStart;

            foreach (Vector2Int neighbor in GetConnectedNeighbors(current))
            {
                if (MaDa.cells[neighbor.x, neighbor.y].distanceFromStart == -1)
                {
                    MaDa.cells[neighbor.x, neighbor.y].distanceFromStart = currentDistance + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    public Vector2Int FindFarthestCellFromStart()
    {
        Vector2Int farthest = MaDa.startCell;
        int bestDistance = -1;

        for (int x = 0; x < MaDa.w; x++)
        {
            for (int y = 0; y < MaDa.h; y++)
            {
                int d = MaDa.cells[x, y].distanceFromStart;
                if (d > bestDistance)
                {
                    bestDistance = d;
                    farthest = new Vector2Int(x, y);
                }
            }
        }

        return farthest;
    }

    public List<Vector2Int> GetConnectedNeighbors(Vector2Int pos)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        CellData cell = MaDa.cells[pos.x, pos.y];

        if (!cell.upw && pos.y + 1 < MaDa.h) result.Add(new Vector2Int(pos.x, pos.y + 1));
        if (!cell.downw && pos.y - 1 >= 0) result.Add(new Vector2Int(pos.x, pos.y - 1));
        if (!cell.rightw && pos.x + 1 < MaDa.w) result.Add(new Vector2Int(pos.x + 1, pos.y));
        if (!cell.linksw && pos.x - 1 >= 0) result.Add(new Vector2Int(pos.x - 1, pos.y));

        return result;
    }
}