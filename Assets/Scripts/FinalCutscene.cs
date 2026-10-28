
using System.Collections;
using UnityEngine;

public class FinalCutscene : MonoBehaviour, Interaction
{
    [Header("Player")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private ItemInspectionController itemInspectionController;
    [SerializeField] private Transform sitPoint;
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Story")]
    [SerializeField] private Kettle kettle;

    [Header("Screen")]
    [SerializeField] private CanvasGroup blackScreen;

    [Header("Camera")]
    [SerializeField] private float seatedCameraHeight = 0.35f;
    [SerializeField] private float cameraLookDownAngle = 30f;

    [Header("Timing")]
    [SerializeField] private float sitDuration = 1.5f;
    [SerializeField] private float lookAtCupDuration = 1.5f;
    [SerializeField] private float pauseBeforeClosingEyes = 1.5f;
    [SerializeField] private float closeEyesDuration = 4f;

    private bool finaleStarted = false;

    public void Interact()
    {
        if (finaleStarted)
            return;

        if (kettle == null || !kettle.TeaHasBeenPoured)
        {
            Debug.Log("Сначала нужно налить чай.");
            return;
        }

        if (itemInspectionController == null ||
            !itemInspectionController.IsHoldingCup)
        {
            Debug.Log("Для финала героиня должна держать чашку.");
            return;
        }

        if (playerCamera == null ||
            playerController == null ||
            sitPoint == null ||
            blackScreen == null)
        {
            Debug.LogWarning(
                "FinalCutscene: проверь ссылки в Inspector.",
                this
            );
            return;
        }

        StartCoroutine(PlayFinalCutscene());
    }

    private IEnumerator PlayFinalCutscene()
    {
        finaleStarted = true;

        // Запоминаем исходные положения.
        Vector3 startPlayerPosition = playerController.transform.position;
        Quaternion startPlayerRotation = playerController.transform.rotation;

        Vector3 startCameraPosition = playerCamera.localPosition;
        Quaternion startCameraRotation = playerCamera.localRotation;

        Vector3 targetPlayerPosition = sitPoint.position;
        Quaternion targetPlayerRotation = sitPoint.rotation;

        Vector3 targetCameraPosition = startCameraPosition;
        targetCameraPosition.y = seatedCameraHeight;

        // Блокируем управление и взаимодействия.
        itemInspectionController.SetCutscenePlaying(true);
        playerController.enabled = false;

        // Останавливаем физическое движение игрока,
        // чтобы Rigidbody не мешал анимации посадки.
        bool hadRigidbody = playerRigidbody != null;
        bool wasKinematic = false;

        if (hadRigidbody)
        {
            wasKinematic = playerRigidbody.isKinematic;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
        }

        // Экран должен быть видимым до начала финала.
        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 0f;
        blackScreen.blocksRaycasts = true;

        // 1. Героиня перемещается к точке посадки,
        // поворачивается и опускает камеру.
        float timer = 0f;

        while (timer < sitDuration)
        {
            timer += Time.deltaTime;
            float t = Smooth(timer / sitDuration);

            playerController.transform.position = Vector3.Lerp(
                startPlayerPosition,
                targetPlayerPosition,
                t
            );

            playerController.transform.rotation = Quaternion.Slerp(
                startPlayerRotation,
                targetPlayerRotation,
                t
            );

            playerCamera.localPosition = Vector3.Lerp(
                startCameraPosition,
                targetCameraPosition,
                t
            );

            yield return null;
        }

        playerController.transform.position = targetPlayerPosition;
        playerController.transform.rotation = targetPlayerRotation;
        playerCamera.localPosition = targetCameraPosition;

        // 2. Героиня смотрит вниз на чашку.
        Quaternion cupLookRotation =
            startCameraRotation *
            Quaternion.Euler(cameraLookDownAngle, 0f, 0f);

        yield return RotateCamera(
            playerCamera,
            playerCamera.localRotation,
            cupLookRotation,
            lookAtCupDuration
        );

        // Небольшая пауза — героиня смотрит на чай.
        yield return new WaitForSeconds(pauseBeforeClosingEyes);

        // 3. Глаза постепенно закрываются:
        // плавно затемняем весь экран.
        float fadeTimer = 0f;

        while (fadeTimer < closeEyesDuration)
        {
            fadeTimer += Time.deltaTime;

            float t = Mathf.Clamp01(
                fadeTimer / closeEyesDuration
            );

            blackScreen.alpha = t * t * (3f - 2f * t);

            yield return null;
        }

        blackScreen.alpha = 1f;
        blackScreen.blocksRaycasts = true;

        // Оставляем игру в завершённом состоянии.
        // Управление намеренно не возвращаем.
        Debug.Log("Финальная кат-сцена завершена.");
    }

    private IEnumerator RotateCamera(
        Transform cameraTransform,
        Quaternion startRotation,
        Quaternion targetRotation,
        float duration)
    {
        float timer = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Smooth(timer / duration);

            cameraTransform.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        cameraTransform.localRotation = targetRotation;
    }

    private float Smooth(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }
}
