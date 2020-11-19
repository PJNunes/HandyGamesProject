using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the behaviour of the bug character
public class BugBehaviourScript : CharacterBehaviourScript
{

    // Its the reference to the hostage which this bug is leading
    GameObject hostage;

    // This sets the initial information of the bug, setting the Y coordinates of the movement and its layer
    override
    public void SetInfo(float[] info)
    {
        yCoordinatesMovement = info[0];
        gameObject.layer = 9+(int) info[1];
        spriteRenderer.sortingLayerName = "Char"+info[1];
    }

    // The iteraction that happens when the bug is hit
    override
    public void Hit(){
        // If leading a hostage, send message to hostage, informing that it was killed
        if(hostage)
        {
            hostage.SendMessage("BugKilled");
        }
        Killed(); 
    }

    // This handles what happens when the bug goes out of bonds
    override
    public void OutOfBounds(){
        Destroy(gameObject);
    }

    // Routine called to fade out the bug
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

        Destroy(gameObject);        
    }

    // Function to set the reference to the hostage
    public void SetHostage(GameObject host){
        hostage = host;
    }    
}
