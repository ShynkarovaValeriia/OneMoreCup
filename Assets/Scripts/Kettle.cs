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

    [SerializeField] private Collider interactionCollider;

    private bool powerOutageHandled = false;

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

    private void Update()
    {
        if (powerOutage == null || !powerOutage.IsPowerOff)
        {
            return;
        }

        if (powerOutageHandled)
        {
            return;
        }

        powerOutageHandled = true;

        // Останавливаем пар и удаляем оставшиеся частицы.
        if (steam != null)
        {
            steam.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        // Выключаем свет чайника.
        if (kettleLight != null)
        {
            kettleLight.enabled = false;
        }

        // Убираем возможность наводиться на чайник.
        if (interactionCollider != null)
        {
            interactionCollider.enabled = false;
        }

        Debug.Log("Чайник отключился из-за аварии электричества.");
    }
}