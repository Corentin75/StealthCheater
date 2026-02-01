using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform cameraPivot;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private CharacterController controller;
    private float xRotation;

    private Animator animator;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
        UpdateAnimation();
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 move = transform.forward * input.y + transform.right * input.x;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleLook()
    {
        Vector2 look = lookAction.action.ReadValue<Vector2>();

        float mouseX = look.x * mouseSensitivity * Time.deltaTime;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void UpdateAnimation()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        bool isWalking = input.sqrMagnitude > 0.01f;
        animator.SetBool("isWalking", isWalking);
    }
}
