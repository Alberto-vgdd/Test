using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    
    [Header("Camera Transform")]
    // Transform to calculate the direction of the movement.
    public Transform cameraTransform;

    // Input values.
    public Vector2 movementInput;

    // Movement Axes for the player. M = V + H
    private Vector3 horizontalDirection;
    private Vector3 verticalDirection;
    private Vector3 movementDirection;



    [Header("Movement Parameters")]
    // Values to create a smooth movement.
    public float movementSpeed;
    // public float movementSmoothTime;
    public float turnSmoothTime;
    // Ref values
    private Vector3 currentVelocity;
    private Vector3 turnSpeed;


    // Variables used to check if the player is grounded/sliding.
    // Also used to avoid sticking to walls.
    public float slopeAngle;
    private bool playerGrounded;
    private bool playerSliding;
    private RaycastHit[] raycastHitArray;
    private Vector3 floorNormal;

    public LayerMask environmentLayerMask;
    Vector3 point1;
    Vector3 point2;
    float radius;
    float radiusScale = 0.95f;


    // Player components
    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCapsuleCollider;
    private Transform playerTransform;

    [Header("Player Animator")]
    public Animator playerAnimator;
    // Variables for the Player Animation
    private bool playerWalking;
    private bool playerRunning;


    // Use this for initialization
    void Awake ()
    {
        SystemAndData.PlayerTransform = playerTransform = transform;
        playerRigidbody = playerTransform.GetComponent<Rigidbody>();
        playerCapsuleCollider = playerTransform.GetComponent<CapsuleCollider>();
        radius = playerCapsuleCollider.radius;


    }
	
	// Update is called once per frame
	void Update ()
    {
        
        // Update movement input and normalize the vector to avoid diagonal acceleration.
        movementInput = new Vector2(SystemAndData.GetHorizontalInput(),SystemAndData.GetVerticalInput()) ;

        // m_MovementInput.magnitude?
        if (Mathf.Abs(movementInput.x)+Mathf.Abs(movementInput.y) > 1 )
        {
            movementInput.Normalize();
        }

        // Update movement directions 
        if (!SystemAndData.IsEnemyLocked)
        {
            horizontalDirection = Vector3.Scale(cameraTransform.right, new Vector3(1f, 0f, 1f)).normalized;
            verticalDirection = Vector3.Scale(cameraTransform.forward, new Vector3(1f, 0f, 1f)).normalized;
        }
        else
        {

            verticalDirection = Vector3.Scale(SystemAndData.LockedEnemyTransform.position - playerTransform.position, new Vector3(1f, 0f, 1f)).normalized;
            horizontalDirection = Vector3.Cross(verticalDirection, -playerTransform.up).normalized;
        }

        movementDirection = horizontalDirection * movementInput.x + verticalDirection * movementInput.y;

       
    
    }

    void FixedUpdate()
    {
        // This is used to update variables for the capsule casts.
        UpdatePlayerCapsulePosition();

        // Asume that the player is not grounded/sliding at the start of the loop
        playerGrounded = false; 
        playerSliding = false;
        floorNormal = Vector3.up;

        // CapsuleCast Below the player to determine the grounded/sliding state
        raycastHitArray = CapsuleCastFromPlayer(Vector3.down,Mathf.Abs(Mathf.Min(playerRigidbody.velocity.y*Time.fixedDeltaTime,-0.01f)),environmentLayerMask.value);
        
        // If the player is grounded asume the player is sliding too
        if (raycastHitArray.Length > 0 ) 
        {
            playerGrounded = true; 
            playerSliding = true;

            // Check if the player is not sliding from the closest plane to the furthest.
            foreach(RaycastHit hit in raycastHitArray)
            {
                // Using hit.normal returns bad normals in the edges of the collider of the floor.
                floorNormal = hit.normal;

                if (Vector3.Angle(hit.normal, Vector3.up) < slopeAngle)
                {
                    playerSliding = false;
                    break;
                }
            }
        }

       
        

        if (movementDirection != Vector3.zero)
        {
            // CapsuleCast in the direction of the movement to avoid the player to stick on walls.
            raycastHitArray = CapsuleCastFromPlayer(movementDirection,movementSpeed*Time.fixedDeltaTime,environmentLayerMask.value);
            
            foreach(RaycastHit hit in raycastHitArray)
            {
                if (Vector3.Angle(hit.normal, Vector3.up) > slopeAngle)
                {
                    movementDirection -= Vector3.Project(movementDirection, Vector3.Scale(hit.normal,new Vector3(1,0,1)).normalized);
                }
            }
        }
        
        //Move the player
        playerRigidbody.velocity = movementDirection * movementSpeed +  Vector3.up*playerRigidbody.velocity.y;

        //If the player is on a steep, adjust the movement direction. If it is on a slope, it should fall.
        if (floorNormal != Vector3.up)
        {
            playerRigidbody.velocity = Vector3.ProjectOnPlane(playerRigidbody.velocity, floorNormal);

            if (playerSliding)
            {  
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, Mathf.Min(playerRigidbody.velocity.y, -movementSpeed), playerRigidbody.velocity.z);
            }  
        }
    	
        //Rotate the player.
        if (playerRigidbody.velocity.magnitude > 0f)
        {
            Vector3 velocityDirection = Vector3.Scale(playerRigidbody.velocity,new Vector3(1,0,1)).normalized;
            
            if (playerSliding)
            {
                playerTransform.forward = Vector3.SmoothDamp(playerTransform.forward, velocityDirection , ref turnSpeed, turnSmoothTime/2);
            }
            else
            {
                if (!SystemAndData.IsEnemyLocked)
                {                    
                    if ( movementInput.magnitude > 0.1f)
                    {
                        if( Vector3.Angle(playerTransform.forward,velocityDirection) > 135)
                        {
                            playerTransform.forward = Vector3.SmoothDamp(playerTransform.forward, velocityDirection , ref turnSpeed, turnSmoothTime/2);
                        }
                        else
                        {
                            playerTransform.forward = Vector3.SmoothDamp(playerTransform.forward, velocityDirection , ref turnSpeed, turnSmoothTime);
                        }
                    }
                }
                else
                {
                    playerTransform.forward = Vector3.SmoothDamp(playerTransform.forward, verticalDirection, ref turnSpeed, turnSmoothTime);
                }
            }
          
            
        }

        //Add gravity the player.
        playerRigidbody.AddForce(Physics.gravity*2f,ForceMode.Acceleration);
        

        // ANIMATION TEST
        playerAnimator.SetBool("Fall", !playerGrounded);
        playerAnimator.SetBool("Slide", playerSliding);
        playerAnimator.SetFloat("Walk Speed",movementInput.magnitude );

    }

    void UpdatePlayerCapsulePosition()
    {
        point1 = playerTransform.position + playerCapsuleCollider.center + playerTransform.up *( playerCapsuleCollider.height / 2 -radius*radiusScale);
        point2 = playerTransform.position + playerCapsuleCollider.center - playerTransform.up *( playerCapsuleCollider.height / 2 -radius*radiusScale);
    }

    RaycastHit[] CapsuleCastFromPlayer(Vector3 direction, float distance, int layerMask)
    {
        return Physics.CapsuleCastAll(point1,point2, radius*radiusScale, direction, distance, layerMask);
    }

}
