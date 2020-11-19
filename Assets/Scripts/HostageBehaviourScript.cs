using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the behaviour of the hostage character
public class HostageBehaviourScript : CharacterBehaviourScript
{
    // Information relative to the amount of leading bugs
    int bugsAlive;

    // Information relative to the id of the hostage
    int hostageId;

    // Called when the character is hit
    override
    public void Hit()
    {                
        gameManager.HostageOut(-1);
        Killed(); 
    }

    // Called when the character goes out of bounds
    override
    public void OutOfBounds()
    {
        gameManager.HostageOut(hostageId);
        Destroy(gameObject);
    }

    // Sets the initial info of the character
    override
    public void SetInfo(float[] info)
    {
        bugsAlive = 2;
        yCoordinatesMovement = info[0];
                
        gameObject.layer = 9+(int) info[1];
        spriteRenderer.sortingLayerName = "Char"+info[1];        

        hostageId = (int) info[2];
    }

    // Routine called to fade out the character
    override
    public IEnumerator FadeOut()
    {        
        for (float f = 1f; f >= -0.05f; f -= 0.05f) 
        {
            Color c = spriteRenderer.color;
            c.a = f;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.1f);
        StartCoroutine("FadeIn");
    }

    // Routine called to fade in the character, used when the character is killed
    IEnumerator FadeIn()
    {
        spriteRenderer.sortingLayerName = "Base";
        transform.position = gameManager.GetFreePosition(true);
        transform.localScale = new Vector3(0.4f, 0.4f, 1);
        transform.rotation = Quaternion.Euler(0, 0, 90);

        for (float f = 0f; f < 1f; f += 0.05f) 
        {
            Color c = spriteRenderer.color;
            c.a = f;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.15f);
        }
    }

    // Called when one of the leading bugs is killed
    public void BugKilled()
    {
        bugsAlive--;

        // If both bugs are killed
        if(bugsAlive == 0 && currentState == state.moving)
        {            

            audioSource.Play(0);
            currentState = state.saving;

            rigidBody.velocity = new Vector2(0, 0);

            savedPosition = gameManager.GetFreePosition(false);

            transform.rotation = Quaternion.Euler(0, 0, 0);

            distToSavedPosition = Vector2.Distance(transform.position, savedPosition);

            Destroy(GetComponent<Collider2D>());
            gameManager.HostageOut(-1);
        }
    }
}
