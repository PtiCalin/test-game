using System.Collections.Generic;
using TestGame.Builders;
using UnityEngine;

namespace TestGame.Collectibles
{
    public sealed class CollectibleSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MazeBuilder maze;

        [Header("Counts")]
        [SerializeField] private int coinCount = 30;
        [SerializeField] private int treasureCount = 5;

        [Header("Prefabs")]
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private GameObject treasurePrefab;

        [Header("Spawn")]
        [SerializeField] private float spawnY = 1f;

        private void Start()
        {
            if (maze == null) maze = FindFirstObjectByType<MazeBuilder>();
            if (maze == null) return;

            Spawn(coinPrefab, coinCount, "Coins");
            Spawn(treasurePrefab, treasureCount, "Treasures");
        }

        private void Spawn(GameObject prefab, int count, string containerName)
        {
            if (prefab == null || count <= 0) return;

            // Clear any previous container to prevent duplicate sets when rebuilding.
            Transform existing = transform.Find(containerName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            GameObject container = new GameObject(containerName);
            container.transform.SetParent(transform, false);

            // Simple retry-based placement using occupancy tracking.
            int placed = 0;
            int safety = Mathf.Max(100, count * 20);

            while (placed < count && safety-- > 0)
            {
                int r = Random.Range(0, maze.Rows);
                int c = Random.Range(0, maze.Cols);

                // Keep entrance/exit cleaner.
                if ((r == 0 && c == 0) || (r == maze.Rows - 1 && c == maze.Cols - 1))
                    continue;

                if (!maze.TryOccupyCell(r, c))
                    continue;

                Vector3 pos = maze.GetCellCenterWorld(r, c, spawnY);
                Instantiate(prefab, pos, Quaternion.identity, container.transform);
                placed++;
            }
        }
    }
}
