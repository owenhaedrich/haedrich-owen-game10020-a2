using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Listen for the Time Sword storing and restoring ISnapshottables and update the HUD to show the snapshotted objects
public class HUD : MonoBehaviour
{
    public TextMeshProUGUI TimeSwordSnapshotsText;

    private List<ISnapshottable> storedSnapshots = new List<ISnapshottable>();

    void Awake()
    {
        ClearTimeSwordSnapshotsText();
    }

    void Start()
    {
        // Connect listeners to the Time Sword events
        TimeSword timeSword = FindFirstObjectByType<TimeSword>();
        timeSword.onSnapshot.AddListener(UpdateTimeSwordSnapshotsText);
        timeSword.onRestore.AddListener(ClearTimeSwordSnapshotsText);
    }

    void UpdateTimeSwordSnapshotsText(ISnapshottable snapshottable)
    {
        if (storedSnapshots.Contains(snapshottable)) return; // Don't show a particular object multiple times

        storedSnapshots.Add(snapshottable);
        string storedObjectsStrings = "";
        foreach (ISnapshottable snapshot in storedSnapshots)
        {
            storedObjectsStrings += "\n" + snapshot.GetName();
        }

        TimeSwordSnapshotsText.text = $"Stored Object: \n{storedObjectsStrings}";
    }

    void ClearTimeSwordSnapshotsText()
    {
        storedSnapshots.Clear();
        TimeSwordSnapshotsText.text = "No Stored Object.";
    }
}
