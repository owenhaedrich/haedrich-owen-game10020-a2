using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlateManager : MonoBehaviour
{
    private static PressurePlateManager _instance;
    public static PressurePlateManager Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PressurePlateManager>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class ColorStateEvent : UnityEvent<ToggleColour, bool> { }
    
    public ColorStateEvent onColorStateChanged = new ColorStateEvent();

    private Dictionary<ToggleColour, int> _activeCounts = new Dictionary<ToggleColour, int>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void TogglePlate(ToggleColour colour, bool isActive)
    {
        if (!_activeCounts.ContainsKey(colour))
        {
            _activeCounts[colour] = 0;
        }

        int previousCount = _activeCounts[colour];
        
        if (isActive)
        {
            _activeCounts[colour]++;
        }
        else
        {
            _activeCounts[colour] = Mathf.Max(0, _activeCounts[colour] - 1);
        }

        int currentCount = _activeCounts[colour];

        bool wasActive = (previousCount % 2 != 0);
        bool nowActive = (currentCount % 2 != 0);

        if (wasActive != nowActive)
        {
            onColorStateChanged.Invoke(colour, nowActive);
        }
    }

    public bool IsColorActive(ToggleColour colour)
    {
        return _activeCounts.ContainsKey(colour) && (_activeCounts[colour] % 2 != 0);
    }
}
