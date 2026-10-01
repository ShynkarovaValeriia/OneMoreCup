using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform playerCamera;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction lookAction;

    private float cameraRotationX = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        lookAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
    }

    private void LateUpdate()
    {
        Look();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 movement = transform.right * input.x + transform.forward * input.y;
        Vector3 velocity = movement * moveSpeed;

        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    private void Look()
    {
        Vector2 input = lookAction.ReadValue<Vector2>() * mouseSensitivity;

        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, input.x, 0f));

        cameraRotationX = Mathf.Clamp(cameraRotationX - input.y, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
    }
}