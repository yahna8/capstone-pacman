using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class GhostMovement : MonoBehaviour
{
    public enum GhostState { Scatter, Chase, Frightened }

    [Header("References")]
    [Tooltip("Player transform (object the ghost will chase/avoid).")]
    public Transform player;

    [Tooltip("Waypoints used in Scatter state (e.g., corners).")]
    public Transform[] scatterPoints;

    [Header("Speeds (m/s)")]
    public float chaseSpeed = 3.5f;
    public float scatterSpeed = 3.0f;
    public float frightenedSpeed = 2.0f;

    [Header("Behaviour")]
    [Tooltip("How quickly the ghost turns toward its move direction.")]
    public float turnSmooth = 12f;

    [Tooltip("Seconds the ghost stays frightened after EnterFrightened().")]
    public float frightenedDuration = 6f;

    [Header("Gravity")]
    public float gravity = -9.81f; // keep grounded on sloped planes

    [Header("State & Visuals")]
    public GhostState state = GhostState.Scatter;

    [Tooltip("Optional: set colors for quick visual debugging per state.")]
    public bool applyStateColor = true;
    public Color scatterColor = new Color(1f, 0.5f, 1f); // pink
    public Color chaseColor   = new Color(1f, 0.2f, 0.2f); // red
    public Color frightenedColor = new Color(0.2f, 0.6f, 1f); // blue

    private CharacterController controller;
    private Renderer rend; // any Renderer on this object (or child)
    private Vector3 verticalVelocity;
    private int scatterIndex = 0;
    private float frightenedTimer = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        rend = GetComponentInChildren<Renderer>();
        ApplyColorForState();
    }

    void Update()
    {
        // --- DEBUG INPUT: change states with 1/2/3 keys ---
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) EnterScatter();
            if (Keyboard.current.digit2Key.wasPressedThisFrame) EnterChase();
            if (Keyboard.current.digit3Key.wasPressedThisFrame) EnterFrightened();
        }

        // Tick frightened timer
        if (state == GhostState.Frightened)
        {
            frightenedTimer -= Time.deltaTime;
            if (frightenedTimer <= 0f) EnterScatter();
        }

        // Decide direction & speed
        Vector3 dir = Vector3.zero;
        float speed = scatterSpeed;

        switch (state)
        {
            case GhostState.Chase:
                if (player != null)
                {
                    dir = player.position - transform.position;
                    dir.y = 0f;
                    dir.Normalize();
                }
                speed = chaseSpeed;
                break;

            case GhostState.Scatter:
                if (scatterPoints != null && scatterPoints.Length > 0)
                {
                    Transform target = scatterPoints[scatterIndex % scatterPoints.Length];
                    Vector3 to = target.position - transform.position;
                    to.y = 0f;

                    // advance waypoint if close
                    if (to.sqrMagnitude < 0.5f * 0.5f)
                        scatterIndex = (scatterIndex + 1) % scatterPoints.Length;

                    dir = to.normalized;
                }
                else
                {
                    // fallback: keep going forward
                    dir = transform.forward;
                }
                speed = scatterSpeed;
                break;

            case GhostState.Frightened:
                if (player != null)
                {
                    // run away from player with a small wobble
                    dir = (transform.position - player.position);
                    dir.y = 0f;
                    if (dir.sqrMagnitude < 0.01f) dir = Random.insideUnitSphere;
                    dir.Normalize();

                    Vector3 wobble = new Vector3(
                        Mathf.PerlinNoise(Time.time * 1.2f, 0f) - 0.5f,
                        0f,
                        Mathf.PerlinNoise(0f, Time.time * 1.2f) - 0.5f
                    ) * 0.6f;

                    dir = (dir + wobble).normalized;
                }
                speed = frightenedSpeed;
                break;
        }

        // Face movement direction smoothly
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSmooth * Time.deltaTime);
        }

        // Horizontal move
        Vector3 horizontal = dir * speed * Time.deltaTime;
        controller.Move(horizontal);

        // Simple gravity
        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    // --- Public API ---
    public void EnterChase()
    {
        state = GhostState.Chase;
        ApplyColorForState();
    }

    public void EnterScatter()
    {
        state = GhostState.Scatter;
        ApplyColorForState();
    }

    public void EnterFrightened()
    {
        state = GhostState.Frightened;
        frightenedTimer = frightenedDuration;
        ApplyColorForState();
    }

    // --- Helpers ---
    private void ApplyColorForState()
    {
        if (!applyStateColor || rend == null) return;

        Color c = scatterColor;
        switch (state)
        {
            case GhostState.Chase: c = chaseColor; break;
            case GhostState.Frightened: c = frightenedColor; break;
        }
        // Use sharedMaterial if you want all ghosts using same material to change together.
        if (rend.material != null) rend.material.color = c;
    }
}
