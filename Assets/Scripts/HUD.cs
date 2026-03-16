using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI TimeSwordSnapshotsText;

    private TimeSword _timeSword;

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
        TimeSwordSnapshotsText.text = $"Stored Object: \n{snapshottable.GetName()}";
    }

    void ClearTimeSwordSnapshotsText()
    {
        TimeSwordSnapshotsText.text = "No Stored Object.";
    }
}
