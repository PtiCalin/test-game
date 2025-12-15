using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a procedural maze for the castle scene, spawns the player, and scatters collectibles.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
	[Header("Ground")]
	[SerializeField] private Vector3 groundScale = new Vector3(5, 1, 5);
	[SerializeField] private Material groundMaterial;
	[SerializeField] private Vector3 levelOffset = new Vector3(60f, 0f, 0f);

	[Header("Player")]
	[SerializeField] private Vector3 playerStartPosition = new Vector3(0, 1f, 0);
	[SerializeField] private float playerMoveSpeed = 7f;
	[SerializeField] private GameObject playerPrefab;

	[Header("Collectibles")]
	[SerializeField] private int numberOfCoins = 10;
	[SerializeField] private int numberOfTreasures = 3;
	[SerializeField] private int coinPointsValue = 10;
	[SerializeField] private int treasurePointsValue = 50;
	[SerializeField] private GameObject coinPrefab;
	[SerializeField] private GameObject treasurePrefab;
	[SerializeField] private Material coinMaterial;
	[SerializeField] private Material treasureMaterial;

	[Header("Maze")]
	[SerializeField, Min(2)] private int mazeRows = 12;
	[SerializeField, Min(2)] private int mazeColumns = 12;
	[SerializeField, Min(1f)] private float cellSize = 4f;
	[SerializeField] private float wallHeight = 10f;
	[SerializeField] private float wallThickness = 0.5f;
	[SerializeField] private Material wallMaterial;

	private GameObject levelParent;
	private bool[,,] mazeLayout;
	private int rows, columns;
	private float spacing, cellHalf, offsetX, offsetZ;
	private bool[,] occupiedCells;
	private List<Vector2Int> availableSpawnCells;
	private Vector2Int entranceCell, exitCell;

	private enum MazeDirection { North = 0, South, East, West }

	private void Start()
	{
		levelParent = new GameObject("Generated Level");
		CreateGround();
		CreateMaze();
		CreatePlayer();
		CreateCollectibles();
	}

	private void CreateGround()
	{
		GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
		ground.name = "Ground";
		ground.transform.SetParent(levelParent.transform);
		ground.transform.position = levelOffset;
		ground.transform.localScale = groundScale;

		if (groundMaterial != null)
			ground.GetComponent<Renderer>().material = groundMaterial;
	}

	private void CreateMaze()
	{
		rows = Mathf.Max(mazeRows, 2);
		columns = Mathf.Max(mazeColumns, 2);
		spacing = Mathf.Max(cellSize, 1f);

		GameObject mazeParent = new GameObject("Maze");
		mazeParent.transform.SetParent(levelParent.transform);

		mazeLayout = GenerateMazeLayout(rows, columns);
		ConfigureEntranceAndExit();
		BuildMazeGeometry(mazeParent);

		ReserveCell(entranceCell.x, entranceCell.y);
		ReserveCell(exitCell.x, exitCell.y);

		// Camera rig integration removed to avoid dependency on IFT2720.GameCamera.
	}

	private void CreateWall(Vector3 position, Vector3 scale, GameObject parent)
	{
		GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		wall.name = "Wall";
		wall.transform.position = position + levelOffset;
		wall.transform.localScale = scale;
		wall.transform.SetParent(parent.transform);

		if (wallMaterial != null)
			wall.GetComponent<Renderer>().material = wallMaterial;
	}

	private bool[,,] GenerateMazeLayout(int r, int c)
	{
		bool[,,] layout = new bool[r, c, 4];
		for (int row = 0; row < r; row++)
			for (int col = 0; col < c; col++)
				for (int dir = 0; dir < 4; dir++)
					layout[row, col, dir] = true;

		bool[,] visited = new bool[r, c];
		Stack<Vector2Int> stack = new Stack<Vector2Int>();

		Vector2Int startCell = new Vector2Int(0, 0);
		visited[startCell.x, startCell.y] = true;
		stack.Push(startCell);

		while (stack.Count > 0)
		{
			Vector2Int currentCell = stack.Peek();
			List<(MazeDirection direction, Vector2Int cell)> neighbors = GetUnvisitedNeighbors(currentCell, visited, r, c);

			if (neighbors.Count > 0)
			{
				int randomIndex = Random.Range(0, neighbors.Count);
				MazeDirection direction = neighbors[randomIndex].direction;
				Vector2Int nextCell = neighbors[randomIndex].cell;

				RemoveWallBetween(layout, currentCell, nextCell, direction);
				visited[nextCell.x, nextCell.y] = true;
				stack.Push(nextCell);
			}
			else
			{
				stack.Pop();
			}
		}

		return layout;
	}

	private List<(MazeDirection direction, Vector2Int cell)> GetUnvisitedNeighbors(Vector2Int cell, bool[,] visited, int r, int c)
	{
		var neighbors = new List<(MazeDirection, Vector2Int)>();
		int row = cell.x, col = cell.y;

		if (row + 1 < r && !visited[row + 1, col]) neighbors.Add((MazeDirection.North, new Vector2Int(row + 1, col)));
		if (col + 1 < c && !visited[row, col + 1]) neighbors.Add((MazeDirection.East, new Vector2Int(row, col + 1)));
		if (row > 0 && !visited[row - 1, col]) neighbors.Add((MazeDirection.South, new Vector2Int(row - 1, col)));
		if (col > 0 && !visited[row, col - 1]) neighbors.Add((MazeDirection.West, new Vector2Int(row, col - 1)));

		return neighbors;
	}

	private void RemoveWallBetween(bool[,,] layout, Vector2Int current, Vector2Int next, MazeDirection dir)
	{
		layout[current.x, current.y, (int)dir] = false;
		layout[next.x, next.y, (int)GetOppositeDirection(dir)] = false;
	}

	private MazeDirection GetOppositeDirection(MazeDirection dir) => dir switch
	{
		MazeDirection.North => MazeDirection.South,
		MazeDirection.South => MazeDirection.North,
		MazeDirection.East => MazeDirection.West,
		_ => MazeDirection.East
	};

	private void BuildMazeGeometry(GameObject parent)
	{
		cellHalf = spacing * 0.5f;
		offsetX = -columns * spacing * 0.5f;
		offsetZ = -rows * spacing * 0.5f;

		for (int row = 0; row < rows; row++)
		{
			for (int col = 0; col < columns; col++)
			{
				Vector3 center = new Vector3(offsetX + col * spacing + cellHalf, wallHeight * 0.5f, offsetZ + row * spacing + cellHalf);

				if (mazeLayout[row, col, (int)MazeDirection.North])
					CreateWall(center + new Vector3(0, 0, cellHalf), new Vector3(spacing, wallHeight, wallThickness), parent);
				if (mazeLayout[row, col, (int)MazeDirection.East])
					CreateWall(center + new Vector3(cellHalf, 0, 0), new Vector3(wallThickness, wallHeight, spacing), parent);
				if (row == 0 && mazeLayout[row, col, (int)MazeDirection.South])
					CreateWall(center - new Vector3(0, 0, cellHalf), new Vector3(spacing, wallHeight, wallThickness), parent);
				if (col == 0 && mazeLayout[row, col, (int)MazeDirection.West])
					CreateWall(center - new Vector3(cellHalf, 0, 0), new Vector3(wallThickness, wallHeight, spacing), parent);
			}
		}

		occupiedCells = new bool[rows, columns];
		availableSpawnCells = null;
	}

	private void ConfigureEntranceAndExit()
	{
		entranceCell = new Vector2Int(0, 0);
		exitCell = new Vector2Int(rows - 1, columns - 1);
		mazeLayout[0, 0, (int)MazeDirection.West] = false;
		mazeLayout[rows - 1, columns - 1, (int)MazeDirection.East] = false;
	}

	private Vector3 GetCellCenter(int row, int col)
	{
		row = Mathf.Clamp(row, 0, rows - 1);
		col = Mathf.Clamp(col, 0, columns - 1);
		return new Vector3(offsetX + col * spacing + cellHalf, 0f, offsetZ + row * spacing + cellHalf) + levelOffset;
	}

	private void CreatePlayer()
	{
		var existing = GameObject.FindWithTag("Player");

		// If a player already exists in the scene (e.g., placed in the corridor), reuse it.
		if (existing != null)
		{
			EnsurePlayerComponents(existing);
			ConfigureCamera(existing.transform);
			ReserveCell(entranceCell.x, entranceCell.y);
			return;
		}

		GameObject player = playerPrefab != null
			? Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, levelParent.transform)
			: new GameObject("Player");

		player.name = "Player";
		player.tag = "Player";
		Vector3 spawnPos = GetCellCenter(entranceCell.x, entranceCell.y);
		player.transform.SetParent(levelParent.transform);

		Rigidbody rb = player.GetComponent<Rigidbody>();
		if (rb == null)
			rb = player.AddComponent<Rigidbody>();
		rb.mass = 1f;
		rb.linearDamping = 0f;
		rb.angularDamping = 0.05f;
		rb.useGravity = true;
		rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

		Collider col = player.GetComponentInChildren<Collider>();
		if (col == null)
			col = player.AddComponent<CapsuleCollider>();
		else if (col.isTrigger)
			col.isTrigger = false;

		float heightOffset = 0.5f;
		Collider groundedCollider = player.GetComponentInChildren<Collider>();
		if (groundedCollider != null)
		{
			Bounds b = groundedCollider.bounds;
			heightOffset = b.extents.y + 0.001f;
		}

		spawnPos.y = Mathf.Max(heightOffset, playerStartPosition.y);
		player.transform.position = spawnPos;

		ReserveCell(entranceCell.x, entranceCell.y);

		EnsurePlayerComponents(player);
		ConfigureCamera(player.transform);
	}

	private void EnsurePlayerComponents(GameObject player)
	{
		// Ensure the CharacterController-based Player is present and configured.
		var controller = player.GetComponent<Player>() ?? player.AddComponent<Player>();
		var cc = player.GetComponent<CharacterController>() ?? player.AddComponent<CharacterController>();
		controller.Speed = playerMoveSpeed;
		controller.SetCamera(Camera.main != null ? Camera.main.transform : null);
	}

	private void ConfigureCamera(Transform target)
	{
		var cam = Camera.main;
		if (cam == null || target == null)
			return;

		var follow = cam.GetComponent<GameCamera>();
		if (follow != null)
		{
			follow.SetTarget(target);
		}
	}

	private void CreateCollectibles()
	{
		GameObject parent = new GameObject("Collectibles");
		parent.transform.SetParent(levelParent.transform);

		EnsureSpawnCellPool();

		int coinPlaced = 0;
		int coinAttempts = 0;
		while (coinPlaced < numberOfCoins && coinAttempts < numberOfCoins * 4)
		{
			coinAttempts++;
			if (!TryReserveSpawnPosition(1f, 0.35f, out Vector3 pos))
				break;

			GameObject coin = coinPrefab != null ? Instantiate(coinPrefab, pos, Quaternion.identity, parent.transform)
				: GameObject.CreatePrimitive(PrimitiveType.Sphere);

			coin.name = coinPrefab != null ? coinPrefab.name : "Coin";
			coin.transform.position = pos;
			coin.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			coin.transform.SetParent(parent.transform);

			Collider col = coin.GetComponentInChildren<Collider>();
			if (col == null)
				col = coin.AddComponent<SphereCollider>();
			col.isTrigger = true;

			Renderer coinRenderer = coin.GetComponentInChildren<Renderer>();
			if (coinRenderer == null)
			{
				coinRenderer = coin.AddComponent<MeshRenderer>();
				coin.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
			}

			if (coinMaterial != null)
				coinRenderer.material = coinMaterial;
			else if (coinPrefab == null)
				coinRenderer.material.color = Color.yellow;

			Collectible c = coin.GetComponent<Collectible>() ?? coin.AddComponent<Collectible>();
			c.Configure(false, coinPointsValue, 100f, 2f, 0.3f);
			coinPlaced++;
		}

		int treasurePlaced = 0;
		int treasureAttempts = 0;
		while (treasurePlaced < numberOfTreasures && treasureAttempts < numberOfTreasures * 4)
		{
			treasureAttempts++;
			if (!TryReserveSpawnPosition(1.5f, 0.45f, out Vector3 pos))
				break;

			GameObject treasure = treasurePrefab != null ? Instantiate(treasurePrefab, pos, Quaternion.identity, parent.transform)
				: GameObject.CreatePrimitive(PrimitiveType.Cube);

			treasure.name = treasurePrefab != null ? treasurePrefab.name : "Treasure";
			treasure.transform.position = pos;
			treasure.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
			treasure.transform.SetParent(parent.transform);

			Collider col = treasure.GetComponentInChildren<Collider>();
			if (col == null)
				col = treasure.AddComponent<BoxCollider>();
			col.isTrigger = true;

			Renderer treasureRenderer = treasure.GetComponentInChildren<Renderer>();
			if (treasureRenderer == null)
			{
				treasureRenderer = treasure.AddComponent<MeshRenderer>();
				treasure.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
			}

			if (treasureMaterial != null)
				treasureRenderer.material = treasureMaterial;
			else if (treasurePrefab == null)
				treasureRenderer.material.color = new Color(1f, 0.5f, 0f);

			Collectible c = treasure.GetComponent<Collectible>() ?? treasure.AddComponent<Collectible>();
			c.Configure(true, treasurePointsValue, 80f, 1.5f, 0.4f);
			treasurePlaced++;
		}

		if (coinPlaced + treasurePlaced == 0)
			Debug.LogWarning("No coins or treasures were spawned. Check counts and cell availability.");
		else
			Debug.Log($"{coinPlaced} coins and {treasurePlaced} treasures created");
	}

	private void ReserveCell(int row, int col)
	{
		if (occupiedCells != null && row >= 0 && row < occupiedCells.GetLength(0) && col >= 0 && col < occupiedCells.GetLength(1))
			occupiedCells[row, col] = true;
	}

	private void EnsureSpawnCellPool()
	{
		if (rows <= 0 || columns <= 0) return;

		if (occupiedCells == null || occupiedCells.GetLength(0) != rows || occupiedCells.GetLength(1) != columns)
			occupiedCells = new bool[rows, columns];

		availableSpawnCells ??= new List<Vector2Int>();
		availableSpawnCells.Clear();

		for (int r = 0; r < rows; r++)
			for (int c = 0; c < columns; c++)
				if (!occupiedCells[r, c])
					availableSpawnCells.Add(new Vector2Int(r, c));
	}

	private bool TryReserveSpawnPosition(float height, float clearance, out Vector3 position)
	{
		position = Vector3.zero;
		if (availableSpawnCells == null || availableSpawnCells.Count == 0) return false;

		int idx = Random.Range(0, availableSpawnCells.Count);
		Vector2Int cell = availableSpawnCells[idx];
		availableSpawnCells.RemoveAt(idx);
		ReserveCell(cell.x, cell.y);

		Vector3 center = GetCellCenter(cell.x, cell.y);
		center.y = height;

		float maxOffset = Mathf.Max(cellHalf - clearance, 0f);
		if (maxOffset > 0f)
			center += new Vector3(Random.Range(-maxOffset, maxOffset), 0f, Random.Range(-maxOffset, maxOffset));

		position = center;
		return true;
	}
}
