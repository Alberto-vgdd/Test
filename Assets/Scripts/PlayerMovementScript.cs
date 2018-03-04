using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
  
    [Header("General Movement Parameters")]
    [Tooltip("Maximum speed the character can achieve.")]
    public float maximumMovementSpeed = 4f;
    [Tooltip("Necessary seconds to perform a 360 degree turn.")]
    public float turnSmoothTime = 0.075f;
    [Tooltip("Multiplier value used to increase or decrease the gravity effect.")]
    public float gravityScale = 2f;

    [Header("Step Climbing")]
    [Tooltip("Maximum step height the character can climb to.")]
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
    Vector3 movementDirection;
    Vector3 horizontalDirection;
    Vector3 verticalDirection;

    // Physics variables (Raycasts, Capsulecasts, etc.)
    LayerMask environmentLayerMask;  
    RaycastHit[] capsulecastHitArray;
    Vector3 point1;
    Vector3 point2;
    float radius;
    float radiusScale = 0.99f;
    bool playerGrounded;
    bool playerSliding;
    Vector3 floorNormal;
    float slideTime = 0.2f;
    float slideTimer = -1f;
    

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

       

        //Rotate the player (Why is this here? Because unity can't interpolate rotations in no-kinematic objects).
        if (playerRigidbody.velocity.magnitude > 0f)
        {
            Vector3 velocityDirection = Vector3.Scale(playerRigidbody.velocity,new Vector3(1,0,1)).normalized;
            
            if (playerSliding)
            {
                playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,Quaternion.LookRotation(velocityDirection,Vector3.up),(360/turnSmoothTime/2)*Time.deltaTime) );
            }
            else
            {
                if (!SystemAndData.IsEnemyLocked)
                {                    
                    if ( movementInput.magnitude > 0.01f && playerRigidbody.velocity.magnitude > 0.01f)
                    {
                        if( Vector3.Angle(playerTransform.forward,velocityDirection) > 135)
                        {
                            playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,Quaternion.LookRotation(velocityDirection,Vector3.up),(360/turnSmoothTime/2)*Time.deltaTime) );
                        }
                        else
                        {
                            playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,Quaternion.LookRotation(velocityDirection,Vector3.up),(360/turnSmoothTime)*Time.deltaTime) );
                        }
                    }
                }
                else
                {
                    playerRigidbody.MoveRotation( Quaternion.RotateTowards(playerRigidbody.rotation,Quaternion.LookRotation(velocityDirection,Vector3.up),(360/turnSmoothTime)*Time.deltaTime) );
                }
            }      
        }

        //This is used to make the player slide ONLY when it has been on a slope for more than Xs
        if (slideTimer >= 0f)
        {
            if (slideTimer >= slideTime)
            {
                playerSliding = true;
            }
            else
            {
                slideTimer += Time.deltaTime;
            }
        }

        // Animations
        playerAnimator.SetBool("Fall", !playerGrounded);
        playerAnimator.SetBool("Slide", playerSliding);
        playerAnimator.SetFloat("Walk Speed",movementInput.magnitude );  


    }


    void FixedUpdate()
    {
        // This is used to update variables for the capsule casts.
        UpdatePlayerCapsulePosition();

        // CapsuleCast Below the player to determine the grounded/sliding state
        capsulecastHitArray = CapsuleCastFromPlayer(Vector3.down, Mathf.Abs(Mathf.Min(playerRigidbody.velocity.y*Time.fixedDeltaTime,-stepMaxHeight)),environmentLayerMask.value);


        // If the player is grounded asume the player is sliding too
        if (capsulecastHitArray.Length > 0 ) 
        {
            playerGrounded = true; 
            playerSliding = true;

            // For every hit in raycastHitArray from the furthest away to the closest, check if it is not a slope.
            for (int i = capsulecastHitArray.Length-1; i >= 0; i--)
            {
            
                // Using hit.normal returns the COLLISION'S NORMAL. (To avoid problems when falling I've used a timer.)
                floorNormal = capsulecastHitArray[i].normal;

                // Check if the ground's surface isn't a slope and the normal isn't "pointing downwards"
                if (Vector3.Angle(capsulecastHitArray[i].normal, Vector3.up) <= steepSlopeAngle && capsulecastHitArray[i].normal.y > 0 )
                {
                    playerSliding = false;
                    slideTimer = -1f;
                    break;
                }
            }
            
            // If the player is about to slide, don't make it slide until a timer completes without interrumptions to avoid problems with the collision normals.
            if (playerSliding && slideTimer < 0f)
            {
                playerSliding = false;
                slideTimer = 0f;
            }

            

        }
        else
        {
            playerGrounded = false; 
            playerSliding = false;
            slideTimer = -1f;
            floorNormal = Vector3.up;
        }

        // //If the player is on a steep, adjust the movement direction.
        if (!Vector3Equal(floorNormal,Vector3.up))
        {
            movementDirection = Vector3.ProjectOnPlane(movementDirection, floorNormal);
        }


        // CapsuleCast in the direction of the movement, to avoid the player to stick on walls and avoid small terrain variations.
        if (!Vector3Equal(movementDirection, Vector3.zero))
        {
            
            capsulecastHitArray = CapsuleCastFromPlayer(movementDirection,maximumMovementSpeed*Time.fixedDeltaTime,environmentLayerMask.value);

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
                    // ...and another capsule collider hits the same object, or the normal is "pointing downwards"
                    RaycastHit hitInfo;
                    if (Physics.CapsuleCast(point1,point2+Vector3.up*stepMaxHeight,radius,movementDirection,out hitInfo,Mathf.Max(capsulecastHit.normal.y,stepMinDepth),environmentLayerMask.value) || capsulecastHit.normal.y < 0)
                    {
                        if (hitInfo.transform == null || capsulecastHit.collider.gameObject.Equals(hitInfo.collider.gameObject))
                        {
                            movementDirection -= Vector3.Project(movementDirection, Vector3.Scale(capsulecastHit.normal,new Vector3(1,0,1)).normalized);
                            break; 
  
                        }
                    }
                    else
                    {
                        playerRigidbody.MovePosition(playerRigidbody.position+Vector3.up*Mathf.Min(capsulecastHit.point.y - (point2.y -radius),stepMaxHeight));
                        break;
                    }

                }      
                else
                {
                    //If the gameObject in front isn't a slope or wall, just use its normal as floor normal and exit the loop.
                    floorNormal = capsulecastHit.normal;
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

        //If the player is on a slope, adjust the movement direction. If it is on a steep, it should fall.
        if (!Vector3Equal(floorNormal,Vector3.up))
        {
            playerRigidbody.velocity = Vector3.ProjectOnPlane(playerRigidbody.velocity, floorNormal);

            if (playerSliding)
            {  
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, Mathf.Min(playerRigidbody.velocity.y, -steepSlidingSpeed), playerRigidbody.velocity.z);
            }
            else
            {
                //If the user isn't giving any input, prevent the character from sliding.
                if (movementInput.magnitude < 0.1f)
                {
                    playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, Mathf.Max(playerRigidbody.velocity.y, Physics.gravity.magnitude*gravityScale*Time.fixedDeltaTime), playerRigidbody.velocity.z);
                }
                //Otherwiswe, clamp the velocity.
                else
                {
                    //playerRigidbody.velocity = Vector3.up*Mathf.Max(playerRigidbody.velocity.y, -Physics.gravity.magnitude*gravityScale*movementSpeed*Time.fixedDeltaTime) + Vector3.ClampMagnitude( new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z),movementSpeed);
                    playerRigidbody.velocity = Vector3.ClampMagnitude(playerRigidbody.velocity,maximumMovementSpeed);
                }
            }
        }
    	
       
        
        //Add gravity the player.
        playerRigidbody.AddForce(Physics.gravity*gravityScale,ForceMode.Acceleration);
    
        
    }

    void UpdatePlayerCapsulePosition()
    {
        point1 = playerRigidbody.position + playerCapsuleCollider.center + playerTransform.up *( playerCapsuleCollider.height / 2 -radius);
        point2 = playerRigidbody.position + playerCapsuleCollider.center - playerTransform.up *( playerCapsuleCollider.height / 2 -radius);
    }

    RaycastHit[] CapsuleCastFromPlayer(Vector3 direction, float distance, int layerMask)
    {
        return Physics.CapsuleCastAll(point1,point2, radius*radiusScale, direction, distance, layerMask);
    }

    public bool Vector3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001;
    }

}
