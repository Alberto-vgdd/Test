using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
  
    [Header("General Movement Parameters")]
    [Tooltip("Maximum speed the character can achieve.")]
    public float maximumMovementSpeed = 4f;
    [Tooltip("The speed measured in Degrees/seconds.")]
    public float turnSpeed = 720f;
    [Tooltip("Multiplier value used to increase or decrease the gravity effect.")]
    public float gravityScale = 2f;

    [Header("Step Climbing")]
    [Tooltip("Maximum step height the character can climb to. (THIS VALE MUST BE EQUAL TO THE CAPSULE COLLIDER'S RADIUS)")]
    public float stepMaxHeight = 0.25f;
    [Tooltip("Minimum free of collider depth that a step must have on top to be climbable.")]
    public float stepMinDepth = 0.4f;

    [Header("Steep Slope Sliding")]
    [Tooltip("Minimum angle between the slope and the ground to consider it a steep.")]
    public float steepSlopeAngle = 50f;
    [Tooltip("Minimum speed the character will get while sliding.")]
    public float steepSlidingSpeed = 8f;
    
    
    // Input variables.
    // Transform to calculate the direction of the movement.
    // Movement Axes for the player. M = V + H
    Transform cameraTransform;
    Vector2 movementInput;
    bool jumpInput;
    Vector3 movementDirection;
    Vector3 horizontalDirection;
    Vector3 verticalDirection;

    // Physics variables (Raycasts, Capsulecasts, etc.)
    LayerMask environmentLayerMask;  
    RaycastHit[] capsulecastHitArray;
    Vector3 point1;
    Vector3 point2;
    float radius;
    float radiusScale = 0.95f;

    public bool playerCloseToGround;
    public bool playerJumping;
    public bool playerSliding;
    public bool playerGrounded;

    Vector3 floorNormal;
    float slopeAngle;

    

    // Player variables
    private Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCapsuleCollider;
    private Transform playerTransform;


    
    // Use this for initialization
    void Awake ()
    {
        SystemAndData.PlayerTransform = playerTransform = transform;
        playerAnimator = playerTransform.GetComponentInChildren<Animator>();
        playerRigidbody = playerTransform.GetComponent<Rigidbody>();
        playerCapsuleCollider = playerTransform.GetComponent<CapsuleCollider>();
        radius = playerCapsuleCollider.radius;
    }

    void Start()
    {
        cameraTransform = SystemAndData.PlayerCamera.transform;
        environmentLayerMask = SystemAndData.EnvironmentLayerMask;

        Screen.SetResolution(853, 480, true, 0);
        Application.targetFrameRate = 30;
        
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

        // Jump Input
        if (SystemAndData.GetJumpButtonDown()) 
        {
            jumpInput = true;
        }


        //Rotate the player (Why is this here? Because unity can't interpolate rotations in no-kinematic objects).
        if (playerRigidbody.velocity.magnitude > 0f)
        {
            Vector3 velocityDirection = Vector3.Scale(playerRigidbody.velocity,new Vector3(1,0,1)).normalized;
            
            if (playerSliding)
            {
                playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,Quaternion.LookRotation(velocityDirection,Vector3.up),turnSpeed*2*Time.deltaTime) );
            }
            else
            {
                if (!SystemAndData.IsEnemyLocked)
                {                    
                    if ( movementInput.magnitude > 0.01f && playerRigidbody.velocity.magnitude > 0.01f)
                    {
                        if( Vector3.Angle(playerTransform.forward,velocityDirection) > 135)
                        {
                            playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,Quaternion.LookRotation(velocityDirection,Vector3.up),turnSpeed*2*Time.deltaTime ));
                        }
                        else
                        {
                            playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,Quaternion.LookRotation(velocityDirection,Vector3.up),turnSpeed*Time.deltaTime) );
                        }
                    }
                }
                else
                {
                    playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,Quaternion.LookRotation(velocityDirection,Vector3.up),turnSpeed*Time.deltaTime) );
                }
            }      
        }


        // Animations
        playerAnimator.SetBool("Fall", !playerCloseToGround);
        playerAnimator.SetBool("Slide", playerSliding);
        playerAnimator.SetFloat("Walk Speed",movementInput.magnitude );  


    }


    void FixedUpdate()
    {

        // This is used to update variables for the capsule casts.
        UpdatePlayerCapsulePosition();

        // CapsuleCast Below the player to determine the grounded/sliding state
        capsulecastHitArray = CapsuleCastFromPlayer(radiusScale,Vector3.down, Mathf.Abs(Mathf.Min(playerRigidbody.velocity.y*Time.fixedDeltaTime,-radius)),environmentLayerMask.value);


        // If the player is grounded asume the player is sliding too
        if (capsulecastHitArray.Length > 0 ) 
        {   
            playerGrounded = false;     
            playerCloseToGround = true; 
            playerSliding = true;
            
            // Character has landed.
            if (playerJumping && playerRigidbody.velocity.y < 0)
            {
                playerJumping = false;
            }

            // For every hit in raycastHitArray from the closest to the furthest, check if it is not a slope.
            for (int i = 0; i<capsulecastHitArray.Length; i++)
            {
                if (capsulecastHitArray[i].distance < 0.05f)
                {
                    playerGrounded = true;

                    if (Vector3Equal(capsulecastHitArray[i].point,Vector3.zero))
                    {
                        Debug.Log("Random Slide Problems");
                        continue;
                    }
                }

                // Using hit.normal returns the COLLISION'S NORMAL. (To avoid problems when falling I've used a timer.)
                floorNormal = capsulecastHitArray[i].normal;
                slopeAngle = Vector3.Angle(floorNormal, Vector3.up);

                // Check if the ground's surface isn't a slope and the normal isn't "pointing downwards"
                if ( slopeAngle <= steepSlopeAngle && floorNormal.y > 0 )
                {
                    playerSliding = false;
                    break;
                }
                // This raycast avoid sliding when the character is close to the edge of a slope, except when the character is already sliding.
                else if (!Physics.Raycast(point2,Vector3.down,radius+stepMaxHeight,environmentLayerMask.value))
                {
                    playerSliding = false;
                    break;
                }
            }
        }
        else
        {
            playerGrounded = false;
            playerCloseToGround = false; 
            playerSliding = false;
            playerJumping = false;
            floorNormal = Vector3.up;
            slopeAngle = 0f;
            
        }


        // Project movementDirection on the floor's normal
        ProjectMovementDirection();

        // CapsuleCast in the direction of the movement, to avoid the player to stick on walls and avoid small terrain variations.
        if (!Vector3Equal(movementDirection, Vector3.zero))
        {
            capsulecastHitArray = CapsuleCastFromPlayer(radiusScale,movementDirection,maximumMovementSpeed*Time.fixedDeltaTime,environmentLayerMask.value);
            


            // This value is used to keep the movementSpeed after constraining the direction.
            float oldMovementMagnitude = movementDirection.magnitude;

            foreach (RaycastHit capsulecastHit in capsulecastHitArray)
            {
                //For colliders that overlap the capsule at the start of the sweep, to avoid problems.
                if (Vector3Equal(Vector3.zero,capsulecastHit.point))
                {
                    continue;
                }

                // If the angle is correct...
                if ( Vector3.Angle(capsulecastHit.normal, Vector3.up) > steepSlopeAngle )
                {
                    
                    // ...and another capsule collider hits any object, or the hit height is not allowed ,or the normal is "pointing downwards"
                    float distanceToGround = capsulecastHit.point.y - (point2.y -radius);
                    if (Physics.CapsuleCast(point1+Vector3.up*stepMaxHeight,point2+Vector3.up*stepMaxHeight,radius,movementDirection,Mathf.Max(capsulecastHit.normal.y,stepMinDepth),environmentLayerMask.value) || distanceToGround > stepMaxHeight || capsulecastHit.normal.y < 0)
                    {
                        movementDirection -= Vector3.Project(movementDirection, Vector3.Scale(capsulecastHit.normal,new Vector3(1,0,1)).normalized);
                        // This break is commented because a bug that caused the character to stop when walking from one wall to another
                        //break; 
                    }
                    else
                    {
                        if (playerCloseToGround)
                        {
                            playerRigidbody.MovePosition(playerRigidbody.position+Vector3.up*distanceToGround);
                            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,0f,playerRigidbody.velocity.z);
                            break;
                        }
                        continue;
                    }
                }      
                else
                {

                    if (playerCloseToGround)
                    {

                        // If the gameObject in front isn't a slope or wall, and the character isn't falling, just use its normal as floor normal.
                        floorNormal = capsulecastHit.normal;
                        slopeAngle = Vector3.Angle(floorNormal,Vector3.up);


                        // And project the movement Direction Again
                        ProjectMovementDirection();
                        
                    }

                    continue;

                }  
                
            }

            
            // If the new movementDirection isn't 0, scale the movementDirection vector.
            if (movementDirection.magnitude >= 0.01f)
            {
                movementDirection *= oldMovementMagnitude/movementDirection.magnitude;
            }

        }

        
        //Move the player
        playerRigidbody.velocity = movementDirection * maximumMovementSpeed +  Vector3.up*playerRigidbody.velocity.y;

        // If the player is grounded, adjust the movement direction. If it is on a steep, it should fall.
        // If the character is not on a slope, project the velocity anyways to avoid problems.
        if ( playerCloseToGround && !playerJumping )
        {
            playerRigidbody.velocity = Vector3.ProjectOnPlane(playerRigidbody.velocity, floorNormal);            

            if (playerSliding)
            {  
                playerRigidbody.velocity -= Vector3.Project(playerRigidbody.velocity, Vector3.Scale(floorNormal,new Vector3(1,0,1)).normalized);
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,  -steepSlidingSpeed, playerRigidbody.velocity.z);

            }
            else
            {
                //If the user isn't giving any input, the character is touching the platform, and it isn't on a slope, prevent the character from sliding, .
                if ( movementInput.magnitude < 0.1f && playerGrounded)
                {
                    playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, Mathf.Max(playerRigidbody.velocity.y, Physics.gravity.magnitude*gravityScale*Time.fixedDeltaTime), playerRigidbody.velocity.z);
                }
                //Otherwiswe, clamp the velocity.
                else
                {
                    playerRigidbody.velocity = Vector3.ClampMagnitude(playerRigidbody.velocity,maximumMovementSpeed);
                }
            }
        }
    	
        
        //Add gravity the player.
        playerRigidbody.AddForce(Physics.gravity*gravityScale,ForceMode.Acceleration);

        // TEST JUMP
        if (jumpInput && playerCloseToGround && !playerSliding )
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,7.5f,playerRigidbody.velocity.z);
            playerJumping = true;
        }
        jumpInput = false;
        
    }

    void UpdatePlayerCapsulePosition()
    {
        point1 = playerRigidbody.position + playerCapsuleCollider.center + playerTransform.up *( playerCapsuleCollider.height / 2 - radius );
        point2 = playerRigidbody.position + playerCapsuleCollider.center - playerTransform.up *( playerCapsuleCollider.height / 2 - radius);
    }

    RaycastHit[] CapsuleCastFromPlayer(float radiusScale,Vector3 direction, float distance, int layerMask)
    {
        return Physics.CapsuleCastAll(point1,point2, radius*radiusScale, direction, distance, layerMask);
    }

    bool Vector3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001;
    }

    void ProjectMovementDirection()
    {
        movementDirection = Vector3.ProjectOnPlane(movementDirection, floorNormal);
    }

}
