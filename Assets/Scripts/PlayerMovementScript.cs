﻿using System.Collections;
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
    public float jumpSpeed = 7.5f;

    [Header("Step Climbing")]
    [Tooltip("Maximum step height the character can climb to. (THIS VALE MUST BE EQUAL TO THE CAPSULE COLLIDER'S RADIUS)")]
    public float stepMaxHeight = 0.25f;
    [Tooltip("Minimum free of collider depth that a step must have on top to be climbable.")]
    public float stepMinDepth = 0.4f;

    [Header("Steep Slope Sliding")]
    [Tooltip("Minimum angle between the slope and the ground to consider it a steep.")]
    public float steepAngle = 50f;
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

    Vector3 groundNormal;
    float groundAngle;
    float groundHeight;


    

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

        // Check If the character is close to the ground, grounded, sliding or falling.
        // Also set floorNormal and floorAngle values.
        CheckIfGroundedOrSliding();

        Jump();

        // Project movementDirection on the floor's normal to properly constraint movement direction later.
        ProjectMovementDirection();

        // If the player is in front of a wall/steep, constraint the movement direction
        // If the player is in front of a step, jump it
        // If the player is in front of a different terrain, project the movement direction.
        ConstraintMovementDirection();

        //Move the player
        playerRigidbody.velocity = movementDirection * maximumMovementSpeed +  Vector3.up*playerRigidbody.velocity.y;

        // If the character is grounded and not jumping, project the velocity to groundNormal.
        // If the character is sliding, constrain the velocity to make the it slide properly
        // If the character is not on a steep, constraint the velocity magnitude
        ProjectVelocityDirection();

        //Add gravity the player.
        playerRigidbody.AddForce(Physics.gravity*gravityScale,ForceMode.Acceleration);

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
    RaycastHit[] OptimizedCapsuleCastFromPlayer(float radiusScale,Vector3 direction, float distance, int layerMask)
    {
        return OptimizedCast.CapsuleCastAll(point1,point2, radius*radiusScale, direction, distance, layerMask);
    }

    bool Vector3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001;
    }

    void ProjectMovementDirection()
    {
        if (Vector3Equal(groundNormal, Vector3.zero))
        {
            movementDirection = Vector3.ProjectOnPlane(movementDirection, groundNormal);
        }        
    }



    void CheckIfGroundedOrSliding()
    {
        // CapsuleCast below the player with a distance at least as big as the stepMaxHeight.
        // We need 2 different grounded booleans:
        //  playerGrounded is used when the player is actually on the ground
        //  playerCloseToGround is used when the character is neither grounded or falling (for example when climbing down steps, used by the animator.) 


        capsulecastHitArray = OptimizedCapsuleCastFromPlayer(radiusScale,Vector3.down, Mathf.Abs(Mathf.Min(playerRigidbody.velocity.y*Time.fixedDeltaTime,-stepMaxHeight)),environmentLayerMask.value);

        if (capsulecastHitArray.Length > 0 ) 
        {   
            groundHeight = point2.y-radius-stepMaxHeight;
            playerCloseToGround = true;
            playerSliding = true;
            playerGrounded = false;     
            
            for (int i = capsulecastHitArray.Length-1; i >= 0 ; i--)
            {
                
                groundNormal = capsulecastHitArray[i].normal;
                groundAngle = Vector3.Angle(groundNormal, Vector3.up);

                // Check if the ground's surface isn't a slope and the normal isn't "pointing downwards"
                // The raycast avoid sliding when the character is close to the edge of a slope.
                if ( groundAngle <= steepAngle && groundNormal.y > 0 )
                {
                    playerSliding = false;
                    //break;

                }
                else if (!Physics.Raycast(point2,Vector3.down,radius+stepMaxHeight,environmentLayerMask.value))
                {
                    playerSliding = false;
                    //break;
                }

                // Check if the character is grounded
                if ( !playerGrounded && (capsulecastHitArray[i].distance*groundNormal).y < 0.05f)
                {
                    playerGrounded = true;
                    groundHeight = capsulecastHitArray[i].point.y;
                }

                if (playerGrounded && !playerSliding)
                {
                    break;
                }
                
            }
        }
        else
        {
            playerCloseToGround = false; 
            playerSliding = false;
            playerGrounded = false;
            
            
            groundNormal = Vector3.zero;
            groundAngle = float.MinValue;
            
        }
    }

    void Jump()
    {
        if (playerJumping)
        {
            capsulecastHitArray = OptimizedCapsuleCastFromPlayer(1f,playerRigidbody.velocity.normalized,maximumMovementSpeed*Time.fixedDeltaTime,environmentLayerMask.value);

            foreach (RaycastHit capsulecastHit in capsulecastHitArray)
            {
 
                // If the angle is correct...
                if ( capsulecastHit.normal.y > 0)
                {
                    playerCloseToGround = true;
                    playerJumping = false;

                    groundNormal = capsulecastHit.normal;
                    groundAngle = Vector3.Angle(groundNormal,Vector3.up);
                    
                    if (groundAngle > steepAngle)
                    {
                        playerSliding = true;
                    }

                    break;

                }
            }

        }

        // TEST JUMP
        if (jumpInput && !playerJumping && playerCloseToGround && !playerSliding )
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,jumpSpeed,playerRigidbody.velocity.z);
            playerJumping = true;
        }
        jumpInput = false;
    }

    void ConstraintMovementDirection()
    {
        // If there is a movementDirection, capsuleCast in that direction to avoid the player to stick on walls and avoid small terrain variations.
        if (!Vector3Equal(movementDirection, Vector3.zero))
        {
           
            capsulecastHitArray = OptimizedCapsuleCastFromPlayer(radiusScale,movementDirection.normalized,maximumMovementSpeed*Time.fixedDeltaTime,environmentLayerMask.value);
        
            // This value is used to mantain the input value after constraining the movementDirection.
            float oldMovementMagnitude = movementDirection.magnitude;

            //foreach (RaycastHit capsulecastHit in capsulecastHitArray)
            for (int i = capsulecastHitArray.Length-1; i >= 0; i--)
            {
                //For colliders that overlap the capsule at the start of the sweep, to avoid problems.
                if (Vector3Equal(Vector3.zero,capsulecastHitArray[i].point))
                {
                    continue;
                }

                // If the angle is correct...
                if ( Vector3.Angle(capsulecastHitArray[i].normal, Vector3.up) > steepAngle )
                {
                    float distanceToGround = Mathf.Max(0f,Vector3.Project(capsulecastHitArray[i].point -(point2 -Vector3.up*radius),groundNormal).y); 

                    // ...and the hit height is not allowed, or the normal is "pointing downwards", or another capsule collider hits any object. 
                    if (distanceToGround > stepMaxHeight || capsulecastHitArray[i].normal.y < 0 ||Physics.CapsuleCast(point1+Vector3.up*stepMaxHeight,point2+Vector3.up*stepMaxHeight,radius,movementDirection.normalized,Mathf.Max(capsulecastHitArray[i].normal.y,stepMinDepth),environmentLayerMask.value) )
                    {
                        movementDirection -= Vector3.Project(movementDirection, Vector3.Scale(capsulecastHitArray[i].normal,new Vector3(1,0,1)).normalized);
                        break; 
                    }
                    else
                    {
                        if (playerGrounded)
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

                    if (playerGrounded)
                    {

                        // If the gameObject in front isn't a slope or wall, and the character isn't falling, just use its normal as floor normal.
                        groundNormal = capsulecastHitArray[i].normal;
                        groundAngle = Vector3.Angle(groundNormal,Vector3.up);


                        // And project the movement Direction Again
                        ProjectMovementDirection();
                        
                    }

                    continue;

                }  
                
            }
            
            // If the new movementDirection isn't 0, scale the movementDirection vector.
            if (!Vector3Equal(movementDirection,Vector3.zero)) 
            {
                movementDirection *= oldMovementMagnitude/movementDirection.magnitude;
            }

        }
    }

    void ProjectVelocityDirection()
    {
        // If the player is grounded in a slope and not jumping, adjust the movement direction. If it is on a steep, it should fall.
        if ( playerGrounded && !playerJumping  )
        {
            playerRigidbody.velocity = Vector3.ProjectOnPlane(playerRigidbody.velocity, groundNormal);  
    
            if (playerSliding)
            {  
                playerRigidbody.velocity -= Vector3.Project(playerRigidbody.velocity, Vector3.Scale(groundNormal,new Vector3(1,0,1)).normalized);
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x,  -steepSlidingSpeed, playerRigidbody.velocity.z);
            }
            else
            {
                //If the user isn't giving any input, prevent the character from sliding.
                if ( movementInput.magnitude < 0.1f )
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
    }
}
