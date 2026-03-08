using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Meters per second for WASD movement.")]
    public float moveSpeed = 4f;

    [Header("Gravity")]
    [Tooltip("Gravity applied downward.")]
    public float gravity = -9.81f;

    [Header("Optional camera for forward direction")]
    [Tooltip("Drag your Main Camera here so WASD moves relative to camera yaw.")]
    public Transform forwardReference;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1) Read WASD
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        // Normalize so diagonal isn't faster
        Vector3 input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // Determine flat (yaw-only) forward to move relative to 
        float yaw;
        if (forwardReference != null)
            yaw = forwardReference.eulerAngles.y;   // follow head/camera yaw 
        else
            yaw = transform.eulerAngles.y;          // follow player body's yaw

        Quaternion flatYaw = Quaternion.Euler(0f, yaw, 0f);
        Vector3 moveWorld = flatYaw * input * moveSpeed;

        // Apply horizontal movement 
        controller.Move(moveWorld * Time.deltaTime);

        // 4) Simple gravity so you stay grounded on slopes
        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f; // keep snapped to ground

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}
