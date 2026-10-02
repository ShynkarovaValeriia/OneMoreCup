using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject interactionText;

    public void Show()
    {
        interactionText.SetActive(true);
    }

    public void Hide()
    {
        interactionText.SetActive(false);
    }
}
