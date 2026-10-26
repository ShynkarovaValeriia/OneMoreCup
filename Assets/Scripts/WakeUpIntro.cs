using System.Collections;
using UnityEngine;
using TMPro;

public class WakeUpIntro : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform playerCamera;

    [Header("Screen")]
    [SerializeField] private CanvasGroup blackScreen;

    [Header("Subtitles")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private GameObject subtitleBackground;
    [SerializeField] private string wakeUpLine = "Треба зробити чай...";

    [Header("Tea Reminder")]
    [SerializeField] private float reminderInterval = 30f;
    [SerializeField] private float reminderDuration = 3f;

    private bool teaTaskCompleted = false;
    private Coroutine reminderCoroutine;

    [Header("Cup")]
    [SerializeField] private Transform cup;
    [SerializeField] private Rigidbody cupRigidbody;
    [SerializeField] private Collider cupCollider;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float initialPause = 1f;
    [SerializeField] private float lookDuration = 1.4f;
    [SerializeField] private float cupPause = 0.7f;
    [SerializeField] private float cupMoveDuration = 0.8f;
    [SerializeField] private float standUpDuration = 1.5f;

    [Header("Camera")]
    [SerializeField] private float seatedHeight = 0.35f;
    [SerializeField] private float lookLeftAngle = -30f;
    [SerializeField] private float lookRightAngle = 30f;
    [SerializeField] private float lookDownAngle = 25f;

    [Header("Cup Position Relative To Camera")]
    [SerializeField]
    private Vector3 cupViewOffset =
        new Vector3(-0.12f, -0.12f, 0.45f);

    [SerializeField]
    private Vector3 cupInspectionRotation =
    new Vector3(0f, 0f, 90f);

    private Transform playerTransform;
    private Vector3 standingCameraPosition;
    private Quaternion originalPlayerRotation;
    private Quaternion originalCameraRotation;

    private Vector3 originalCupPosition;
    private Quaternion originalCupRotation;

    private bool originalCupKinematic;
    private bool originalCupColliderEnabled;

    private void Awake()
    {
        playerTransform = playerController.transform;

        standingCameraPosition = playerCamera.localPosition;
        originalPlayerRotation = playerTransform.rotation;
        originalCameraRotation = playerCamera.localRotation;

        originalCupPosition = cup.position;
        originalCupRotation = cup.rotation;

        if (cupRigidbody != null)
        {
            originalCupKinematic = cupRigidbody.isKinematic;
            cupRigidbody.isKinematic = true;
        }

        if (cupCollider != null)
        {
            originalCupColliderEnabled = cupCollider.enabled;
            cupCollider.enabled = false;
        }

        playerController.enabled = false;

        Vector3 seatedPosition = standingCameraPosition;
        seatedPosition.y = seatedHeight;
        playerCamera.localPosition = seatedPosition;

        blackScreen.alpha = 1f;
        blackScreen.blocksRaycasts = true;

        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);

        if (subtitleBackground != null)
        {
            subtitleBackground.SetActive(false);
        }
    }

    private IEnumerator Start()
    {
        yield return FadeScreen(1f, 0f);

        yield return new WaitForSeconds(initialPause);

        // Sleepy glance to the left and back.
        yield return LookAround(lookLeftAngle);
        yield return LookAround(0f);

        // Glance to the right and return to the television.
        yield return LookAround(lookRightAngle);
        yield return LookAround(0f);

        // Look down at the cup.
        yield return RotateCameraPitch(lookDownAngle);
        yield return new WaitForSeconds(cupPause);

        yield return MoveCupToView();

        // Героиня замечает, что чашка пустая.
        subtitleText.text = wakeUpLine;
        subtitleText.gameObject.SetActive(true);

        if (subtitleBackground != null)
        {
            subtitleBackground.SetActive(true);
        }

        // Сцена продолжается без дополнительной паузы.
        yield return new WaitForSeconds(cupPause);
        yield return ReturnCup();

        yield return RotateCameraPitch(0f);
        yield return StandUp();

        // Скрываем субтитры после того, как героиня встала.
        subtitleText.gameObject.SetActive(false);

        if (subtitleBackground != null)
        {
            subtitleBackground.SetActive(false);
        }

        RestoreCupPhysics();

        blackScreen.blocksRaycasts = false;
        playerController.enabled = true;

        reminderCoroutine = StartCoroutine(TeaReminderRoutine());
    }

    private IEnumerator FadeScreen(float from, float to)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            blackScreen.alpha = Mathf.Lerp(
                from, to, Mathf.Clamp01(timer / fadeDuration));

            yield return null;
        }

        blackScreen.alpha = to;
    }

    private IEnumerator LookAround(float yawOffset)
    {
        Quaternion startRotation = playerTransform.rotation;
        Quaternion targetRotation =
            originalPlayerRotation *
            Quaternion.Euler(0f, yawOffset, 0f);

        yield return RotatePlayer(startRotation, targetRotation);
    }

    private IEnumerator RotatePlayer(
        Quaternion startRotation,
        Quaternion targetRotation)
    {
        float timer = 0f;

        while (timer < lookDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / lookDuration);
            t = t * t * (3f - 2f * t);

            playerTransform.rotation =
                Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        playerTransform.rotation = targetRotation;
    }

    private IEnumerator RotateCameraPitch(float pitch)
    {
        Quaternion startRotation = playerCamera.localRotation;
        Quaternion targetRotation =
            originalCameraRotation * Quaternion.Euler(pitch, 0f, 0f);

        float timer = 0f;

        while (timer < lookDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / lookDuration);
            t = t * t * (3f - 2f * t);

            playerCamera.localRotation =
                Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        playerCamera.localRotation = targetRotation;
    }

    private IEnumerator MoveCupToView()
    {
        Vector3 startPosition = cup.position;
        Quaternion startRotation = cup.rotation;
        Quaternion targetRotation = playerCamera.rotation * Quaternion.Euler(cupInspectionRotation);

        float timer = 0f;

        while (timer < cupMoveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / cupMoveDuration);
            t = t * t * (3f - 2f * t);

            Vector3 targetPosition =
                playerCamera.TransformPoint(cupViewOffset);

            cup.position = Vector3.Lerp(
                startPosition, targetPosition, t);

            cup.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        cup.position = playerCamera.TransformPoint(cupViewOffset);
        cup.rotation = targetRotation;
    }

    private IEnumerator ReturnCup()
    {
        Vector3 startPosition = cup.position;
        Quaternion startRotation = cup.rotation;

        float timer = 0f;

        while (timer < cupMoveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / cupMoveDuration);
            t = t * t * (3f - 2f * t);

            cup.position = Vector3.Lerp(
                startPosition, originalCupPosition, t);

            cup.rotation = Quaternion.Slerp(
                startRotation, originalCupRotation, t);

            yield return null;
        }

        cup.position = originalCupPosition;
        cup.rotation = originalCupRotation;
    }

    private IEnumerator StandUp()
    {
        Vector3 startPosition = playerCamera.localPosition;
        float timer = 0f;

        while (timer < standUpDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / standUpDuration);
            t = t * t * (3f - 2f * t);

            playerCamera.localPosition = Vector3.Lerp(
                startPosition, standingCameraPosition, t);

            yield return null;
        }

        playerCamera.localPosition = standingCameraPosition;
    }

    private void RestoreCupPhysics()
    {
        if (cupRigidbody != null)
        {
            cupRigidbody.isKinematic = originalCupKinematic;
        }

        if (cupCollider != null)
        {
            cupCollider.enabled = originalCupColliderEnabled;
        }
    }

    private IEnumerator TeaReminderRoutine()
    {
        while (!teaTaskCompleted)
        {
            // Ждём 30 секунд после предыдущего напоминания.
            yield return new WaitForSeconds(reminderInterval);

            if (teaTaskCompleted)
            {
                yield break;
            }

            // Показываем мысль героини.
            subtitleText.text = wakeUpLine;
            subtitleText.gameObject.SetActive(true);

            if (subtitleBackground != null)
            {
                subtitleBackground.SetActive(true);
            }

            // Оставляем фразу на экране на несколько секунд.
            yield return new WaitForSeconds(reminderDuration);

            if (teaTaskCompleted)
            {
                yield break;
            }

            // Скрываем субтитры до следующего напоминания.
            subtitleText.gameObject.SetActive(false);

            if (subtitleBackground != null)
            {
                subtitleBackground.SetActive(false);
            }
        }
    }

    public void CompleteTeaTask()
    {
        if (teaTaskCompleted)
        {
            return;
        }

        teaTaskCompleted = true;

        // Останавливаем дальнейшие напоминания.
        if (reminderCoroutine != null)
        {
            StopCoroutine(reminderCoroutine);
            reminderCoroutine = null;
        }

        // Убираем фразу, если она сейчас отображается.
        subtitleText.gameObject.SetActive(false);

        if (subtitleBackground != null)
        {
            subtitleBackground.SetActive(false);
        }

        Debug.Log("Задание выполнено: чайник включён!");
    }

    public void ShowMatchReminder()
    {
        StopAllCoroutines();
        StartCoroutine(ShowMatchReminderRoutine());
    }

    private System.Collections.IEnumerator ShowMatchReminderRoutine()
    {
        subtitleText.text = "Десь лежали сірники..";

        subtitleText.gameObject.SetActive(true);

        if (subtitleBackground != null)
            subtitleBackground.SetActive(true);

        yield return new WaitForSeconds(4f);

        subtitleText.gameObject.SetActive(false);

        if (subtitleBackground != null)
            subtitleBackground.SetActive(false);
    }

    private Coroutine fuseBoxReminderCoroutine;
    private bool fuseBoxReminderShown = false;

    public void ShowFuseBoxReminder()
    {
        if (fuseBoxReminderShown)
            return;

        fuseBoxReminderShown = true;

        if (fuseBoxReminderCoroutine != null)
            StopCoroutine(fuseBoxReminderCoroutine);

        fuseBoxReminderCoroutine =
            StartCoroutine(ShowFuseBoxReminderRoutine());
    }

    private IEnumerator ShowFuseBoxReminderRoutine()
    {
        subtitleText.text = "Потрібно перевірити електрощиток";

        subtitleText.gameObject.SetActive(true);

        if (subtitleBackground != null)
            subtitleBackground.SetActive(true);

        yield return new WaitForSeconds(4f);

        subtitleText.gameObject.SetActive(false);

        if (subtitleBackground != null)
            subtitleBackground.SetActive(false);

        fuseBoxReminderCoroutine = null;
    }
}
