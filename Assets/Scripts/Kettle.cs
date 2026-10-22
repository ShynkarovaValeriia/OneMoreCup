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

    private bool isBoiling = false;

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

    public void Interact()
    {
        // Не запускаем чайник повторно.
        if (isBoiling)
        {
            return;
        }

        isBoiling = true;

        if (wakeUpIntro != null)
        {
            wakeUpIntro.CompleteTeaTask();
        }

        // Включаем пар.
        if (steam != null)
        {
            steam.Play();
        }

        // Включаем свет.
        if (kettleLight != null)
        {
            kettleLight.enabled = true;
        }

        // Включаем мигание света.
        if (powerOutage != null)
        {
            powerOutage.StartPowerOutage();
        }

        Debug.Log("Чайник включён: свет и пар активированы!");
    }
}