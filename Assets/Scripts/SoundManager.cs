using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip timeSwordSnapshot;
    public AudioClip timeSwordRestore;
    public AudioClip pressurePlateClick;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Listen for Time Sword events and play the appropriate audio clip
        TimeSword timeSword = FindFirstObjectByType<TimeSword>();
        timeSword.onSnapshot.AddListener(PlayTimeSwordSnapshot);
        timeSword.onRestore.AddListener(PlayTimeSwordRestore);

        // Listen to all pressure plates and play an audio clip when any one of them are toggled
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

    void PlayPressurePlateClick(bool active)
    {
        audioSource.PlayOneShot(pressurePlateClick);
    }
}
