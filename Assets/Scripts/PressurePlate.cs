using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Send the onToggle event when something is on the pressure plate or everything leaves the pressure plate
public class PressurePlate : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<bool> onToggle = new UnityEvent<bool>(); // Share the toggle state when toggled

    public ToggleColour toggleColour = ToggleColour.Red;

    private List<GameObject> objectsOnPressurePlates = new List<GameObject>();

    // Activate and send an active toggle event when something is on the pressure plate
    private void OnTriggerEnter(Collider other)
    {
        onToggle.Invoke(true);
        objectsOnPressurePlates.Add(other.gameObject);
    }
    
    // Activate and send an inactives toggle event when everything leaves the pressure plate
    private void OnTriggerExit(Collider other)
    {
        objectsOnPressurePlates.Remove(other.gameObject);
        
        if (objectsOnPressurePlates.Count == 0 ) onToggle.Invoke(false);
    }
}
