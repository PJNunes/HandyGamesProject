using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Handles the management of the overall game
public class GameManagerScript : MonoBehaviour
{

    // Reference to the spawner game object
    [SerializeField]
    GameObject spawner;

    // Reference to the pause menu object
    [SerializeField]
    GameObject pauseMenu;

    // Reference to the game over menu object
    [SerializeField]
    GameObject gameOverMenu;

    // Reference to the bullet counter object
    [SerializeField]
    GameObject bulletCounter;

    // Reference to the game over cause object
    [SerializeField]
    GameObject gameOverCause;    

    // Reference to the audio source with the background music
    [SerializeField]
    AudioSource backgroundMusic;    

    /* Enum relative to the possible states of the game:
        active - game is unpaused;
        paused - game is paused;
        over - game is over
    */
    enum GameState {active, paused, over}

    /*Enum relative to the possible causes of game over:
        bullet - game ended due to running out of bullets;
        hostage - game ended due to all characters being saved or killed
    */
    enum EndCause {bullet, hostage}
    
    // Reference to the bullet counter
    Text bulletCounterText;

    // Reference to the game over cause text
    Text gameOverCauseText;

    //Current state of the game
    GameState currentGameState;

    //Number of bullets remaining
    int bulletsRemaining;

    //Number of active hostages remaining
    int hostagesInGame;

    // X coordinates of the position for freed characters
    float freedXCoordinates;

    // Y coordinates of the position for freed characters
    float freedYCoordinates;

    // Start is called before the first frame update
    void Start()
    {
        currentGameState = GameState.active;

        hostagesInGame = 5;

        bulletsRemaining = 20;
        bulletCounterText = bulletCounter.GetComponent<Text>();
        bulletCounterText.text = bulletsRemaining.ToString();

        gameOverCauseText = gameOverCause.GetComponent<Text>();

        freedXCoordinates = 7.3f;
        freedYCoordinates = 5.4f;
    }

    void Update() {
        // Is Escape is pressed, pause or unpause the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(currentGameState == GameState.active)
                Pause();
            else if (currentGameState == GameState.paused)
                Resume();
        }
        // If game is active, and left mouse button is pressed
        else if(currentGameState == GameState.active && Input.GetMouseButtonDown(0))
        {
            // Send raycast to check which objects were hit
            RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            
            int layer, charLayer = 100, groundLayer = 99;
            Transform character = null;

            // Find the top block and character hit
            foreach(RaycastHit2D hit in hits)
            {
                layer = hit.transform.gameObject.layer;
                if(hit.transform.tag == "Character")
                {                
                    if(layer < charLayer)
                    {
                        charLayer = hit.transform.gameObject.layer;
                        character = hit.transform;
                    }
                }
                else if(layer < groundLayer)
                {
                    groundLayer = hit.transform.gameObject.layer;
                }
            }

            // If the layer of the character is the same as the block or lower, then the character was hit
            if(charLayer <= groundLayer)
            {
                BulletSpent();
                character.SendMessage("Hit");
            }
        }        
    }

    // Returns the position in which the character will move to when freed
    public Vector2 GetFreePosition(bool isDead)
    {
        Vector2 result;
        // Since dead characters are rotated, coordinates are changed slightly
        if(isDead)
            result = new Vector2(freedXCoordinates - 0.05f, freedYCoordinates - 0.05f);
        else
            result = new Vector2(freedXCoordinates, freedYCoordinates);

        // Calculate the new coordinates for the next character
        freedXCoordinates -= 0.5f;

        return result;
    }

    // Called when a character leaves the screen.
    public void HostageOut(int hostageId)
    {
        // If value of the hostage is not -1, then indicates the character left the screen by out of bonds
        // Hence, it can be spawned again        
        if(hostageId == -1)
        {
            hostagesInGame--;
            // If all hostages were freed or killed, end the game
            if (hostagesInGame == 0)
                EndGame((int) EndCause.hostage);
            else
                spawner.SendMessage("HostageOut", -1);
        }
        else
        {
            spawner.SendMessage("HostageOut", hostageId);
        }
    }

    // Called when a bullet is used
    public void BulletSpent()
    {        
        bulletsRemaining--;
        bulletCounterText.text = bulletsRemaining.ToString();

        // If all bullets are used, end the game
        if (bulletsRemaining == 0)
            EndGame((int) EndCause.bullet);
    }

    // Function called to pause the game
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        currentGameState = GameState.paused;
        backgroundMusic.volume = 0.25f;
    }

    // Function called to resume the game
    public void Resume()
    {
        pauseMenu.SetActive(false);
        backgroundMusic.volume = 0.5f;
        Time.timeScale = 1;
        currentGameState = GameState.active;
    }

    // Function called to end the game
    public void EndGame(int cause)
    {  
        switch((EndCause)cause)
        {
            case EndCause.hostage:
                gameOverCauseText.text = "All hostages were rescued or killed!";
                break;
            case EndCause.bullet:
                gameOverCauseText.text = "You run out of bullets!";
                break;
        }

        backgroundMusic.volume = 0.25f;        
        currentGameState = GameState.over;
        gameOverMenu.SetActive(true);
        Time.timeScale = 0;        
    }

    // Function called to restart the game
    public void RestartGame()
    { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }

    // Function called to quit the game
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

}
