using System.Collections.Generic;
using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
    public static MazeBuilder Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject pelletPrefab;
    public GameObject powerPelletPrefab;
    public GameObject floorPrefab;

    [Header("Settings")]
    public float cellSize = 1f;

    [Header("Maze Origin Offset")]
    public Vector3 mazeOrigin = Vector3.zero; // World-space offset for the maze

    // 0 = normal pellet
    // 1 = wall
    // 2 = power pellet
    // 3 = ghost spawn (empty floor)
    private int[,] maze = new int[,]
    {
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,2,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,2,1},
        {1,0,1,1,1,0,1,1,1,1,0,1,0,1,1,1,1,0,1,1,1,0,1},
        {1,0,1,1,1,0,1,1,1,1,0,1,0,1,1,1,1,0,1,1,1,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,1,1,0,1,0,1,1,1,1,1,1,1,0,1,0,1,1,1,0,1},
        {1,0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,0,1},
        {1,1,1,1,1,0,1,1,1,1,0,1,0,1,1,1,1,0,1,1,1,1,1},
        {0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,1,0,1,0,0,0,0},
        {0,0,0,0,1,0,1,0,1,1,1,0,1,1,1,0,1,0,1,0,0,0,0},
        {1,1,1,1,1,0,0,0,1,0,0,0,0,0,1,0,0,0,1,1,1,1,1},
        {0,0,0,0,0,0,0,0,1,0,0,0,0,0,1,0,0,0,0,0,0,0,0},
        {1,1,1,1,1,0,1,0,1,0,0,0,0,0,1,0,1,0,1,1,1,1,1},
        {0,0,0,0,1,0,1,0,1,1,1,1,1,1,1,0,1,0,1,0,0,0,0},
        {0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,1,0,1,0,0,0,0},
        {1,1,1,1,1,0,1,0,1,1,1,1,1,1,1,0,1,0,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,1,1,1,0,1,1,1,1,0,1,0,1,1,1,1,0,1,1,1,0,1},
        {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,1},
        {1,1,0,0,1,0,1,0,1,1,1,1,1,1,1,0,1,0,1,0,0,1,1},
        {1,0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,0,1},
        {1,0,1,1,1,1,1,1,1,1,0,1,0,1,1,1,1,1,1,1,1,0,1},
        {1,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
    };

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildMaze();
    }

    void BuildMaze()
    {
        for (int y = 0; y < maze.GetLength(0); y++)
        {
            for (int x = 0; x < maze.GetLength(1); x++)
            {
                Vector3 localPos = new Vector3(x * cellSize, 0, -y * cellSize);

                // Parent under Maze GameObject
                switch (maze[y, x])
                {
                    case 1: // Wall
                        if (wallPrefab != null)
                        {
                            GameObject wall = Instantiate(wallPrefab, transform);
                            wall.transform.localPosition = localPos + mazeOrigin;
                        }
                        break;
                    case 0: // Normal pellet
                        if (pelletPrefab != null)
                        {
                            GameObject pellet = Instantiate(pelletPrefab, transform);
                            ConfigurePellet(pellet, "Pellet");
                            pellet.transform.localPosition = localPos + mazeOrigin + Vector3.up * 0.1f;
                        }
                        break;
                    case 2: // Power pellet
                        if (powerPelletPrefab != null)
                        {
                            GameObject powerPellet = Instantiate(powerPelletPrefab, transform);
                            ConfigurePellet(powerPellet, "PowerPellet");
                            powerPellet.transform.localPosition = localPos + mazeOrigin + Vector3.up * 0.1f;
                        }
                        break;
                    case 3: // Ghost spawn
                        // handle ghost spawn logic
                        break;
                }

                // Floor
                if (floorPrefab != null)
                {
                    GameObject floor = Instantiate(floorPrefab, transform);
                    floor.transform.localPosition = localPos + mazeOrigin;
                }
            }
        }
    }

    private void ConfigurePellet(GameObject pellet, string pelletTag)
    {
        pellet.tag = pelletTag;

        Collider[] colliders = pellet.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].isTrigger = true;
    }

    public bool TryGetNextPathStep(Vector3 fromWorld, Vector3 targetWorld, out Vector3 nextWorld)
    {
        nextWorld = fromWorld;

        if (!TryWorldToNearestWalkableCell(fromWorld, out Vector2Int start) ||
            !TryWorldToNearestWalkableCell(targetWorld, out Vector2Int goal))
        {
            return false;
        }

        if (start == goal)
        {
            nextWorld = CellToWorld(goal);
            return true;
        }

        if (!TryFindNextCell(start, goal, out Vector2Int nextCell))
            return false;

        nextWorld = CellToWorld(nextCell);
        return true;
    }

    public bool TryGetNearestWalkablePosition(Vector3 worldPosition, out Vector3 walkableWorld)
    {
        walkableWorld = worldPosition;

        if (!TryWorldToNearestWalkableCell(worldPosition, out Vector2Int cell))
            return false;

        walkableWorld = CellToWorld(cell);
        return true;
    }

    public bool IsWalkableCell(Vector2Int cell)
    {
        if (cell.y < 0 || cell.y >= maze.GetLength(0) ||
            cell.x < 0 || cell.x >= maze.GetLength(1))
        {
            return false;
        }

        return maze[cell.y, cell.x] != 1;
    }

    private bool TryWorldToNearestWalkableCell(Vector3 worldPosition, out Vector2Int cell)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition) - mazeOrigin;
        Vector2Int rounded = new Vector2Int(
            Mathf.RoundToInt(local.x / cellSize),
            Mathf.RoundToInt(-local.z / cellSize));

        if (IsWalkableCell(rounded))
        {
            cell = rounded;
            return true;
        }

        const int maxSearchRadius = 8;
        for (int radius = 1; radius <= maxSearchRadius; radius++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                        continue;

                    Vector2Int candidate = rounded + new Vector2Int(x, y);
                    if (IsWalkableCell(candidate))
                    {
                        cell = candidate;
                        return true;
                    }
                }
            }
        }

        cell = rounded;
        return false;
    }

    private bool TryFindNextCell(Vector2Int start, Vector2Int goal, out Vector2Int nextCell)
    {
        nextCell = start;

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        frontier.Enqueue(start);
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            if (current == goal)
                break;

            foreach (Vector2Int neighbor in GetWalkableNeighbors(current))
            {
                if (cameFrom.ContainsKey(neighbor))
                    continue;

                frontier.Enqueue(neighbor);
                cameFrom[neighbor] = current;
            }
        }

        if (!cameFrom.ContainsKey(goal))
            return false;

        Vector2Int step = goal;
        while (cameFrom[step] != start)
            step = cameFrom[step];

        nextCell = step;
        return true;
    }

    private IEnumerable<Vector2Int> GetWalkableNeighbors(Vector2Int cell)
    {
        Vector2Int[] offsets =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (Vector2Int offset in offsets)
        {
            Vector2Int neighbor = cell + offset;
            if (IsWalkableCell(neighbor))
                yield return neighbor;
        }
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        Vector3 local = new Vector3(cell.x * cellSize, 0f, -cell.y * cellSize) + mazeOrigin;
        return transform.TransformPoint(local);
    }
}
