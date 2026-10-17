using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject interactionText;

    private TMP_Text tmpText;
    private Text legacyText;

    private void Awake()
    {
        if (interactionText != null)
        {
            tmpText = interactionText.GetComponent<TMP_Text>();
            legacyText = interactionText.GetComponent<Text>();
        }
    }

    public void Show()
    {
        Show("Натисніть E");
    }

    public void Show(string message)
    {
        if (interactionText == null)
            return;

        if (tmpText != null)
            tmpText.text = message;
        else if (legacyText != null)
            legacyText.text = message;

        interactionText.SetActive(true);
    }

    public void Hide()
    {
        if (interactionText != null)
            interactionText.SetActive(false);
    }
}