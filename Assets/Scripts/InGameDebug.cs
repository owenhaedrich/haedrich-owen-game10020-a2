using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InGameDebug : MonoBehaviour
{
    public InputActionReference Reset;

    private void Awake()
    {
        Reset.action.performed += _ => ResetScene();
    }

   private void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
