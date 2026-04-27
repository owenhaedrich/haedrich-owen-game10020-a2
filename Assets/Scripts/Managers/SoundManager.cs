using UnityEngine;

// Listen for onToggle from the pressure plates, and onSnapshot and onRestore from the Time Sword. Play appropriate sound effects when the events fire.
public class SoundManager : MonoBehaviour
{
    public AudioClip timeSwordSnapshot;
    public AudioClip timeSwordRestore;
    public AudioClip pressurePlateClick;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
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

        // Listen for scene change events from GameManager
        GameManager.Instance.onSceneLoadStarted.AddListener(HandleSceneLoadStarted);
        GameManager.Instance.onSceneLoadFinished.AddListener(HandleSceneLoadFinished);
    }

    private void OnDestroy()
    {
        GameManager.Instance.onSceneLoadStarted.RemoveListener(HandleSceneLoadStarted);
        GameManager.Instance.onSceneLoadFinished.RemoveListener(HandleSceneLoadFinished);
    }

    void HandleSceneLoadStarted(string sceneName)
    {
        Debug.Log($"[SoundManager] Scene loading started: {sceneName}. Stopping or fading out music...");
    }

    void HandleSceneLoadFinished(string sceneName)
    {
        Debug.Log($"[SoundManager] Scene loading finished: {sceneName}. Starting level music...");
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
