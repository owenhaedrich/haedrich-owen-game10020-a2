using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    public UnityEvent onToggle = new UnityEvent();

    public ToggleColour toggleColour = ToggleColour.Red;

    private void OnTriggerEnter(Collider other)
    {
        onToggle.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        onToggle.Invoke();
    }
}
