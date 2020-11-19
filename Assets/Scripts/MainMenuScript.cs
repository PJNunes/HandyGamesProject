using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Handles the behavior of the Main Menu
public class MainMenuScript : MonoBehaviour
{
    // Called to start the game
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Called to quit the game
    public void QuitGame()
    {
        Application.Quit();
    }
}
