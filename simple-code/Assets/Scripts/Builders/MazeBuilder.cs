using System;
using System.Collections.Generic;
using TestGame.Collectibles;
using UnityEngine;

namespace TestGame.Builders
{
    public sealed class MazeBuilder : MonoBehaviour
    {
        public enum Dir { North = 0, East = 1, South = 2, West = 3 }

        [Header("Grid")]
        [SerializeField] private int rows = 8;
        [SerializeField] private int cols = 12;
        [SerializeField] private float cellSize = 4f;
        [SerializeField] private float floorThickness = 0.2f;
        [SerializeField] private float wallHeight = 10f;
        [SerializeField] private float wallThickness = 0.2f;

        [Header("Materials")]
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallMaterial;

        [Header("Placement")]
        [SerializeField] private Vector3 levelOffset = Vector3.zero;

        [Header("Build")]
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private int randomSeed;
        [SerializeField] private bool useRandomSeed = true;

        [Header("Corridor Attachment")]
        [SerializeField] private CorridorBuilder corridor;
        [SerializeField] private float corridorGap = 0.5f;

        public float Spacing => cellSize;
        public float CellHalf => cellSize * 0.5f;
        public float CellSize => cellSize;
        public int Rows => rows;
        public int Cols => cols;
        public float OffsetX { get; private set; }
        public float OffsetZ { get; private set; }
        public bool IsBuilt { get; private set; }

        public event Action MazeBuilt;

        public event Action PlayerEnteredMaze;

        private bool[,,] _mazeLayout; // [row, col, dir] = true means wall exists
        private bool[,] _visited;
        private bool[,] _occupied;

        private System.Random _rng;

        private void Start()
        {
            if (buildOnStart) Build();
        }

        public void Build()
        {
            IsBuilt = false;

            if (rows < 1) rows = 1;
            if (cols < 1) cols = 1;

            ClearChildren();

            _mazeLayout = new bool[rows, cols, 4];
            _visited = new bool[rows, cols];
            _occupied = new bool[rows, cols];

            // Initialize all walls to true.
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    for (int d = 0; d < 4; d++)
                        _mazeLayout[r, c, d] = true;

            int seed = useRandomSeed ? Environment.TickCount : randomSeed;
            _rng = new System.Random(seed);

            CarvePerfectMazeDFS();

            // Configure entrance at west wall of cell (0,0) and exit at east wall of last cell.
            _mazeLayout[0, 0, (int)Dir.West] = false;
            _mazeLayout[rows - 1, cols - 1, (int)Dir.East] = false;

            // Center around (0,0) then shift by levelOffset.
            float mazeWidth = cols * cellSize;
            float mazeDepth = rows * cellSize;
            OffsetX = -mazeWidth * 0.5f + CellHalf;
            OffsetZ = -mazeDepth * 0.5f + CellHalf;

            BuildGeometry();
            BuildEntranceTrigger();
            BuildExitTrigger();
            AttachCorridor();

            IsBuilt = true;
            MazeBuilt?.Invoke();
        }

        public Vector3 GetCellCenterWorld(int row, int col, float y = 0f)
        {
            Vector3 local = new Vector3(OffsetX + col * cellSize, y, OffsetZ + row * cellSize);
            return transform.TransformPoint(local + levelOffset);
        }

        public bool TryOccupyCell(int row, int col)
        {
            if (row < 0 || row >= rows || col < 0 || col >= cols) return false;
            if (_occupied[row, col]) return false;
            _occupied[row, col] = true;
            return true;
        }

        public bool IsWall(int row, int col, Dir dir)
        {
            return _mazeLayout[row, col, (int)dir];
        }

        public bool TryWorldToCell(Vector3 worldPos, out int row, out int col)
        {
            Vector3 local = transform.InverseTransformPoint(worldPos);
            local -= levelOffset;

            float fx = (local.x - OffsetX) / cellSize;
            float fz = (local.z - OffsetZ) / cellSize;

            col = Mathf.RoundToInt(fx);
            row = Mathf.RoundToInt(fz);

            if (row < 0 || row >= rows || col < 0 || col >= cols)
            {
                row = -1;
                col = -1;
                return false;
            }

            return true;
        }

        private void CarvePerfectMazeDFS()
        {
            Stack<(int r, int c)> stack = new Stack<(int r, int c)>();
            stack.Push((0, 0));
            _visited[0, 0] = true;

            while (stack.Count > 0)
            {
                (int r, int c) current = stack.Peek();

                List<(Dir dir, int nr, int nc)> neighbors = new List<(Dir, int, int)>(4);
                if (TryNeighbor(current.r - 1, current.c, Dir.North, out var n1)) neighbors.Add(n1);
                if (TryNeighbor(current.r, current.c + 1, Dir.East, out var n2)) neighbors.Add(n2);
                if (TryNeighbor(current.r + 1, current.c, Dir.South, out var n3)) neighbors.Add(n3);
                if (TryNeighbor(current.r, current.c - 1, Dir.West, out var n4)) neighbors.Add(n4);

                if (neighbors.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                var next = neighbors[_rng.Next(neighbors.Count)];
                RemoveWallBetween(current.r, current.c, next.nr, next.nc, next.dir);
                _visited[next.nr, next.nc] = true;
                stack.Push((next.nr, next.nc));
            }
        }

        private bool TryNeighbor(int nr, int nc, Dir dirFromCurrent, out (Dir dir, int nr, int nc) result)
        {
            result = (dirFromCurrent, nr, nc);
            if (nr < 0 || nr >= rows || nc < 0 || nc >= cols) return false;
            if (_visited[nr, nc]) return false;
            return true;
        }

        private void RemoveWallBetween(int r, int c, int nr, int nc, Dir dir)
        {
            _mazeLayout[r, c, (int)dir] = false;
            Dir opposite = Opposite(dir);
            _mazeLayout[nr, nc, (int)opposite] = false;
        }

        private static Dir Opposite(Dir d)
        {
            switch (d)
            {
                case Dir.North: return Dir.South;
                case Dir.East: return Dir.West;
                case Dir.South: return Dir.North;
                default: return Dir.East;
            }
        }

        private void BuildGeometry()
        {
            // Floor as a solid slab to ensure reliable collisions.
            float mazeWidth = cols * cellSize;
            float mazeDepth = rows * cellSize;
            float halfThickness = Mathf.Max(0.01f, floorThickness) * 0.5f;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(transform, false);
            floor.transform.localScale = new Vector3(mazeWidth, halfThickness * 2f, mazeDepth);
            floor.transform.localPosition = new Vector3(0f, -halfThickness, 0f) + levelOffset;
            ApplyMaterial(floor, floorMaterial);

            // Walls: loop all cells; create walls where mazeLayout indicates.
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Vector3 cellCenter = new Vector3(OffsetX + c * cellSize, 0f, OffsetZ + r * cellSize) + levelOffset;

                    if (_mazeLayout[r, c, (int)Dir.North])
                        CreateWall($"Wall_N_{r}_{c}", cellCenter + new Vector3(0f, wallHeight * 0.5f, -CellHalf), new Vector3(cellSize + wallThickness, wallHeight, wallThickness));
                    if (_mazeLayout[r, c, (int)Dir.South])
                        CreateWall($"Wall_S_{r}_{c}", cellCenter + new Vector3(0f, wallHeight * 0.5f, CellHalf), new Vector3(cellSize + wallThickness, wallHeight, wallThickness));
                    if (_mazeLayout[r, c, (int)Dir.West])
                        CreateWall($"Wall_W_{r}_{c}", cellCenter + new Vector3(-CellHalf, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, cellSize + wallThickness));
                    if (_mazeLayout[r, c, (int)Dir.East])
                        CreateWall($"Wall_E_{r}_{c}", cellCenter + new Vector3(CellHalf, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, cellSize + wallThickness));
                }
            }
        }

        private void BuildEntranceTrigger()
        {
            // Entrance is west wall of cell (0,0). Create a trigger just inside that opening.
            GameObject trig = new GameObject("Entrance Trigger");
            trig.transform.SetParent(transform, false);
            trig.transform.localPosition = new Vector3(OffsetX + 0 * cellSize - CellHalf + 0.75f, 1f, OffsetZ + 0 * cellSize) + levelOffset;
            var box = trig.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1.5f, 2f, 2f);

            var t = trig.AddComponent<MazeEntranceTrigger>();
            t.PlayerEntered += () => PlayerEnteredMaze?.Invoke();
        }

        private void BuildExitTrigger()
        {
            // Exit is east wall of the last cell.
            GameObject trig = new GameObject("Exit Trigger");
            trig.transform.SetParent(transform, false);
            float x = OffsetX + (cols - 1) * cellSize + CellHalf - 0.75f;
            float z = OffsetZ + (rows - 1) * cellSize;
            trig.transform.localPosition = new Vector3(x, 1f, z) + levelOffset;
            var box = trig.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1.5f, 2f, 2f);
            trig.AddComponent<MazeExitTrigger>();
        }

        private void AttachCorridor()
        {
            if (corridor == null) return;

            // Entrance center on the west side of cell (0,0).
            Vector3 entranceLocal = new Vector3(OffsetX - CellHalf, 0f, OffsetZ) + levelOffset;
            Vector3 entranceWorld = transform.TransformPoint(entranceLocal);

            // Aim corridor toward the maze (facing west side -> corridor forward points -X in maze space).
            Vector3 forward = -transform.right; // world -X relative to maze
            float halfLen = corridor.Length * 0.5f;

            // Place the corridor so its entrance sits just outside the maze entrance with an optional gap.
            Vector3 corridorPos = entranceWorld + forward * (halfLen + corridorGap);
            Quaternion corridorRot = Quaternion.LookRotation(forward, Vector3.up);

            corridor.transform.SetPositionAndRotation(corridorPos, corridorRot);

            // Rebuild corridor if it is set to build on start; this ensures updated dimensions after reposition.
            corridor.Build();
        }

        private void CreateWall(string name, Vector3 localPos, Vector3 localScale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = localPos;
            wall.transform.localScale = localScale;
            ApplyMaterial(wall, wallMaterial);
        }

        private static void ApplyMaterial(GameObject go, Material mat)
        {
            if (mat == null) return;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

                // Keep the collectibles container so the spawner persists between rebuilds.
                if (child.GetComponent<CollectibleSpawner>() != null)
                    continue;

                Destroy(child.gameObject);
            }
        }
    }
}
