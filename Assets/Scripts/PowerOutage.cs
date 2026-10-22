using System.Collections;
using UnityEngine;

public class PowerOutage : MonoBehaviour
{
    [Header("Apartment Lights")]
    [SerializeField] private Light[] apartmentLights;

    [Header("Timing")]
    [SerializeField] private float delayBeforeFlicker = 3f;
    [SerializeField] private float flickerDuration = 8f;

    private bool outageStarted = false;

    public void StartPowerOutage()
    {
        // Не запускаем аварию повторно.
        if (outageStarted)
        {
            return;
        }

        outageStarted = true;
        StartCoroutine(FlickerLights());
    }

    private IEnumerator FlickerLights()
    {
        // Небольшая задержка после включения чайника.
        yield return new WaitForSeconds(delayBeforeFlicker);

        float elapsed = 0f;

        // Свет начинает мигать всё быстрее.
        while (elapsed < flickerDuration)
        {
            float progress = elapsed / flickerDuration;

            // Постепенно сокращаем длительность мигания.
            float interval = Mathf.Lerp(0.8f, 0.12f, progress);

            // Все лампы гаснут одновременно.
            SetApartmentLights(false);
            yield return new WaitForSeconds(interval * 0.5f);

            // Затем снова включаются.
            SetApartmentLights(true);
            yield return new WaitForSeconds(interval * 0.5f);

            elapsed += interval;
        }

        // Финальное отключение электричества.
        SetApartmentLights(false);

        Debug.Log("Авария электричества: свет во всей квартире отключён!");
    }

    private void SetApartmentLights(bool state)
    {
        if (apartmentLights == null)
        {
            return;
        }

        foreach (Light apartmentLight in apartmentLights)
        {
            if (apartmentLight != null)
            {
                apartmentLight.enabled = state;
            }
        }
    }
}