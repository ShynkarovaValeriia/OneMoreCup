using UnityEngine;

public class InspectableItem : MonoBehaviour, Interaction
{
    [SerializeField] private ItemInspectionController inspectionController;

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
}