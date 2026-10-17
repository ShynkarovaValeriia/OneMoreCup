using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintSpeed = 4f;

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

        float currentSpeed = moveSpeed;
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed)
        {
            currentSpeed = sprintSpeed;
        }

        Vector3 velocity = movement * currentSpeed;

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
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            Interaction interactable = hit.collider.GetComponent<Interaction>();

            InspectableItem inspectableItem = hit.collider.GetComponentInParent<InspectableItem>();

            if (interactable == null && inspectableItem != null)
            {
                interactable = inspectableItem;
            }

            if (interactable != null)
            {
                if (inspectableItem != null)
                {
                    interactionUI.Show(inspectableItem.ItemName);
                }
                else
                {
                    interactionUI.Show("Натисніть E");
                }

                bool pressedE = interactAction != null &&
                interactAction.WasPressedThisFrame();

                bool pressedLeftMouse = Mouse.current != null &&
                                        Mouse.current.leftButton.wasPressedThisFrame;

                if (pressedE || pressedLeftMouse)
                {
                    interactable.Interact();
                }

                return;
            }
        }

        interactionUI.Hide();
    }
}