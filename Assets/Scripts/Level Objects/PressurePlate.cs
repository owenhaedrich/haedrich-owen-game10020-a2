using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Send the onToggle event when something is on the pressure plate or everything leaves the pressure plate
public class PressurePlate : MonoBehaviour
{
    public UnityEvent<bool> onToggle = new UnityEvent<bool>(); // Share the toggle state when toggled

    public ToggleColour toggleColour = ToggleColour.Red;

    private List<GameObject> objectsOnPressurePlates = new List<GameObject>();

    private void Start()
    {
        // Check for objects already on the pressure plate at start
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        // Use OverlapBox to find colliders within the trigger's bounds
        Vector3 center = transform.TransformPoint(boxCollider.center);
        Vector3 halfExtents = Vector3.Scale(boxCollider.size, transform.lossyScale) * 0.5f;
        Collider[] colliders = Physics.OverlapBox(center, halfExtents, transform.rotation);

        foreach (Collider col in colliders)
        {
            // Only count if it's not our own collider and it's not a trigger (unless we want to count other triggers)
            if (col.gameObject != gameObject && !col.isTrigger)
            {
                if (!objectsOnPressurePlates.Contains(col.gameObject))
                {
                    objectsOnPressurePlates.Add(col.gameObject);
                }
            }
        }

        if (objectsOnPressurePlates.Count > 0)
        {
            onToggle.Invoke(true);
            PressurePlateManager.Instance.TogglePlate(toggleColour, true);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return; // Ignore other triggers
        if (objectsOnPressurePlates.Contains(other.gameObject)) return;

        objectsOnPressurePlates.Add(other.gameObject);
        
        if (objectsOnPressurePlates.Count == 1)
        {
            onToggle.Invoke(true);
            PressurePlateManager.Instance.TogglePlate(toggleColour, true);
        }
    }
    
    // Activate and send an inactives toggle event when everything leaves the pressure plate
    private void OnTriggerExit(Collider other)
    {
        if (!objectsOnPressurePlates.Contains(other.gameObject)) return;

        objectsOnPressurePlates.Remove(other.gameObject);
        
        if (objectsOnPressurePlates.Count == 0)
        {
            onToggle.Invoke(false);
            PressurePlateManager.Instance.TogglePlate(toggleColour, false);
        }
    }
}
