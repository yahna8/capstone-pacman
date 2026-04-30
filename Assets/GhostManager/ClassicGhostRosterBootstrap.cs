using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ClassicGhostRosterBootstrap
{
    private static readonly GhostStateManager.GhostType[] RosterTypes =
    {
        GhostStateManager.GhostType.Blinky,
        GhostStateManager.GhostType.Pinky,
        GhostStateManager.GhostType.Inky,
        GhostStateManager.GhostType.Clyde
    };

    private static readonly string[] RosterNames =
    {
        "Blinky",
        "Pinky",
        "Inky",
        "Clyde"
    };

    private static readonly Color[] RosterColors =
    {
        new Color(1f, 0.1f, 0.1f, 1f),   // Blinky (red)
        new Color(1f, 0.6f, 0.8f, 1f),   // Pinky
        new Color(0.2f, 0.8f, 1f, 1f),   // Inky (blue/cyan)
        new Color(1f, 0.6f, 0.2f, 1f)    // Clyde (orange)
    };

    private const float WallCheckRadius = 0.3f;
    private static readonly Collider[] OverlapBuffer = new Collider[64];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ScheduleClassicGhostRoster()
    {
        GameObject runnerObject = new GameObject("ClassicGhostRosterBootstrapRunner");
        runnerObject.AddComponent<ClassicGhostRosterBootstrapRunner>();
    }

    internal static bool IsMazeReady()
    {
        GameObject mazeRoot = GameObject.Find("MazeGeneration");
        return mazeRoot != null && mazeRoot.transform.childCount > 0;
    }

    internal static void ConfigureClassicGhostRoster()
    {
        if (!Application.isPlaying)
            return;

        GhostStateManager[] foundGhosts = Object.FindObjectsByType<GhostStateManager>(FindObjectsSortMode.InstanceID);
        if (foundGhosts == null || foundGhosts.Length == 0)
        {
            Debug.LogWarning("ClassicGhostRosterBootstrap: no GhostStateManager found in scene.");
            return;
        }

        List<GhostStateManager> roster = new List<GhostStateManager>(foundGhosts);
        GhostStateManager template = roster[0];

        while (roster.Count < RosterNames.Length)
        {
            GameObject cloneObject = Object.Instantiate(template.gameObject, template.transform.position, template.transform.rotation);
            GhostStateManager cloneManager = cloneObject.GetComponent<GhostStateManager>();
            if (cloneManager != null)
                roster.Add(cloneManager);
        }

        Transform player = FindPlayerTransform();
        ScoreManager scoreManager = ScoreManager.Instance != null ? ScoreManager.Instance : Object.FindAnyObjectByType<ScoreManager>();
        GameStateManager gameStateManager = GameStateManager.Instance != null
            ? GameStateManager.Instance
            : Object.FindAnyObjectByType<GameStateManager>();
        GhostModeController modeController = GhostModeController.Instance != null
            ? GhostModeController.Instance
            : GhostModeController.GetOrCreate();

        Transform[] scatterCorners = FindScatterCorners();

        Vector3 preferredCenter = template.SpawnTransform != null
            ? template.SpawnTransform.position
            : (template.HomeTransform != null ? template.HomeTransform.position : template.transform.position);

        Vector3 safeCenter = FindSafeGhostClusterCenter(preferredCenter);

        Transform home = EnsureAnchor("GhostHome", safeCenter);
        Transform spawn = EnsureAnchor("GhostSpawn", safeCenter);

        home.position = safeCenter;
        spawn.position = safeCenter;

        for (int i = RosterNames.Length; i < roster.Count; i++)
            roster[i].gameObject.SetActive(false);

        for (int i = 0; i < RosterNames.Length; i++)
        {
            GhostStateManager ghost = roster[i];
            ghost.gameObject.name = RosterNames[i];
            ghost.gameObject.tag = "Ghost";
            ghost.gameObject.SetActive(true);
            ghost.transform.position = spawn.position + GhostStateManager.GetSpawnOffsetForType(RosterTypes[i]);
            ghost.transform.rotation = Quaternion.identity;

            DisableLegacyGhostMovement(ghost);

            ghost.SetGhostType(RosterTypes[i]);
            ghost.ConfigureRuntimeReferences(
                player,
                GetClassicScatterCorner(i, scatterCorners),
                home,
                spawn,
                scoreManager,
                modeController,
                gameStateManager);

            ApplyGhostColor(ghost, RosterColors[i]);
            ghost.ResetToHomeAndRespawn();
        }
    }

    private static Transform FindPlayerTransform()
    {
        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
            return taggedPlayer.transform;

        CollisionTrigger collisionTrigger = Object.FindAnyObjectByType<CollisionTrigger>();
        if (collisionTrigger != null)
            return collisionTrigger.transform;

        GameObject fallbackPlayer = GameObject.Find("Sphere");
        return fallbackPlayer != null ? fallbackPlayer.transform : null;
    }

    private static void DisableLegacyGhostMovement(GhostStateManager ghost)
    {
        GhostMovement[] legacyMovementScripts = ghost.GetComponentsInChildren<GhostMovement>(true);
        for (int i = 0; i < legacyMovementScripts.Length; i++)
            legacyMovementScripts[i].enabled = false;
    }

    private static Transform[] FindScatterCorners()
    {
        GameObject cornersRoot = GameObject.Find("GhostWaypoints");
        if (cornersRoot == null)
            return System.Array.Empty<Transform>();

        Transform root = cornersRoot.transform;
        List<Transform> corners = new List<Transform>(root.childCount);
        for (int i = 0; i < root.childCount; i++)
            corners.Add(root.GetChild(i));

        corners.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return corners.ToArray();
    }

    private static Transform GetClassicScatterCorner(int rosterIndex, Transform[] corners)
    {
        if (corners == null || corners.Length == 0)
            return null;

        if (corners.Length < 4)
            return corners[Mathf.Clamp(rosterIndex, 0, corners.Length - 1)];

        // Sorted by name: Waypoint1, Waypoint2, Waypoint3, Waypoint4.
        // Blinky->top-right(2), Pinky->top-left(3), Inky->bottom-right(1), Clyde->bottom-left(4).
        switch (rosterIndex)
        {
            case 0: return corners[1];
            case 1: return corners[2];
            case 2: return corners[0];
            case 3: return corners[3];
            default: return corners[0];
        }
    }

    private static Transform EnsureAnchor(string objectName, Vector3 worldPosition)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
            return existing.transform;

        GameObject created = new GameObject(objectName);
        created.transform.position = worldPosition;
        return created.transform;
    }

    private static Vector3 FindSafeGhostClusterCenter(Vector3 preferredCenter)
    {
        if (IsClusterClear(preferredCenter))
            return preferredCenter;

        const int maxRing = 24;
        const float step = 1f;

        for (int ring = 1; ring <= maxRing; ring++)
        {
            int min = -ring;
            int max = ring;

            for (int x = min; x <= max; x++)
            {
                for (int z = min; z <= max; z++)
                {
                    bool isRingEdge = x == min || x == max || z == min || z == max;
                    if (!isRingEdge)
                        continue;

                    Vector3 candidate = preferredCenter + new Vector3(x * step, 0f, z * step);
                    if (IsClusterClear(candidate))
                        return candidate;
                }
            }
        }

        Debug.LogWarning("ClassicGhostRosterBootstrap: no clear spawn cluster found; using preferred center.");
        return preferredCenter;
    }

    private static bool IsClusterClear(Vector3 center)
    {
        for (int i = 0; i < RosterTypes.Length; i++)
        {
            Vector3 position = center + GhostStateManager.GetSpawnOffsetForType(RosterTypes[i]);
            if (IsInsideWall(position))
                return false;
        }

        return true;
    }

    private static bool IsInsideWall(Vector3 worldPosition)
    {
        Vector3 probe = worldPosition + Vector3.up * 0.5f;
        int count = Physics.OverlapSphereNonAlloc(
            probe,
            WallCheckRadius,
            OverlapBuffer,
            ~0,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider col = OverlapBuffer[i];
            if (col == null || !col.enabled || col.isTrigger)
                continue;

            if (col.gameObject != null && col.gameObject.name.StartsWith("Wall"))
                return true;
        }

        return false;
    }

    private static void ApplyGhostColor(GhostStateManager ghost, Color color)
    {
        Renderer renderer = ghost.GetComponentInChildren<Renderer>();
        if (renderer == null)
            return;

        Material runtimeMaterial = renderer.material;
        if (runtimeMaterial == null)
            return;

        if (runtimeMaterial.HasProperty("_BaseColor"))
            runtimeMaterial.SetColor("_BaseColor", color);

        if (runtimeMaterial.HasProperty("_Color"))
            runtimeMaterial.SetColor("_Color", color);

        ghost.SetNormalColor(color);
    }
}

[DefaultExecutionOrder(1000)]
public class ClassicGhostRosterBootstrapRunner : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Maze generation happens in Start(); wait at least one frame.
        yield return null;

        // Then give the maze a short window to populate colliders.
        int waitFrames = 0;
        while (!ClassicGhostRosterBootstrap.IsMazeReady() && waitFrames < 180)
        {
            waitFrames++;
            yield return null;
        }

        ClassicGhostRosterBootstrap.ConfigureClassicGhostRoster();
        Destroy(gameObject);
    }
}
