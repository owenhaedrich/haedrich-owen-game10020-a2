using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI TimeSwordSnapshotsText;

    private List<ISnapshottable> storedSnapshots = new List<ISnapshottable>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClearTimeSwordSnapshotsText();

        TimeSword timeSword = FindFirstObjectByType<TimeSword>();
        timeSword.onSnapshot.AddListener(UpdateTimeSwordSnapshotsText);
        timeSword.onRestore.AddListener(ClearTimeSwordSnapshotsText);
    }

    void UpdateTimeSwordSnapshotsText(ISnapshottable snapshottable)
    {
        if (storedSnapshots.Contains(snapshottable)) return;

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
