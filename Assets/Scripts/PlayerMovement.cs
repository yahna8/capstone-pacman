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

    [Header("Camera Controls")]
    [Tooltip("Drag your Main Camera here.")]
    public Transform forwardReference;
    [Tooltip("Adjust mouse look speed.")]
    public float mouseSensitivity = 2f;

    [Header("VR Testing Override")]
    [Tooltip("Check this to use WASD and Mouse. Uncheck before building to Quest.")]
    public bool useDesktopTesting = true;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float cameraPitch = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (useDesktopTesting)
        {
            // 1. Mouse Look
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate the player body left/right
            transform.Rotate(0f, mouseX, 0f);

            // Rotate the camera up/down
            if (forwardReference != null)
            {
                cameraPitch -= mouseY;
                cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
                forwardReference.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
            }

            // 2. Read WASD
            float h = Input.GetAxisRaw("Horizontal"); 
            float v = Input.GetAxisRaw("Vertical");   

            Vector3 input = new Vector3(h, 0f, v);
            if (input.sqrMagnitude > 1f) input.Normalize();

            float yaw;
            if (forwardReference != null)
                yaw = forwardReference.eulerAngles.y;  
            else
                yaw = transform.eulerAngles.y;         

            Quaternion flatYaw = Quaternion.Euler(0f, yaw, 0f);
            Vector3 moveWorld = flatYaw * input * moveSpeed;

            controller.Move(moveWorld * Time.deltaTime);

            // 3. Gravity
            if (controller.isGrounded && verticalVelocity.y < 0f)
                verticalVelocity.y = -2f; 

            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move(verticalVelocity * Time.deltaTime);
        }
    }
}