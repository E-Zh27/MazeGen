using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMenu : MonoBehaviour
{
    public DeathMenu deathMenu;
    
    public void Awake()
    {
            Cursor.lockState = CursorLockMode.None;
            deathMenu.gameObject.SetActive(true);
            Time.timeScale = 0;
    }
}
