using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip timeSwordSnapshot;
    public AudioClip timeSwordRestore;
    public AudioClip pressurePlateClick;

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        TimeSword timeSword = FindFirstObjectByType<TimeSword>();
        timeSword.onSnapshot.AddListener(PlayTimeSwordSnapshot);
        timeSword.onRestore.AddListener(PlayTimeSwordRestore);

        PressurePlate[] pressurePlates = FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
        foreach (PressurePlate pressurePlate in pressurePlates)
        {
            pressurePlate.onToggle.AddListener(PlayPressurePlateClick);
        }
    }

    void PlayTimeSwordSnapshot(ISnapshottable snapshottable)
    {
        audioSource.PlayOneShot(timeSwordSnapshot);
    }

    void PlayTimeSwordRestore()
    {
        audioSource.PlayOneShot(timeSwordRestore);
    }

    void PlayPressurePlateClick(bool active, ToggleColour colour)
    {
        audioSource.PlayOneShot(pressurePlateClick);
    }
}
