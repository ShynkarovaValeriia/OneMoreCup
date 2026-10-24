using UnityEngine;

public class FuseBox : MonoBehaviour, Interaction
{
    [Header("References")]
    [SerializeField] private PowerOutage powerOutage;
    [SerializeField] private Collider interactionCollider;

    private bool hasBeenUsed = false;

    // Можно ли сейчас взаимодействовать со щитком?
    public bool CanInteract
    {
        get
        {
            return !hasBeenUsed &&
                   powerOutage != null &&
                   powerOutage.IsPowerOff &&
                   !powerOutage.IsPowerRestoring &&
                   !powerOutage.IsPowerRestored;
        }
    }

    private void Update()
    {
        // До отключения электричества и после использования
        // отключаем только Collider взаимодействия.
        if (interactionCollider != null)
        {
            interactionCollider.enabled = CanInteract;
        }
    }

    public void Interact()
    {
        if (!CanInteract)
            return;

        hasBeenUsed = true;

        if (interactionCollider != null)
            interactionCollider.enabled = false;

        powerOutage.RestorePower();
    }
}