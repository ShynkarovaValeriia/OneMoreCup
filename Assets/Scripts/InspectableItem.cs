using UnityEngine;

public class InspectableItem : MonoBehaviour, Interaction
{
    [Header("References")]
    [SerializeField] private ItemInspectionController inspectionController;

    [SerializeField] private Outline[] outlines;

    [Header("Item Settings")]
    [SerializeField] private string itemName = "Предмет";

    [SerializeField] private bool canCarry = false;

    [Header("Power Outage")]
    [SerializeField] private PowerOutage powerOutage;

    [SerializeField] private bool canCarryOnlyAfterPowerOff = false;

    [Header("Carrying Settings")]
    [SerializeField] private bool canDrop = true;

    [SerializeField] private GameObject carriedVisual;

    public string ItemName => itemName;

    public bool CanCarry
    {
        get
        {
            if (!canCarry)
                return false;

            if (canCarryOnlyAfterPowerOff)
            {
                return powerOutage != null &&
                       powerOutage.IsPowerOff;
            }

            return true;
        }
    }

    public bool CanDrop => canDrop;

    public GameObject CarriedVisual => carriedVisual;

    [Header("Carried Visual Transform")]
    [SerializeField]
    private Vector3 carriedPosition =
    new Vector3(0.35f, -0.25f, 0.8f);

    [SerializeField]
    private Vector3 carriedRotation =
        new Vector3(0f, 0f, 0f);

    [SerializeField]
    private Vector3 carriedScale =
        new Vector3(0.3f, 0.3f, 0.3f);

    public Vector3 CarriedPosition => carriedPosition;
    public Vector3 CarriedRotation => carriedRotation;
    public Vector3 CarriedScale => carriedScale;

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