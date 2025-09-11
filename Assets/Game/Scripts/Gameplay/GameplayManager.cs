using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private InputManager input;

    private void OnEnable()
    {
        input.OnMainMenuInput += BackToMainMenu;
    }

    private void OnDestroy()
    {
        input.OnMainMenuInput -= BackToMainMenu;
    }

    private void BackToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }
}
