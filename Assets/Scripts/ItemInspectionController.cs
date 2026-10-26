using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class ItemInspectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InteractionUI interactionUI;
    [SerializeField] private InspectableItemHighlighter itemHighlighter;
    [SerializeField] private TMP_Text actionPrompt;

    [Header("Story")]
    [SerializeField] private WakeUpIntro wakeUpIntro;
    [SerializeField] private PowerOutage powerOutage;

    [Header("Inspection Settings")]
    [SerializeField] private float inspectDistance = 1.2f;
    [SerializeField] private float rotationSpeed = 0.2f;

    [Header("Carrying Settings")]
    [SerializeField]
    private Vector3 carryPosition =
        new Vector3(0.35f, -0.25f, 0.8f);

    private Transform inspectedItem;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private InspectableItem inspectedItemData;

    private GameObject activeCarriedVisual;
    private bool usingSeparateCarryVisual;

    private Rigidbody itemRigidbody;
    private bool wasKinematic;
    private bool wasUsingGravity;

    private Collider[] itemColliders;
    private bool[] colliderStates;

    private bool isInspecting;
    private bool isCarrying;
    private bool canCarryItem;

    private CursorLockMode previousCursorLock;
    private bool previousCursorVisibility;

    public bool IsInspecting => isInspecting;
    public bool IsCarrying => isCarrying;

    public bool IsHoldingCup =>
        isCarrying &&
        inspectedItemData != null &&
        inspectedItemData.ItemName == "Чашка";

    public Transform HeldCupTransform =>
        IsHoldingCup ? inspectedItem : null;

    [Header("Match Burnout")]
    [SerializeField] private float matchBurnOutDuration = 0.2f;

    private Coroutine extinguishCoroutine;

    [SerializeField] private Transform matchBurningVisual;

    private bool isCutscenePlaying = false;

    private void Start()
    {
        HideActionPrompt();
    }

    public void BeginInspection(Transform item)
    {
        if (isInspecting || isCarrying || item == null)
            return;

        inspectedItem = item;

        originalParent = item.parent;
        originalPosition = item.position;
        originalRotation = item.rotation;

        inspectedItemData = item.GetComponent<InspectableItem>();

        canCarryItem =
            inspectedItemData != null &&
            inspectedItemData.CanCarry;

        itemRigidbody = item.GetComponent<Rigidbody>();

        if (itemRigidbody != null)
        {
            wasKinematic = itemRigidbody.isKinematic;
            wasUsingGravity = itemRigidbody.useGravity;

            itemRigidbody.isKinematic = true;
            itemRigidbody.useGravity = false;
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
        }

        itemColliders = item.GetComponentsInChildren<Collider>(true);
        colliderStates = new bool[itemColliders.Length];

        for (int i = 0; i < itemColliders.Length; i++)
        {
            colliderStates[i] = itemColliders[i].enabled;
            itemColliders[i].enabled = false;
        }

        previousCursorLock = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        if (interactionUI != null)
            interactionUI.Hide();

        if (itemHighlighter != null)
            itemHighlighter.enabled = false;

        isInspecting = true;

        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        item.SetParent(playerCamera, false);
        item.localPosition = new Vector3(0f, 0f, inspectDistance);
        item.localRotation = Quaternion.identity;

        if (canCarryItem)
        {
            if (inspectedItemData.CanDrop)
                ShowActionPrompt("ЛКМ Взяти  ПКМ Покласти");
            else
                ShowActionPrompt("ЛКМ Взяти");
        }
        else
        {
            HideActionPrompt();
        }
    }

    private void Update()
    {
        if (isCutscenePlaying)
            return;

        if (isInspecting)
        {
            if (inspectedItem == null)
            {
                EndInspection();
                return;
            }

            RotateItem();

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    TakeItem();
                    return;
                }

                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    ReturnItemToOriginalPlace();
                    return;
                }
            }

            if (Keyboard.current != null &&
                (Keyboard.current.eKey.wasPressedThisFrame ||
                 Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                EndInspection();
            }
        }
        else if (isCarrying)
        {
            if (inspectedItem == null)
            {
                FinishInteraction();
                return;
            }

            if (inspectedItemData != null &&
                inspectedItemData.CanDrop &&
                Mouse.current != null &&
                Mouse.current.rightButton.wasPressedThisFrame)
            {
                DropItem();
            }
        }
    }

    private void RotateItem()
    {
        if (Mouse.current == null || inspectedItem == null)
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

    private void TakeItem()
    {
        if (!canCarryItem || inspectedItem == null)
            return;

        GameObject separateVisual =
            inspectedItemData != null
                ? inspectedItemData.CarriedVisual
                : null;

        isInspecting = false;
        isCarrying = true;

        if (separateVisual != null)
        {
            // Возвращаем оригинальный предмет на место.
            inspectedItem.SetParent(originalParent, true);
            inspectedItem.SetPositionAndRotation(
                originalPosition,
                originalRotation
            );

            RestorePhysicsAndColliders();

            // Теперь переносим отдельную модель в руке.
            usingSeparateCarryVisual = true;
            activeCarriedVisual = separateVisual;
            inspectedItem = separateVisual.transform;

            itemRigidbody = null;
            itemColliders = null;
            colliderStates = null;

            activeCarriedVisual.SetActive(true);
            inspectedItem.SetParent(playerCamera, false);

            inspectedItem.localPosition =
                inspectedItemData.CarriedPosition;

            inspectedItem.localRotation =
                Quaternion.Euler(inspectedItemData.CarriedRotation);

            inspectedItem.localScale =
                inspectedItemData.CarriedScale;

            if (inspectedItemData != null && inspectedItemData.ItemName == "Сірники" && powerOutage != null && powerOutage.IsPowerOff && wakeUpIntro != null)
            {
                wakeUpIntro.ShowFuseBoxReminder();
            }
        }
        else
        {
            // Обычный предмет, например чашка.
            usingSeparateCarryVisual = false;
            activeCarriedVisual = null;

            inspectedItem.SetParent(playerCamera, false);
            inspectedItem.localPosition = carryPosition;
            inspectedItem.localRotation = Quaternion.identity;
        }

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (inspectedItemData != null &&
            inspectedItemData.CanDrop)
        {
            ShowActionPrompt("ПКМ Покласти");
        }
        else
        {
            HideActionPrompt();
        }
    }

    private void ReturnItemToOriginalPlace()
    {
        if (inspectedItem == null)
        {
            EndInspection();
            return;
        }

        inspectedItem.SetParent(originalParent, true);
        inspectedItem.SetPositionAndRotation(
            originalPosition,
            originalRotation
        );

        RestorePhysicsAndColliders();
        FinishInteraction();
    }

    private void DropItem()
    {
        if (inspectedItem == null)
        {
            FinishInteraction();
            return;
        }

        // Предмет с отдельной моделью не выбрасываем.
        if (usingSeparateCarryVisual)
            return;

        Transform item = inspectedItem;

        item.SetParent(null, true);

        item.position =
            playerCamera.position +
            playerCamera.forward * 0.8f -
            playerCamera.up * 0.2f;

        RestorePhysicsAndColliders();

        if (itemRigidbody != null)
        {
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
        }

        FinishInteraction();
    }

    public void EndInspection()
    {
        if (!isInspecting)
            return;

        ReturnItemToOriginalPlace();
    }

    private void RestorePhysicsAndColliders()
    {
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = wasKinematic;
            itemRigidbody.useGravity = wasUsingGravity;
        }

        if (itemColliders != null)
        {
            for (int i = 0; i < itemColliders.Length; i++)
            {
                if (itemColliders[i] != null)
                    itemColliders[i].enabled = colliderStates[i];
            }
        }
    }

    private void FinishInteraction()
    {
        if (usingSeparateCarryVisual &&
            activeCarriedVisual != null)
        {
            activeCarriedVisual.SetActive(false);
            activeCarriedVisual.transform.SetParent(null, true);
        }

        inspectedItem = null;
        inspectedItemData = null;
        itemRigidbody = null;
        itemColliders = null;
        colliderStates = null;

        activeCarriedVisual = null;
        usingSeparateCarryVisual = false;

        isInspecting = false;
        isCarrying = false;
        canCarryItem = false;

        if (playerController != null)
            playerController.enabled = true;

        if (itemHighlighter != null)
            itemHighlighter.enabled = true;

        HideActionPrompt();

        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisibility;
    }

    private void ShowActionPrompt(string message)
    {
        if (actionPrompt == null)
            return;

        actionPrompt.text = message;
        actionPrompt.gameObject.SetActive(true);
    }

    private void HideActionPrompt()
    {
        if (actionPrompt != null)
            actionPrompt.gameObject.SetActive(false);
    }

    public void ExtinguishCarriedMatch()
    {
        // Проверяем, что игрок держит именно спичку.
        if (inspectedItemData == null ||
            inspectedItemData.ItemName != "Сірники" ||
            !isCarrying ||
            !usingSeparateCarryVisual ||
            activeCarriedVisual == null)
        {
            return;
        }

        // Не запускаем сгорание повторно.
        if (extinguishCoroutine != null)
            return;

        extinguishCoroutine =
            StartCoroutine(BurnOutMatchRoutine());
    }

    private IEnumerator BurnOutMatchRoutine()
    {
        GameObject match = activeCarriedVisual;
        Transform matchTransform = match.transform;

        // Запоминаем исходный размер спички.
        Vector3 initialScale = matchBurningVisual != null
            ? matchBurningVisual.localScale
            : matchTransform.localScale;

        // Находим источник света внутри модели спички.
        Light matchLight = match.GetComponentInChildren<Light>(true);

        float initialIntensity = 0f;
        float initialRange = 0f;

        if (matchLight != null)
        {
            initialIntensity = matchLight.intensity;
            initialRange = matchLight.range;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, matchBurnOutDuration);

        // Постепенно уменьшаем спичку и её свечение.
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            if (matchBurningVisual != null)
            {
                Vector3 scale = matchBurningVisual.localScale;

                // Спичка укорачивается сверху вниз,
                // а основание остаётся на месте.
                scale.y = Mathf.Lerp(
                    initialScale.y,
                    0f,
                    progress
                );

                matchBurningVisual.localScale = scale;
            }

            // Постепенно гасим свет.
            if (matchLight != null)
            {
                matchLight.intensity =
                    Mathf.Lerp(initialIntensity, 0f, progress);

                matchLight.range =
                    Mathf.Lerp(initialRange, 0f, progress);
            }

            yield return null;
        }

        // Убираем спичку после завершения анимации.
        if (match != null)
        {
            match.SetActive(false);
            match.transform.SetParent(null, true);

            // Возвращаем исходный размер для возможного повторного использования.
            match.transform.localScale = initialScale;
        }

        // Полностью освобождаем состояние удержания предмета.
        activeCarriedVisual = null;
        inspectedItem = null;
        inspectedItemData = null;

        itemRigidbody = null;
        itemColliders = null;
        colliderStates = null;

        usingSeparateCarryVisual = false;
        isInspecting = false;
        isCarrying = false;
        canCarryItem = false;

        extinguishCoroutine = null;

        if (playerController != null)
            playerController.enabled = true;

        if (itemHighlighter != null)
            itemHighlighter.enabled = true;

        if (matchBurningVisual != null)
        {
            matchBurningVisual.localScale = initialScale;
        }

        HideActionPrompt();

        Debug.Log("Спичка догорела и исчезла.");
    }

    public void SetCutscenePlaying(bool playing)
    {
        isCutscenePlaying = playing;

        if (playerController != null)
            playerController.enabled = !playing;

        if (playing)
        {
            if (interactionUI != null)
                interactionUI.Hide();

            HideActionPrompt();
        }
        else
        {
            if (itemHighlighter != null)
                itemHighlighter.enabled = true;
        }
    }
}