using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float sprintMultiplier = 2f;
    public float jumpForce = 1.8f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform cameraPivot;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private CharacterController controller;
    private float xRotation;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        sprintAction.action.Enable();
        jumpAction.action.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        sprintAction.action.Disable();
        jumpAction.action.Disable();
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
    }

    void HandleMovement()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        bool isSprinting = sprintAction.action.IsPressed();
        bool isGrounded = controller.isGrounded;

        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // force le contact au sol
        }

        if (jumpAction.action.WasPressedThisFrame() && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move =
            transform.forward * input.y +
            transform.right * input.x;

        Vector3 velocity = move * speed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        Vector2 look = lookAction.action.ReadValue<Vector2>();

        float mouseX = look.x * mouseSensitivity * Time.deltaTime;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
