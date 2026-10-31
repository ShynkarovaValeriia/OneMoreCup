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

    private bool isBoiling = false;

    private bool hasTriggeredOutage = false;

    private bool powerOutageHandled = false;

    private bool powerRestorationHandled = false;

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

        if (isBoiling)
            return;

        if (powerOutage != null &&
            (powerOutage.IsPowerOff ||
             powerOutage.IsPowerRestoring))
        {
            return;
        }

        if (hasTriggeredOutage &&
            powerOutage != null &&
            !powerOutage.IsPowerRestored)
        {
            return;
        }

        isBoiling = true;

        if (wakeUpIntro != null)
        {
            wakeUpIntro.StopObjectiveReminder();
        }

        if (!hasTriggeredOutage && wakeUpIntro != null)
        {
            wakeUpIntro.CompleteTeaTask();
        }

        if (steam != null)
        {
            steam.Play();
        }

        if (kettleLight != null)
        {
            kettleLight.enabled = true;
        }

        if (!hasTriggeredOutage)
        {
            hasTriggeredOutage = true;

            if (powerOutage != null)
            {
                powerOutage.StartPowerOutage();
            }
        }

        Debug.Log("Чайник ввімкнено");
    }

    private void Update()
    {
        if (powerOutage == null)
            return;

        if (powerOutage.IsPowerOff && !powerOutageHandled)
        {
            powerOutageHandled = true;

            isBoiling = false;

            if (wakeUpIntro != null)
            {
                wakeUpIntro.StartObjectiveReminder(
                    "Десь лежали сірники.."
                );
            }

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

            if (interactionCollider != null)
            {
                interactionCollider.enabled = false;
            }

            Debug.Log("чайник вимкнено");
        }

        if (powerOutage.IsPowerRestored &&
            !powerRestorationHandled)
        {
            powerRestorationHandled = true;

            if (wakeUpIntro != null)
            {
                wakeUpIntro.StopObjectiveReminder();

                wakeUpIntro.ShowMakeTeaReminder();
            }
        }

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
            yield break;
        }

        isPouring = true;

        Vector3 originalPosition = kettleVisual.position;
        Quaternion originalRotation = kettleVisual.rotation;

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

        itemInspectionController.SetCutscenePlaying(true);

        yield return MoveKettle(
            kettleVisual,
            pouringTarget.position,
            pouringTarget.rotation,
            kettleMoveDuration
        );

        if (teaStreamParticles != null)
        {
            teaStreamParticles.Clear();
            teaStreamParticles.Play();
        }

        yield return new WaitForSeconds(teaPourDuration);

        if (teaStreamParticles != null)
        {
            teaStreamParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }

        if (cupTeaVisual != null)
            cupTeaVisual.SetActive(true);

        teaHasBeenPoured = true;

        yield return MoveKettle(
            kettleVisual,
            originalPosition,
            originalRotation,
            kettleMoveDuration
        );

        itemInspectionController.SetCutscenePlaying(false);
        isPouring = false;

        if (wakeUpIntro != null)
        {
            wakeUpIntro.StartRestReminders();
        }

        Debug.Log("Чашка повна");
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