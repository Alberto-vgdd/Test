using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    // Transform to calculate the direction of the movement.
    [Header("Camera Transform")]
    public Transform cameraTransform;

    // Input values.
    // Movement Axes for the player. M = V + H
    private Vector2 movementInput;
    private Vector3 movementDirection;
    private Vector3 horizontalDirection;
    private Vector3 verticalDirection;



    // Values to create a smooth movement.
    [Header("Movement Parameters")]
    public float movementSpeed;
    public float turnSmoothTime;
    public float gravityScale;
    private Vector3 currentVelocity;
    private Vector3 turnSpeed;


    // Variables used to check if the player is grounded/sliding.
    // Also used to avoid sticking to walls.
    public float slopeAngle;
    public float stepOffset;
    private bool playerGrounded;
    private bool playerSliding;
    private RaycastHit[] raycastHitArray;
    private Vector3 floorNormal;

    // CapsuleCast variables
    public LayerMask environmentLayerMask;
    Vector3 point1;
    Vector3 point2;
    Vector3 feetPoint;
    float radius;
    float radiusScale = 0.95f;


    // Player components
    // Variables for the Player Animation
    [Header("Player Animator")]
    public Animator playerAnimator;
    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCapsuleCollider;
    private Transform playerTransform;





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
                    if ( movementInput.magnitude > 0.1f)
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

        // Animations
        playerAnimator.SetBool("Fall", !playerGrounded);
        playerAnimator.SetBool("Slide", playerSliding);
        playerAnimator.SetFloat("Walk Speed",movementInput.magnitude );  

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
        raycastHitArray = CapsuleCastFromPlayer(Vector3.down, Mathf.Abs(Mathf.Min(playerRigidbody.velocity.y*Time.fixedDeltaTime,-stepOffset)),environmentLayerMask.value);


        // If the player is grounded asume the player is sliding too
        if (raycastHitArray.Length > 0 ) 
        {
            playerGrounded = true; 
            playerSliding = true;

            // Check if the player is not sliding from the closest plane to the furthest.
            foreach(RaycastHit hit in raycastHitArray)
            {

                // Using hit.normal returns the COLLISION'S NORMAL. (To avoid problems when falling I've changed the animator)
                floorNormal = hit.normal;

                // Check if the ground's surface isn't a slope and the normal isn't "pointing downwards"
                if (Vector3.Angle(hit.normal, Vector3.up) <= slopeAngle && hit.normal.y > 0 )
                {
                    playerSliding = false;
                    break;
                }

                
            }

        }

        // //If the player is on a steep, adjust the movement direction.
        if (!Vector3Equal(floorNormal,Vector3.up))
        {
            movementDirection = Vector3.ProjectOnPlane(movementDirection, floorNormal);
        }


        // CapsuleCast in the direction of the movement, to avoid the player to stick on walls and avoid small terrain variations.
        if (!Vector3Equal(movementDirection, Vector3.zero))
        {
            
            raycastHitArray = CapsuleCastFromPlayer(movementDirection,movementDirection.magnitude*movementSpeed*Time.fixedDeltaTime,environmentLayerMask.value);

            // This value is used to keep the movementSpeed after constraining the direction.
            float oldMovementMagnitude = movementDirection.magnitude;


            for (int i = 0; i < raycastHitArray.Length; i++)
            {
                //For colliders that overlap the capsule at the start of the sweep, to avoid problems.
                if (Vector3Equal(Vector3.zero,raycastHitArray[i].point))
                {
                    continue;
                }


                // If the angle is correct...
                if ( Vector3.Angle(raycastHitArray[i].normal, Vector3.up) > slopeAngle )
                {
                    // ...and another capsule collider hits the same object, or the normal is "pointing downwards"
                    RaycastHit hitInfo;
                    if (Physics.CapsuleCast(point1,point2+Vector3.up*stepOffset,radius,movementDirection,out hitInfo,1f,environmentLayerMask.value) || raycastHitArray[i].normal.y < 0)
                    {
                        if (raycastHitArray[i].collider.Equals(hitInfo.collider) || raycastHitArray[i].collider == null)
                        {
                            movementDirection -= Vector3.Project(movementDirection, Vector3.Scale(raycastHitArray[i].normal,new Vector3(1,0,1)).normalized);
                            break; 
  
                        }
                    }
                    else
                    {
                        playerRigidbody.MovePosition(playerRigidbody.position+Vector3.up*(raycastHitArray[i].point.y - (point2.y -radius)));
                        break;
                        
                        
                    }

                }      
                else
                {
                    //If the gameObject in front isn't a slope or wall, just use its normal as floor normal and exit the loop.
                    floorNormal = raycastHitArray[i].normal;
                    continue;

                }  
                
            }

            
            // If the new movementDirection isn't 0, scale the movementDirection vector.
            if (movementDirection.magnitude >= 0.001f)
            {
                movementDirection *= oldMovementMagnitude/movementDirection.magnitude;
            }
        }

        


        //Move the player
        playerRigidbody.velocity = movementDirection * movementSpeed +  Vector3.up*playerRigidbody.velocity.y;

        //If the player is on a steep, adjust the movement direction. If it is on a slope, it should fall.
        if (!Vector3Equal(floorNormal,Vector3.up))
        {
            playerRigidbody.velocity = Vector3.ProjectOnPlane(playerRigidbody.velocity, floorNormal);

            if (playerSliding)
            {  
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, Mathf.Min(playerRigidbody.velocity.y, -movementSpeed*2f), playerRigidbody.velocity.z);
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
                    playerRigidbody.velocity = Vector3.ClampMagnitude(playerRigidbody.velocity,movementSpeed);
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
