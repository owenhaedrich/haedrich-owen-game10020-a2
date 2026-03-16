using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI TimeSwordSnapshotsText;

    private TimeSword _timeSword;
    private List<string> storedObjects = new List<string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClearTimeSwordSnapshotsText();

        _timeSword = FindFirstObjectByType<TimeSword>();
        _timeSword.onTimeSwordSnapshot.AddListener(UpdateTimeSwordSnapshotsText);
        _timeSword.onTimeSwordRestore.AddListener(ClearTimeSwordSnapshotsText);
    }

    void UpdateTimeSwordSnapshotsText(ISnapshottable snapshottable)
    {
        storedObjects.Add(snapshottable.GetName());
        string storedObjectsList = string.Join("\n", storedObjects.ToArray());
        TimeSwordSnapshotsText.text = $"Stored Object: \n{storedObjectsList}";
    }

    void ClearTimeSwordSnapshotsText()
    {
        storedObjects.Clear();
        TimeSwordSnapshotsText.text = "No Stored Object.";
    }
}
