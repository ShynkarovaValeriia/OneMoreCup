using UnityEngine;

public class Kettle : MonoBehaviour, Interaction
{
    [Header("Steam")]
    [SerializeField] private ParticleSystem steam;

    [Header("Kettle Light")]
    [SerializeField] private Light kettleLight;

    [Header("Story")]
    [SerializeField] private WakeUpIntro wakeUpIntro;

    [Header("Power Outage")]
    [SerializeField] private PowerOutage powerOutage;

    [Header("Interaction")]
    [SerializeField] private Collider interactionCollider;

    // Работает ли чайник в данный момент?
    private bool isBoiling = false;

    // Была ли уже запущена первая авария?
    private bool hasTriggeredOutage = false;

    // Обработали ли отключение чайника во время аварии?
    private bool powerOutageHandled = false;

    [Header("Tea Pouring")]
    [SerializeField] private ItemInspectionController itemInspectionController;
    [SerializeField] private GameObject cupTeaVisual;
    [SerializeField] private ParticleSystem teaPourEffect;
    [SerializeField] private float pourDuration = 2f;

    private bool teaHasBeenPoured = false;
    private bool isPouring = false;

    public bool TeaHasBeenPoured => teaHasBeenPoured;

    [Header("Pouring Cutscene")]
    [SerializeField] private Transform kettleVisual;
    [SerializeField] private Transform pouringTarget;

    [SerializeField] private float kettleMoveDuration = 0.8f;
    [SerializeField] private float teaPourDuration = 2f;

    private ParticleSystem teaStreamParticles;

    private void Awake()
    {
        // При запуске игры чайник выключен.
        if (steam != null)
        {
            steam.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        if (kettleLight != null)
        {
            kettleLight.enabled = false;
        }
    }

    private void Start()
    {
        if (teaPourEffect != null)
        {
            teaStreamParticles = teaPourEffect;
        }

        if (teaStreamParticles != null)
        {
            teaStreamParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    public void Interact()
    {
        // Если чайник работает, электричество восстановлено,
        // а игрок держит чашку — начинаем кат-сцену наливания.
        if (isBoiling &&
            itemInspectionController != null &&
            itemInspectionController.IsHoldingCup &&
            !teaHasBeenPoured &&
            !isPouring &&
            powerOutage != null &&
            powerOutage.IsPowerRestored)
        {
            StartCoroutine(PourTeaCutscene());
            return;
        }

        // Нельзя повторно включить уже работающий чайник.
        if (isBoiling)
            return;

        // Во время отключения и восстановления электричества
        // чайник включать нельзя.
        if (powerOutage != null &&
            (powerOutage.IsPowerOff ||
             powerOutage.IsPowerRestoring))
        {
            return;
        }

        // Если первая авария уже произошла, но свет ещё
        // не восстановился, повторно включать чайник нельзя.
        if (hasTriggeredOutage &&
            powerOutage != null &&
            !powerOutage.IsPowerRestored)
        {
            return;
        }

        isBoiling = true;

        // Завершаем первоначальную задачу приготовления чая
        // только при первом включении чайника.
        if (!hasTriggeredOutage && wakeUpIntro != null)
        {
            wakeUpIntro.CompleteTeaTask();
        }

        // Включаем пар.
        if (steam != null)
        {
            steam.Play();
        }

        // Включаем подсветку чайника.
        if (kettleLight != null)
        {
            kettleLight.enabled = true;
        }

        // Первое включение запускает аварию.
        // Повторное включение после ремонта этого не делает.
        if (!hasTriggeredOutage)
        {
            hasTriggeredOutage = true;

            if (powerOutage != null)
            {
                powerOutage.StartPowerOutage();
            }
        }

        Debug.Log("Чайник включён: пар и подсветка активированы!");
    }

    private void Update()
    {
        if (powerOutage == null)
            return;

        // Электричество полностью отключилось.
        if (powerOutage.IsPowerOff && !powerOutageHandled)
        {
            powerOutageHandled = true;

            // Чайник выключается из-за аварии.
            isBoiling = false;

            if (steam != null)
            {
                steam.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }

            if (kettleLight != null)
            {
                kettleLight.enabled = false;
            }

            // Запрещаем взаимодействие во время отключения.
            if (interactionCollider != null)
            {
                interactionCollider.enabled = false;
            }

            Debug.Log("Чайник отключился из-за аварии электричества.");
        }

        // Только после полного восстановления электричества
        // снова разрешаем включить чайник.
        if (powerOutage.IsPowerRestored &&
            interactionCollider != null &&
            !isBoiling)
        {
            interactionCollider.enabled = true;
        }
    }

    private System.Collections.IEnumerator PourTeaCutscene()
    {
        if (kettleVisual == null ||
            pouringTarget == null ||
            itemInspectionController == null ||
            itemInspectionController.HeldCupTransform == null)
        {
            Debug.LogWarning(
                "Kettle: назначь Kettle Visual, Pouring Target " +
                "и Item Inspection Controller.",
                this
            );

            yield break;
        }

        isPouring = true;

        // Запоминаем исходное положение чайника.
        Vector3 originalPosition = kettleVisual.position;
        Quaternion originalRotation = kettleVisual.rotation;

        // Сразу выключаем кипение.
        isBoiling = false;

        // Выключаем пар.
        if (steam != null)
        {
            steam.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        // Выключаем подсветку чайника.
        if (kettleLight != null)
        {
            kettleLight.enabled = false;
        }

        // Блокируем управление игроком на время кат-сцены.
        itemInspectionController.SetCutscenePlaying(true);

        // 1. Поднимаем чайник и перемещаем к чашке.
        yield return MoveKettle(
            kettleVisual,
            pouringTarget.position,
            pouringTarget.rotation,
            kettleMoveDuration
        );

        // 2. Включаем струю чая.
        if (teaStreamParticles != null)
        {
            teaStreamParticles.Clear();
            teaStreamParticles.Play();
        }

        yield return new WaitForSeconds(teaPourDuration);

        // 3. Останавливаем появление новых частиц чая.
        if (teaStreamParticles != null)
        {
            teaStreamParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }

        // 4. Показываем чай внутри чашки.
        if (cupTeaVisual != null)
            cupTeaVisual.SetActive(true);

        teaHasBeenPoured = true;

        // 5. Плавно возвращаем чайник на место.
        yield return MoveKettle(
            kettleVisual,
            originalPosition,
            originalRotation,
            kettleMoveDuration
        );

        // 6. Возвращаем управление игроку.
        itemInspectionController.SetCutscenePlaying(false);
        isPouring = false;

        if (wakeUpIntro != null)
        {
            wakeUpIntro.StartRestReminders();
        }

        Debug.Log("Кат-сцена завершена: чай налит.");
    }

    private System.Collections.IEnumerator MoveKettle(
        Transform target,
        Vector3 destinationPosition,
        Quaternion destinationRotation,
        float duration)
    {
        Vector3 startPosition = target.position;
        Quaternion startRotation = target.rotation;

        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            // Плавное начало и завершение движения.
            float smoothProgress =
                progress * progress * (3f - 2f * progress);

            target.position = Vector3.Lerp(
                startPosition,
                destinationPosition,
                smoothProgress
            );

            target.rotation = Quaternion.Slerp(
                startRotation,
                destinationRotation,
                smoothProgress
            );

            yield return null;
        }

        target.position = destinationPosition;
        target.rotation = destinationRotation;
    }
}