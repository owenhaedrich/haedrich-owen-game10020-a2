using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip timeSwordSnapshot;
    public AudioClip timeSwordRestore;

    private AudioSource audioSource;
    private TimeSword _timeSword;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        _timeSword = FindFirstObjectByType<TimeSword>();

        _timeSword.onTimeSwordSnapshot.AddListener(PlayTimeSwordSnapshot);
        _timeSword.onTimeSwordRestore.AddListener(PlayTimeSwordRestore);
    }

    void PlayTimeSwordSnapshot(ISnapshottable snapshottable)
    {
        audioSource.PlayOneShot(timeSwordSnapshot);
    }

    void PlayTimeSwordRestore()
    {
        audioSource.PlayOneShot(timeSwordRestore);
    }
}
