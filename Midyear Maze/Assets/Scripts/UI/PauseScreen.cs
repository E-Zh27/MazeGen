using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseScreen : MonoBehaviour
{
    public PauseMenu pauseMenu;

    // Called when we click the "Resume" button.
    public void OnResumeButton ()
    {
        pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
    }
    
    
    // Called when we click the "Main Menu" button.
    public void OnMainMenuButton ()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

}