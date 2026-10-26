using System.Collections;
using UnityEngine;

public class PowerOutage : MonoBehaviour
{
    [Header("Apartment Lights")]
    [SerializeField] private Light[] apartmentLights;

    [Header("Power Outage Timing")]
    [SerializeField] private float delayBeforeFlicker = 3f;
    [SerializeField] private float flickerDuration = 8f;

    [Header("Power Restoration Timing")]
    [SerializeField] private float restorationDuration = 6f;
    [SerializeField] private float restorationStartInterval = 0.15f;
    [SerializeField] private float restorationEndInterval = 1.0f;

    [Header("Story")]
    [SerializeField] private WakeUpIntro wakeUpIntro;

    [Header("Held Items")]
    [SerializeField] private ItemInspectionController itemInspectionController;

    private bool outageStarted = false;
    private bool restorationStarted = false;

    public bool IsPowerOff { get; private set; } = false;
    public bool IsPowerRestoring { get; private set; } = false;
    public bool IsPowerRestored { get; private set; } = false;

    public void StartPowerOutage()
    {
        if (outageStarted || IsPowerRestored)
            return;

        outageStarted = true;
        StartCoroutine(FlickerLights());
    }

    private IEnumerator FlickerLights()
    {
        yield return new WaitForSeconds(delayBeforeFlicker);

        float elapsed = 0f;

        while (elapsed < flickerDuration)
        {
            float progress = elapsed / flickerDuration;
            float interval = Mathf.Lerp(0.8f, 0.12f, progress);

            SetApartmentLights(false);
            yield return new WaitForSeconds(interval * 0.5f);

            SetApartmentLights(true);
            yield return new WaitForSeconds(interval * 0.5f);

            elapsed += interval;
        }

        SetApartmentLights(false);

        IsPowerOff = true;

        StartCoroutine(ShowMatchReminderAfterDelay());

        Debug.Log("Авария электричества: свет во всей квартире отключён!");
    }

    private IEnumerator ShowMatchReminderAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        if (wakeUpIntro != null)
            wakeUpIntro.ShowMatchReminder();
    }

    public void RestorePower()
    {
        // Восстанавливать можно только после отключения.
        if (!IsPowerOff || restorationStarted || IsPowerRestored)
            return;

        restorationStarted = true;
        StartCoroutine(RestorePowerRoutine());
    }

    private IEnumerator RestorePowerRoutine()
    {
        IsPowerRestoring = true;

        Debug.Log("Началось восстановление электричества!");

        float elapsed = 0f;

        while (elapsed < restorationDuration)
        {
            float progress = elapsed / restorationDuration;

            // Мигание начинается быстро и постепенно замедляется.
            float interval = Mathf.Lerp(
                restorationStartInterval,
                restorationEndInterval,
                progress
            );

            SetApartmentLights(true);
            yield return new WaitForSeconds(interval);

            SetApartmentLights(false);
            yield return new WaitForSeconds(interval * 0.5f);

            elapsed += interval * 1.5f;
        }

        // Финальное состояние: свет остаётся включённым.
        SetApartmentLights(true);

        IsPowerOff = false;
        IsPowerRestoring = false;
        IsPowerRestored = true;

        if (itemInspectionController != null)
        {
            itemInspectionController.ExtinguishCarriedMatch();
        }

        Debug.Log("Электричество восстановлено!");
    }

    private void SetApartmentLights(bool state)
    {
        if (apartmentLights == null)
            return;

        foreach (Light apartmentLight in apartmentLights)
        {
            if (apartmentLight != null)
                apartmentLight.enabled = state;
        }
    }
}