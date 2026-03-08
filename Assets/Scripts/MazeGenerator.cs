using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public int width = 28;
    public int height = 36;

    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pelletPrefab;
    public GameObject powerPelletPrefab;
    public GameObject ghostSpawnFloorPrefab; // Red floor prefab

    public float tileSize = 2f;

    // Configurable ghost spawn size
    public int ghostSpawnWidth = 2;
    public int ghostSpawnHeight = 2;

    private enum TileType { Wall, Path, GhostSpawn, Pellet, PowerPellet }
    private TileType[,] grid;

    public List<Vector3> ghostSpawnPositions = new List<Vector3>();

    void Start()
    {
        GenerateMaze();
        BuildMaze();
    }

    void GenerateMaze()
    {
        grid = new TileType[width, height];

        // Fill grid with walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = TileType.Wall;

        GenerateLeftHalfWithDFS();
        MirrorRightSide();
        PlaceGhostSpawnArea();
        PlacePelletsWithPowerCorners();
    }

    void GenerateLeftHalfWithDFS()
    {
        bool[,] visited = new bool[width, height];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        Vector2Int start = new Vector2Int(1, 1);
        stack.Push(start);
        visited[1, 1] = true;
        grid[1, 1] = TileType.Path;

        Vector2Int[] directions =
        {
            new Vector2Int(2, 0),
            new Vector2Int(-2, 0),
            new Vector2Int(0, 2),
            new Vector2Int(0, -2)
        };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> neighbors = new List<Vector2Int>();

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;

                if (next.x > 0 && next.x < width / 2 &&
                    next.y > 0 && next.y < height - 1 &&
                    !visited[next.x, next.y])
                {
                    neighbors.Add(next);
                }
            }

            if (neighbors.Count > 0)
            {
                Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
                Vector2Int wallBetween = current + (chosen - current) / 2;

                grid[wallBetween.x, wallBetween.y] = TileType.Path;
                grid[chosen.x, chosen.y] = TileType.Path;

                visited[chosen.x, chosen.y] = true;
                stack.Push(chosen);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    void MirrorRightSide()
    {
        for (int x = 0; x < width / 2; x++)
            for (int y = 0; y < height; y++)
                grid[width - 1 - x, y] = grid[x, y];
    }

    void PlaceGhostSpawnArea()
    {
        int spawnX = width / 2 - ghostSpawnWidth / 2;
        int spawnY = height / 2 - ghostSpawnHeight / 2;

        for (int x = spawnX; x < spawnX + ghostSpawnWidth; x++)
        {
            for (int y = spawnY; y < spawnY + ghostSpawnHeight; y++)
            {
                grid[x, y] = TileType.GhostSpawn;
            }
        }

        // Ensure entrance below spawn area
        grid[width / 2, spawnY - 1] = TileType.Path;
    }

    void PlacePelletsWithPowerCorners()
    {
        // Place pellets on all paths
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == TileType.Path)
                {
                    grid[x, y] = TileType.Pellet;
                }
            }
        }

        // Power pellet corner positions
        int[,] corners = new int[,] { { 1, 1 }, { 1, height - 2 }, { width - 2, 1 }, { width - 2, height - 2 } };

        for (int i = 0; i < 4; i++)
        {
            int cx = corners[i, 0];
            int cy = corners[i, 1];

            if (grid[cx, cy] == TileType.Wall)
            {
                grid[cx, cy] = TileType.PowerPellet;

                // Make adjacent tile a path so pellet is reachable
                if (cx > 0) grid[cx - 1, cy] = TileType.Path;
            }
            else
            {
                grid[cx, cy] = TileType.PowerPellet;
            }
        }
    }



    void BuildMaze()
    {
        ghostSpawnPositions.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 localPos = new Vector3(x * tileSize, 0, y * tileSize);

                switch (grid[x, y])
                {
                    case TileType.Wall:
                        {
                            GameObject wall = Instantiate(wallPrefab, transform);
                            wall.transform.localPosition = localPos;
                            break;
                        }

                    case TileType.Path:
                        {
                            GameObject floor = Instantiate(floorPrefab, transform);
                            floor.transform.localPosition = localPos;
                            break;
                        }

                    case TileType.Pellet:
                        {
                            GameObject floor = Instantiate(floorPrefab, transform);
                            floor.transform.localPosition = localPos;

                            GameObject pellet = Instantiate(pelletPrefab, transform);
                            pellet.transform.localPosition = localPos + Vector3.up * 0.25f;
                            break;
                        }

                    case TileType.PowerPellet:
                        {
                            GameObject floor = Instantiate(floorPrefab, transform);
                            floor.transform.localPosition = localPos;

                            GameObject power = Instantiate(powerPelletPrefab, transform);
                            power.transform.localPosition = localPos + Vector3.up * 0.25f;
                            break;
                        }

                    case TileType.GhostSpawn:
                        {
                            GameObject spawn = Instantiate(ghostSpawnFloorPrefab, transform);
                            spawn.transform.localPosition = localPos + Vector3.up * 0.01f;

                            ghostSpawnPositions.Add(spawn.transform.position);
                            break;
                        }
                }
            }
        }
    }

}