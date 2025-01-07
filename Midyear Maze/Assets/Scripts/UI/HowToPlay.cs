using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class HowToPlay : MonoBehaviour
{
    // Called when we click the "Back" button.
    public void OnBackButton ()
    {
        SceneManager.LoadScene(0);
    }
}