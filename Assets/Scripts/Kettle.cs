using UnityEngine;

public class Kettle : MonoBehaviour, Interaction
{
    [Header("Steam")]
    [SerializeField] private ParticleSystem steam;

    [Header("Kettle Light")]
    [SerializeField] private Light kettleLight;

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

        Debug.Log("Чайник включён: свет и пар активированы!");
    }
}