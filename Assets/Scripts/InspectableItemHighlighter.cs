using UnityEngine;

public class InspectableItemHighlighter : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float rayDistance = 3f;

    private InspectableItem currentItem;

    private void Update()
    {
        InspectableItem foundItem = null;

        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                InspectableItem candidate = hit.collider.GetComponentInParent<InspectableItem>();

                if (candidate != null)
                {
                    foundItem = candidate;
                    break;
                }
            }
        }

        if (currentItem != foundItem)
        {
            if (currentItem != null)
                currentItem.SetHighlight(false);

            currentItem = foundItem;

            if (currentItem != null)
                currentItem.SetHighlight(true);
        }
    }

    private void OnDisable()
    {
        if (currentItem != null)
            currentItem.SetHighlight(false);

        currentItem = null;
    }
}