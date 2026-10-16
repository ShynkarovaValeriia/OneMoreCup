using UnityEngine;

public class LightSwitch : MonoBehaviour, Interaction
{
    [Header("Lights controlled by this switch")]
    [SerializeField] private Light[] roomLights;

    private bool isOn = true;

    private void Start()
    {
        if (roomLights != null && roomLights.Length > 0
            && roomLights[0] != null)
        {
            isOn = roomLights[0].enabled;
        }
    }

    public void Interact()
    {
        if (roomLights == null || roomLights.Length == 0)
        {
            Debug.LogWarning("LightSwitch: No lights assigned!", this);
            return;
        }

        isOn = !isOn;

        foreach (Light roomLight in roomLights)
        {
            if (roomLight != null)
            {
                roomLight.enabled = isOn;
            }
        }
    }
}