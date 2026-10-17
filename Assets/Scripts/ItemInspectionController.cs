using UnityEngine;
using UnityEngine.InputSystem;

public class ItemInspectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private PlayerController playerController;

    [Header("Inspection Settings")]
    [SerializeField] private float inspectDistance = 1.2f;
    [SerializeField] private float rotationSpeed = 0.2f;

    [SerializeField] private InteractionUI interactionUI;

    private Transform inspectedItem;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody itemRigidbody;
    private bool wasKinematic;
    private bool isInspecting;

    private CursorLockMode previousCursorLock;
    private bool previousCursorVisibility;

    public bool IsInspecting => isInspecting;

    public void BeginInspection(Transform item)
    {
        if (isInspecting || item == null)
            return;

        inspectedItem = item;

        originalParent = item.parent;
        originalPosition = item.position;
        originalRotation = item.rotation;

        itemRigidbody = item.GetComponent<Rigidbody>();

        if (itemRigidbody != null)
        {
            wasKinematic = itemRigidbody.isKinematic;
            itemRigidbody.isKinematic = true;
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
        }

        previousCursorLock = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        if (interactionUI != null)
            interactionUI.Hide();

        isInspecting = true;

        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        item.SetParent(playerCamera, true);
        item.localPosition = new Vector3(0f, 0f, inspectDistance);
        item.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (!isInspecting || inspectedItem == null)
            return;

        RotateItem();

        if (Keyboard.current != null &&
            (Keyboard.current.eKey.wasPressedThisFrame ||
             Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            EndInspection();
        }
    }

    private void RotateItem()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        inspectedItem.Rotate(
            playerCamera.up,
            -mouseDelta.x * rotationSpeed,
            Space.World
        );

        inspectedItem.Rotate(
            playerCamera.right,
            mouseDelta.y * rotationSpeed,
            Space.World
        );
    }

    public void EndInspection()
    {
        if (!isInspecting)
            return;

        if (inspectedItem != null)
        {
            inspectedItem.SetParent(originalParent, true);
            inspectedItem.SetPositionAndRotation(
                originalPosition,
                originalRotation
            );
        }

        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = wasKinematic;
        }

        inspectedItem = null;
        itemRigidbody = null;
        isInspecting = false;

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisibility;

        if (playerController != null)
            playerController.enabled = true;
    }
}