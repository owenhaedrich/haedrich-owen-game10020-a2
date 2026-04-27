using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    private LevelManager _manager;

    public void Initialize(LevelManager manager)
    {
        _manager = manager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_manager != null)
            {
                _manager.EndLevel();
            }
            else
            {
                Debug.LogWarning("[LevelEndTrigger] No LevelManager assigned!");
            }
        }
    }
}
