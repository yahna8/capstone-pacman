using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
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
    int[,] maze = new int[,]
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
                            pellet.transform.localPosition = localPos + mazeOrigin + Vector3.up * 0.1f;
                            ConfigurePelletInstance(pellet, "Pellet");
                        }
                        break;
                    case 2: // Power pellet
                        if (powerPelletPrefab != null)
                        {
                            GameObject powerPellet = Instantiate(powerPelletPrefab, transform);
                            powerPellet.transform.localPosition = localPos + mazeOrigin + Vector3.up * 0.1f;
                            ConfigurePelletInstance(powerPellet, "PowerPellet");
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

    private void ConfigurePelletInstance(GameObject pelletObject, string tagName)
    {
        if (pelletObject == null)
            return;

        // Runtime guard: keep generated pellets interactible even if prefab settings drift.
        pelletObject.tag = tagName;

        Collider col = pelletObject.GetComponent<Collider>();
        if (col == null)
            col = pelletObject.AddComponent<SphereCollider>();

        col.isTrigger = true;
    }
}
