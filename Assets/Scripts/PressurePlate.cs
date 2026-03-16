using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class PressurePlate : MonoBehaviour
{
    public UnityEvent<bool, ToggleColour> onToggle = new UnityEvent<bool, ToggleColour>();

    public ToggleColour toggleColour = ToggleColour.Red;

    private List<GameObject> objectsOnPressurePlates = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        onToggle.Invoke(true, toggleColour);
        objectsOnPressurePlates.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        objectsOnPressurePlates.Remove(other.gameObject);
        
        if (objectsOnPressurePlates.Count == 0 ) onToggle.Invoke(false, toggleColour);
    }
}
