using System.Collections.Generic;
using UnityEngine;

public class MazePlacer : MonoBehaviour
{
    MazeData MaDa;
    MazeAnalyzer analyzer;
    System.Random rng;

    [Header("Placement Settings")]
    public int npcCount = 3;
    public int minNpcDistanceFromStart = 5;

    void Awake()
    {
        MaDa = GetComponent<MazeData>();
        analyzer = GetComponent<MazeAnalyzer>();
    }

    public void PlaceFeatures(int seed)
    {
        rng = new System.Random(seed);

        ClearOldPlacements();
        PlaceStart();
        analyzer.AnalyzeMaze();
        PlaceExit();
        analyzer.AnalyzeMaze();
        PlaceNpcs();
    }

    void ClearOldPlacements()
    {
        for (int x = 0; x < MaDa.w; x++)
        {
            for (int y = 0; y < MaDa.h; y++)
            {
                MaDa.cells[x, y].isStart = false;
                MaDa.cells[x, y].isExit = false;
                MaDa.cells[x, y].hasNpcSpawn = false;
            }
        }

        MaDa.npcSpawnCells.Clear();
        MaDa.argumentCellA = new Vector2Int(-1, -1);
        MaDa.argumentCellB = new Vector2Int(-1, -1);
    }

    void PlaceStart()
    {
        MaDa.startCell = new Vector2Int(0, 0);
        MaDa.cells[MaDa.startCell.x, MaDa.startCell.y].isStart = true;
    }

    void PlaceExit()
    {
        MaDa.exitCell = analyzer.FindFarthestCellFromStart();
        MaDa.cells[MaDa.exitCell.x, MaDa.exitCell.y].isExit = true;
    }

    void PlaceNpcs()
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 0; x < MaDa.w; x++)
        {
            for (int y = 0; y < MaDa.h; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                CellData cell = MaDa.cells[x, y];

                if (pos == MaDa.startCell)
                    continue;

                if (pos == MaDa.exitCell)
                    continue;

                if (cell.distanceFromStart < minNpcDistanceFromStart)
                    continue;

                candidates.Add(pos);
            }
        }

        Shuffle(candidates);

        HashSet<Vector2Int> reservedCells = new HashSet<Vector2Int>();

        bool pairReserved = ReserveArgumentPairGuaranteed(candidates, reservedCells);

        List<Vector2Int> remainingCandidates = new List<Vector2Int>();

        for (int i = 0; i < candidates.Count; i++)
        {
            if (!reservedCells.Contains(candidates[i]))
                remainingCandidates.Add(candidates[i]);
        }

        Shuffle(remainingCandidates);

        int normalNpcAmount = Mathf.Min(npcCount, remainingCandidates.Count);

        for (int i = 0; i < normalNpcAmount; i++)
        {
            Vector2Int pos = remainingCandidates[i];
            MaDa.cells[pos.x, pos.y].hasNpcSpawn = true;
            MaDa.npcSpawnCells.Add(pos);
        }

        Debug.Log("Argument pair reserved: " + pairReserved);
        Debug.Log("ArgumentCellA: " + MaDa.argumentCellA);
        Debug.Log("ArgumentCellB: " + MaDa.argumentCellB);
        Debug.Log("Normal NPC count placed: " + normalNpcAmount);
        Debug.Log("Total npcSpawnCells: " + MaDa.npcSpawnCells.Count);
    }

    bool ReserveArgumentPairGuaranteed(List<Vector2Int> candidates, HashSet<Vector2Int> reservedCells)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            Vector2Int a = candidates[i];

            List<Vector2Int> neighbors = GetValidConnectedNeighbors(a);

            if (neighbors.Count == 0)
                continue;

            Vector2Int b = neighbors[rng.Next(neighbors.Count)];

            MaDa.argumentCellA = a;
            MaDa.argumentCellB = b;

            reservedCells.Add(a);
            reservedCells.Add(b);

            MaDa.cells[a.x, a.y].hasNpcSpawn = true;
            MaDa.cells[b.x, b.y].hasNpcSpawn = true;

            MaDa.npcSpawnCells.Add(a);
            MaDa.npcSpawnCells.Add(b);

            Debug.Log("Reserved argument pair at " + a + " and " + b);
            return true;
        }

        Debug.LogWarning("No valid connected argument pair found.");
        return false;
    }

    List<Vector2Int> GetValidConnectedNeighbors(Vector2Int a)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        Vector2Int[] possible =
        {
            new Vector2Int(a.x + 1, a.y),
            new Vector2Int(a.x - 1, a.y),
            new Vector2Int(a.x, a.y + 1),
            new Vector2Int(a.x, a.y - 1)
        };

        for (int i = 0; i < possible.Length; i++)
        {
            Vector2Int b = possible[i];

            if (!IsInsideMaze(b))
                continue;

            if (b == MaDa.startCell)
                continue;

            if (b == MaDa.exitCell)
                continue;

            if (MaDa.cells[b.x, b.y].distanceFromStart < minNpcDistanceFromStart)
                continue;

            if (AreCellsConnected(a, b))
                result.Add(b);
        }

        return result;
    }

    bool IsInsideMaze(Vector2Int p)
    {
        return p.x >= 0 && p.x < MaDa.w && p.y >= 0 && p.y < MaDa.h;
    }

    bool AreCellsConnected(Vector2Int a, Vector2Int b)
    {
        if (a.x == b.x)
        {
            if (a.y + 1 == b.y)
                return !MaDa.cells[a.x, a.y].upw && !MaDa.cells[b.x, b.y].downw;

            if (a.y - 1 == b.y)
                return !MaDa.cells[a.x, a.y].downw && !MaDa.cells[b.x, b.y].upw;
        }

        if (a.y == b.y)
        {
            if (a.x + 1 == b.x)
                return !MaDa.cells[a.x, a.y].rightw && !MaDa.cells[b.x, b.y].linksw;

            if (a.x - 1 == b.x)
                return !MaDa.cells[a.x, a.y].linksw && !MaDa.cells[b.x, b.y].rightw;
        }

        return false;
    }

    void Shuffle(List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = rng.Next(i, list.Count);
            Vector2Int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}