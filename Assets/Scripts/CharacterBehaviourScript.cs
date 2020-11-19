using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the general behaviour of the characters
abstract public class CharacterBehaviourScript : MonoBehaviour
{

    //Reference to the audio clip with the kill sound
    [SerializeField]
    protected AudioClip killSound;

    // Reference to the game manager script
    protected GameManagerScript gameManager;
    
    // Reference to the rigidbody
    protected Rigidbody2D rigidBody; 

    // Reference to the animator
    protected Animator animator; 

    // Reference to the sprite renderer
    protected SpriteRenderer spriteRenderer; 

    // Reference to the audio source
    protected AudioSource audioSource; 

    /* Enum relative to the possible states of the characters: 
        moving - moving through the scene;
        saving - after character is saved, character is moving to the top corner;
        saved - character is in the top corner;
        dead - character is dead
    */
    protected enum state {moving, saving, saved, dead};

    /* Enum relative to the possible states of going through ramps: 
        noRamp - moving horizontally;
        goingE, goingW - moving through ramp;
        doneE, doneW - out of the ramp, setting necessary scripts
    */
    protected enum rampState {noRamp, goingE, doneE, goingW, doneW};

    // Current state of the character
    protected state currentState;

    // Current ramp state of the character
    protected rampState currentRampState;

    // Reference to the last ramp traversed
    GameObject ramp;

    // Speed of the character
    protected float speed;

        // Value representing the height the character is travelling
    protected float yCoordinatesMovement;

    // Progress of the rotation
    protected float rotationProgress;

    // Value for the start of the rotation
    Quaternion startRotation;

    // Value for the end of the rotation
    Quaternion endRotation;

    // Position of the character when freed
    protected Vector2 savedPosition;

    // Initial distance to the when freed position
    protected float distToSavedPosition;

    // Obtain the references necessary
    private void Awake() {
        rigidBody = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        audioSource = gameObject.GetComponent<AudioSource>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManagerScript>();        
    }

    // Start is called before the first frame update
    void Start()
    {
        currentState = state.moving;        
        currentRampState = rampState.noRamp;
        speed = 1;
        rotationProgress = 1;
    }

    // Update of the character
    void FixedUpdate()
    {
        // If moving
        if(currentState == state.moving)
        {
            // If Rotate is not finished, rotate untill reaching the required value
            if (rotationProgress < 1 && rotationProgress >= 0){
                rotationProgress += Time.deltaTime * 5;

                transform.rotation = Quaternion.Lerp(startRotation, endRotation, rotationProgress);
            }

            // Movement is dependent of the state
            switch(currentRampState)
            {
                // If going through ramp, move further up or down
                case rampState.goingE:
                    rigidBody.velocity = new Vector2(speed, -speed/3);
                    break;
                case rampState.goingW:
                    rigidBody.velocity = new Vector2(speed, speed/3);
                    break;
                // If finished going through ramp, snap to the correspondent Y coordinates and move forward
                case rampState.doneE:
                    transform.position = new Vector3(transform.position.x, yCoordinatesMovement, transform.position.z);
                    rigidBody.velocity = new Vector2(speed, 0);
                    currentRampState = rampState.noRamp;
                    break;
                case rampState.doneW:
                    transform.position = new Vector3(transform.position.x, yCoordinatesMovement, transform.position.z);
                    rigidBody.velocity = new Vector2(speed, 0);
                    currentRampState = rampState.noRamp;
                    break;
                // Anything else, just move forward
                default:
                    rigidBody.velocity = new Vector2(speed, 0);
                    break;
            }
        }
        // If character is being saved, move to the target while scaling size down
        else if(currentState == state.saving)
        {
            
            // calculate distance to move
            float step =  speed * Time.deltaTime * 4; 
            transform.position = Vector2.MoveTowards(transform.position, savedPosition, step);
            
            // calculate scale to reduce
            float dist = Vector2.Distance(transform.position, savedPosition);
            float scaling = dist/distToSavedPosition;
            float scale = scaling * 0.6f + 0.4f;            
            transform.localScale = new Vector3(scale, scale, 1);

            // Check if the position of the character and destiny are approximately equal.
            if (Vector2.Distance(transform.position, savedPosition) < 0.001f)
            {
                rigidBody.velocity = new Vector2(0, 0);
                currentState = state.saved;
            }
        }            
    }

    // Check if character entered the desctructor or a ramp
    private void OnTriggerEnter2D(Collider2D other) {
        switch(other.tag)
        {
            case "RampE":
                // Verify if is a different ramp and is on the same layer
                if(other.gameObject.layer == gameObject.layer && other.gameObject != ramp)
                {
                    currentRampState = rampState.goingE;
                    ramp = other.gameObject;
                    yCoordinatesMovement -= 0.4f;
                    StartRotating(-30);
                }

                break;
            case "RampW":
                if(other.gameObject.layer == gameObject.layer && other.gameObject != ramp)
                {
                    currentRampState = rampState.goingW;
                    ramp = other.gameObject;
                    yCoordinatesMovement += 0.4f;
                    StartRotating(30);
                }
                break;
            case "Destructor":
                OutOfBounds();
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        switch(other.tag)
        {
            case "RampE":
                // Verify if it is the same ramp that was used last, if the rotation progress is completed and is on the same layer
                if(other.gameObject.layer == gameObject.layer && rotationProgress == 1 && currentRampState == rampState.goingE && ramp == other.gameObject)
                {
                    currentRampState = rampState.doneE;
                    StartRotating(0);
                }
                break;
            case "RampW":
                if(other.gameObject.layer == gameObject.layer && rotationProgress == 1 && currentRampState == rampState.goingW && ramp == other.gameObject)
                {
                    currentRampState = rampState.doneW;
                    StartRotating(0);
                }
                break;
        }
    }

    // Function to start the rotation, setting the required information
    void StartRotating(float zPosition)
    {
        startRotation = transform.rotation;
        endRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, zPosition);
        rotationProgress = 0;
    }

    // Function to call the scenario of the character when killed
    public void Killed(){
        currentState = state.dead;
        audioSource.PlayOneShot(killSound, 0.75f);
        Destroy(GetComponent<Collider2D>());   
        rigidBody.velocity = new Vector2(0, 0);
        animator.SetTrigger("charDead");
        StartCoroutine("FadeOut"); 
    }    

    // Called if the character is hit by the player
    abstract public void Hit();

    // Called if the character goes out of bounds
    abstract public void OutOfBounds();

    // Sets the initial info of the character
    abstract public void SetInfo(float[] info);

    // Starts the Fade out routine
    abstract public IEnumerator FadeOut();
}
