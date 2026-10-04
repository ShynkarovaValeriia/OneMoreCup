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

    [Header("Interaction")]
    [SerializeField] private InteractionUI interactionUI;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction interactAction;

    private float cameraRotationX = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        lookAction?.Enable();
        interactAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        interactAction?.Disable();
    }

    private void LateUpdate()
    {
        Look();
        Interact();
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

    private void Interact()
    {
        // расстояние на котором игрок может доставать до предемета чтобы взаимодействовать
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            Interaction interactable = hit.collider.GetComponent<Interaction>();

            if (interactable != null)
            {
                interactionUI.Show();

                if (interactAction.WasPressedThisFrame())
                {
                    interactable.Interact();
                }

                return;
            }
        }

        interactionUI.Hide();
    }
}