using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public PauseMenu pauseMenu;
    public PlayerHealth playerHealth;
    
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape) && !pauseMenu.isActiveAndEnabled && playerHealth.health > 0)
        {
            Cursor.lockState = CursorLockMode.None;
            pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
            Time.timeScale = 0;
        }
    }
}
