using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("End Level Settings")]
    public Collider endLevelTrigger;

    private void Start()
    {
        if (endLevelTrigger != null)
        {
            // Ensure the trigger has the LevelEndTrigger component
            if (!endLevelTrigger.TryGetComponent<LevelEndTrigger>(out var triggerLogic))
            {
                triggerLogic = endLevelTrigger.gameObject.AddComponent<LevelEndTrigger>();
            }
            
            triggerLogic.Initialize(this);
            
            // Ensure it's actually a trigger
            if (!endLevelTrigger.isTrigger)
            {
                Debug.LogWarning($"[LevelManager] Collider {endLevelTrigger.name} is not set as a trigger! Setting it now.");
                endLevelTrigger.isTrigger = true;
            }
        }
        else
        {
            Debug.LogError("[LevelManager] End Level Trigger is not assigned!");
        }
    }

    public void EndLevel()
    {
        Debug.Log("[LevelManager] End of level reached!");
        GameManager.Instance.LevelComplete();
    }
}
