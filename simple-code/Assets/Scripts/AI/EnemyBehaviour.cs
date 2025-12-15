using System.Collections;
using System.Collections.Generic;
using TestGame.Builders;
using TestGame.Core;
using UnityEngine;

namespace TestGame.AI
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class EnemyBehaviour : MonoBehaviour
    {
        public enum Mode
        {
            Patrol,
            Chase
        }

        [Header("References")]
        [SerializeField] private MazeBuilder maze;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject modelPrefab;

        [Header("Spawn")]
        [SerializeField] private float spawnDelayAfterMazeEntry = 30f;
        [SerializeField] private AudioClip releaseSfx;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float chaseDistance = 10f;

        [Header("State")]
        [SerializeField] private Mode mode = Mode.Patrol;

        private CharacterController _cc;
        private bool _spawnStarted;
        private bool _released;
        private GameObject _modelInstance;
        private AudioSource _audio;

        // Grid pathfinding state (scaffold-level)
        private readonly List<Vector3> _pathWorld = new List<Vector3>();
        private int _pathIndex;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _audio = GetComponent<AudioSource>();
            if (_audio == null)
                _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
        }

        private void Start()
        {
            if (maze == null) maze = FindFirstObjectByType<MazeBuilder>();
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }

            if (maze != null)
                maze.PlayerEnteredMaze += OnPlayerEnteredMaze;

            // Stay active so coroutines/events still run, but keep controller/model disabled until release.
            PrepareForRelease();
        }

        private void OnDestroy()
        {
            if (maze != null)
                maze.PlayerEnteredMaze -= OnPlayerEnteredMaze;
        }

        private void OnPlayerEnteredMaze()
        {
            if (_spawnStarted) return;
            _spawnStarted = true;
            StartCoroutine(SpawnAfterDelay());
        }

        private IEnumerator SpawnAfterDelay()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, spawnDelayAfterMazeEntry));

            if (maze == null) yield break;
            if (player == null) yield break;

            // Spawn roughly at the far end (near exit cell) by default.
            int r = Mathf.Max(0, maze.Rows - 1);
            int c = Mathf.Max(0, maze.Cols - 1);
            Vector3 spawnPos = maze.GetCellCenterWorld(r, c, 1f);
            transform.position = spawnPos;

            EnsureModel(active: true);
            _cc.enabled = true;
            _released = true;
            PlayReleaseSfx();

            RebuildPathToPlayer();
        }

        private void PrepareForRelease()
        {
            EnsureModel(active: false);
            _cc.enabled = false;
            _released = false;
        }

        private void EnsureModel(bool active)
        {
            if (_modelInstance == null)
            {
                if (modelPrefab == null) return;
                _modelInstance = Instantiate(modelPrefab, transform);
                _modelInstance.transform.localPosition = Vector3.zero;
                _modelInstance.transform.localRotation = Quaternion.identity;
            }
            _modelInstance.SetActive(active);
        }

        private void PlayReleaseSfx()
        {
            if (releaseSfx == null || _audio == null) return;
            _audio.PlayOneShot(releaseSfx);
        }

        private void Update()
        {
            if (!_released) return;
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.position);
            mode = dist <= chaseDistance ? Mode.Chase : Mode.Patrol;

            if (mode == Mode.Chase)
            {
                if (_pathWorld.Count == 0 || Time.frameCount % 20 == 0)
                    RebuildPathToPlayer();
            }

            MoveAlongPath();
        }

        private void MoveAlongPath()
        {
            if (_pathWorld.Count == 0) return;
            if (_pathIndex >= _pathWorld.Count) _pathIndex = 0;

            Vector3 target = _pathWorld[_pathIndex];
            Vector3 to = target - transform.position;
            to.y = 0f;

            if (to.magnitude < 0.3f)
            {
                _pathIndex++;
                return;
            }

            Vector3 dir = to.normalized;
            _cc.SimpleMove(dir * moveSpeed);
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        private void RebuildPathToPlayer()
        {
            _pathWorld.Clear();
            _pathIndex = 0;

            if (maze == null || player == null)
            {
                _pathWorld.Add(player != null ? player.position : transform.position);
                return;
            }

            if (!maze.TryWorldToCell(transform.position, out int sr, out int sc) ||
                !maze.TryWorldToCell(player.position, out int tr, out int tc))
            {
                _pathWorld.Add(player.position);
                return;
            }

            if (sr == tr && sc == tc)
            {
                _pathWorld.Add(player.position);
                return;
            }

            if (!TryAStar(sr, sc, tr, tc, out List<(int r, int c)> cellPath))
            {
                _pathWorld.Add(player.position);
                return;
            }

            // Convert to world points (skip the first cell if it's our current cell).
            for (int i = 0; i < cellPath.Count; i++)
            {
                if (i == 0 && cellPath[i].r == sr && cellPath[i].c == sc) continue;
                Vector3 p = maze.GetCellCenterWorld(cellPath[i].r, cellPath[i].c, 1f);
                _pathWorld.Add(p);
            }

            // Always end exactly at the player's current position for smoother chasing.
            _pathWorld.Add(player.position);
        }

        private bool TryAStar(int sr, int sc, int tr, int tc, out List<(int r, int c)> path)
        {
            path = new List<(int r, int c)>();

            var open = new SimplePriorityQueue();
            var cameFrom = new Dictionary<(int r, int c), (int r, int c)>();
            var gScore = new Dictionary<(int r, int c), int>();

            (int r, int c) start = (sr, sc);
            (int r, int c) goal = (tr, tc);

            gScore[start] = 0;
            open.Enqueue(start, Heuristic(start, goal));

            int safety = maze.Rows * maze.Cols * 20;
            while (open.Count > 0 && safety-- > 0)
            {
                var current = open.Dequeue();
                if (current == goal)
                {
                    Reconstruct(cameFrom, current, path);
                    return true;
                }

                foreach (var next in GetNeighbors(current.r, current.c))
                {
                    int tentative = gScore[current] + 1;
                    if (!gScore.TryGetValue(next, out int best) || tentative < best)
                    {
                        cameFrom[next] = current;
                        gScore[next] = tentative;
                        int f = tentative + Heuristic(next, goal);
                        open.EnqueueOrDecrease(next, f);
                    }
                }
            }

            return false;
        }

        private IEnumerable<(int r, int c)> GetNeighbors(int r, int c)
        {
            // North
            if (r > 0 && !maze.IsWall(r, c, MazeBuilder.Dir.North))
                yield return (r - 1, c);
            // East
            if (c < maze.Cols - 1 && !maze.IsWall(r, c, MazeBuilder.Dir.East))
                yield return (r, c + 1);
            // South
            if (r < maze.Rows - 1 && !maze.IsWall(r, c, MazeBuilder.Dir.South))
                yield return (r + 1, c);
            // West
            if (c > 0 && !maze.IsWall(r, c, MazeBuilder.Dir.West))
                yield return (r, c - 1);
        }

        private static int Heuristic((int r, int c) a, (int r, int c) b)
        {
            return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.c - b.c);
        }

        private static void Reconstruct(Dictionary<(int r, int c), (int r, int c)> cameFrom, (int r, int c) current, List<(int r, int c)> outPath)
        {
            outPath.Clear();
            outPath.Add(current);
            while (cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                outPath.Add(current);
            }
            outPath.Reverse();
        }

        private sealed class SimplePriorityQueue
        {
            // Small, allocation-light PQ good enough for grid sizes used here.
            private readonly List<(int r, int c)> _items = new List<(int r, int c)>();
            private readonly List<int> _prio = new List<int>();
            private readonly Dictionary<(int r, int c), int> _index = new Dictionary<(int r, int c), int>();

            public int Count => _items.Count;

            public void Enqueue((int r, int c) node, int priority)
            {
                if (_index.ContainsKey(node))
                {
                    EnqueueOrDecrease(node, priority);
                    return;
                }

                _index[node] = _items.Count;
                _items.Add(node);
                _prio.Add(priority);
            }

            public void EnqueueOrDecrease((int r, int c) node, int priority)
            {
                if (_index.TryGetValue(node, out int idx))
                {
                    if (priority < _prio[idx]) _prio[idx] = priority;
                    return;
                }

                Enqueue(node, priority);
            }

            public (int r, int c) Dequeue()
            {
                int bestIdx = 0;
                int bestPrio = _prio[0];
                for (int i = 1; i < _prio.Count; i++)
                {
                    if (_prio[i] < bestPrio)
                    {
                        bestPrio = _prio[i];
                        bestIdx = i;
                    }
                }

                var node = _items[bestIdx];
                RemoveAt(bestIdx);
                return node;
            }

            private void RemoveAt(int idx)
            {
                var node = _items[idx];
                _index.Remove(node);

                int last = _items.Count - 1;
                if (idx != last)
                {
                    var lastNode = _items[last];
                    _items[idx] = lastNode;
                    _prio[idx] = _prio[last];
                    _index[lastNode] = idx;
                }

                _items.RemoveAt(last);
                _prio.RemoveAt(last);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                GameManager.Instance?.LoadYouLost();
            }
        }
    }
}
