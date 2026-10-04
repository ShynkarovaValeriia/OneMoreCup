using UnityEngine;

public class Door : MonoBehaviour, Interaction
{
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;

    private bool isOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void Open()
    {
        doorPivot.localRotation = Quaternion.Euler(0f, openAngle, 0f);
        isOpen = true;
    }

    private void Close()
    {
        doorPivot.localRotation = Quaternion.Euler(0f, 0f, 0f);
        isOpen = false;
    }
}
