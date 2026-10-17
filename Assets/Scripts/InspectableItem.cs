using UnityEngine;

public class InspectableItem : MonoBehaviour, Interaction
{
    [SerializeField] private ItemInspectionController inspectionController;

    // Белая подсветка предмета
    [SerializeField] private Outline[] outlines;

    // Название предмета при наведении
    [SerializeField] private string itemName = "Предмет";

    public string ItemName => itemName;


    // Возможность брать предмет с собой
    [SerializeField] private bool canCarry = false;

    public bool CanCarry => canCarry;

    public void Interact()
    {
        if (inspectionController != null)
        {
            inspectionController.BeginInspection(transform);
        }
        else
        {
            Debug.LogWarning(
                "InspectableItem: Inspection Controller is not assigned!",
                this
            );
        }
    }

    // Белая подсветка предмета
    public void SetHighlight(bool enabled)
    {
        if (outlines == null)
            return;

        foreach (Outline outline in outlines)
        {
            if (outline != null)
                outline.enabled = enabled;
        }
    }
}