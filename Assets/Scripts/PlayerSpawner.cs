using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    MazeData MaDa;

    [Header("Player")]
    public Transform playerTransform;
    public float playerY = 1f;

    void Awake()
    {
        MaDa = GetComponent<MazeData>();
    }

    public void SpawnPlayerAtStart()
    {
        if (MaDa == null)
        {
            Debug.LogError("MazeData not found on this GameObject.");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("Player Transform not assigned.");
            return;
        }

        Vector3 spawnPos = new Vector3(
            MaDa.startCell.x * MaDa.cellSize,
            playerY,
            MaDa.startCell.y * MaDa.cellSize
        );

        playerTransform.position = spawnPos;
    }
}