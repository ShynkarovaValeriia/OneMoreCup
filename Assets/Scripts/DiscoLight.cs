using UnityEngine;

public class DiscoLight : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light[] discoLights;

    [Header("Intensity")]
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 3f;

    [Header("Blinking")]
    [SerializeField] private float minBeatInterval = 0.25f;
    [SerializeField] private float maxBeatInterval = 0.65f;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Random Effects")]
    [SerializeField] private float blackoutChance = 0.15f;

    private float timer;
    private float nextBeat;
    private float targetIntensity;

    private void Start()
    {
        targetIntensity = maxIntensity;
        nextBeat = Random.Range(minBeatInterval, maxBeatInterval);

        foreach (Light discoLight in discoLights)
        {
            if (discoLight != null)
                discoLight.enabled = true;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextBeat)
        {
            timer = 0f;

            if (Random.value < blackoutChance)
            {
                targetIntensity = 0f;
            }
            else
            {
                targetIntensity = Random.Range(
                    minIntensity,
                    maxIntensity
                );
            }

            nextBeat = Random.Range(
                minBeatInterval,
                maxBeatInterval
            );
        }

        foreach (Light discoLight in discoLights)
        {
            if (discoLight == null)
                continue;

            discoLight.intensity = Mathf.Lerp(
                discoLight.intensity,
                targetIntensity,
                smoothSpeed * Time.deltaTime
            );
        }
    }
}