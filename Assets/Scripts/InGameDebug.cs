using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InGameDebug : MonoBehaviour
{
    public InputActionReference Reset;

    private void OnEnable()
    {
        if (Reset != null && Reset.action != null)
            Reset.action.performed += HandleReset;
    }

    private void OnDisable()
    {
        if (Reset != null && Reset.action != null)
            Reset.action.performed -= HandleReset;
    }

    private void HandleReset(InputAction.CallbackContext context)
    {
        ResetScene();
    }

   private void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
