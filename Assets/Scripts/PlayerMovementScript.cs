using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    
    [Header("Camera Transform")]
    // Transform to calculate the direction of the movement.
    public Transform m_CameraTransform;

    // Input values.
    private Vector2 m_MovementInput;

    // Movement Axes for the player. M = V + H
    private Vector3 m_HorizontalDirection;
    private Vector3 m_VerticalDirection;
    private Vector3 m_MovementDirection;



    [Header("Movement Parameters")]
    // Values to create a smooth movement.
    public float m_MovementSpeed;
    public float m_TurnSmoothTime;
    private Vector3 m_PlayerCurrentVelocity;
    // Ref value used to smoothly turn around.
    private Vector3 m_TurnSpeed;


    // Variables used to check if the player is grounded/sliding.
    // Also used to avoid sticking to walls.
    public float m_PlayerToFloorOffset;
    public float m_SlopeAngle;
    private bool m_PlayerGrounded;
    private bool m_PlayerSliding;
    private RaycastHit[] m_RaycastHitArray;
    private Vector3 m_FloorNormal;

    public LayerMask m_EnvironmentLayerMask;
    Vector3 point1;
    Vector3 point2;
    float radius;


    // Player components
    private Rigidbody m_PlayerRigidbody;
    private CapsuleCollider m_PlayerCapsuleCollider;
    private Transform m_PlayerTransform;

    [Header("Player Animator")]
    public Animator m_PlayerAnimator;
    // Variables for the Player Animation
    private bool m_PlayerWalking;
    private bool m_PlayerRunning;


    // Use this for initialization
    void Awake ()
    {
        SystemAndData.PlayerTransform = m_PlayerTransform = transform;
        m_PlayerRigidbody = m_PlayerTransform.GetComponent<Rigidbody>();
        m_PlayerCapsuleCollider = m_PlayerTransform.GetComponent<CapsuleCollider>();
        radius = m_PlayerCapsuleCollider.radius;
    }
	
	// Update is called once per frame
	void Update ()
    {
        
        // Update movement input and normalize the vector to avoid diagonal acceleration.
        m_MovementInput = new Vector2(SystemAndData.GetHorizontalInput(),SystemAndData.GetVerticalInput()) ;

        // m_MovementInput.magnitude?
        if (Mathf.Abs(m_MovementInput.x)+Mathf.Abs(m_MovementInput.y) > 1 )
        {
            m_MovementInput.Normalize();
        }

        // Update movement directions 
        if (!SystemAndData.IsEnemyLocked)
        {
            m_HorizontalDirection = Vector3.Scale(m_CameraTransform.right, new Vector3(1f, 0f, 1f)).normalized;
            m_VerticalDirection = Vector3.Scale(m_CameraTransform.forward, new Vector3(1f, 0f, 1f)).normalized;
        }
        else
        {

            m_VerticalDirection = Vector3.Scale(SystemAndData.LockedEnemyTransform.position - m_PlayerTransform.position, new Vector3(1f, 0f, 1f)).normalized;
            m_HorizontalDirection = Vector3.Cross(m_VerticalDirection, -m_PlayerTransform.up).normalized;
        }

        m_MovementDirection = m_HorizontalDirection * m_MovementInput.x + m_VerticalDirection * m_MovementInput.y;

       
    
    }

    void FixedUpdate()
    {
        // This is used to update variables for the capsule casts.
        UpdatePlayerCapsulePosition();

        // Asume that the player is not grounded/sliding at the start of the loop
        m_PlayerGrounded = false; 
        m_PlayerSliding = false;
        m_FloorNormal = Vector3.up;

        // CapsuleCast Below the player to determine the grounded/sliding state
        m_RaycastHitArray = CapsuleCastFromPlayer(0.95f,Vector3.down,m_PlayerToFloorOffset,m_EnvironmentLayerMask.value);
        
        // If the player is grounded asume the player is sliding too
        if (m_RaycastHitArray.Length > 0 ) 
        {
            m_PlayerGrounded = true; 
            m_PlayerSliding = true;
        }

        // Check if the player is not sliding from the closest plane to the furthest.
        foreach(RaycastHit hit in m_RaycastHitArray)
        {
            m_FloorNormal = hit.normal;

            if (Vector3.Angle(hit.normal, Vector3.up) < m_SlopeAngle)
            {
                m_PlayerSliding = false;
                break;
            }
        }
        

        if (m_MovementDirection != Vector3.zero)
        {
            // CapsuleCast in the direction of the movement to avoid the player to stick on walls.
            m_RaycastHitArray = CapsuleCastFromPlayer(0.95f,m_MovementDirection,m_MovementSpeed*Time.fixedDeltaTime,m_EnvironmentLayerMask.value);
            
            foreach(RaycastHit hit in m_RaycastHitArray)
            {
                if (Vector3.Angle(hit.normal, Vector3.up) > m_SlopeAngle)
                {
                    m_MovementDirection -= Vector3.Project(m_MovementDirection, Vector3.Scale(hit.normal,new Vector3(1,0,1)).normalized);
                }
            }
        }
        
        //Move the player
        m_PlayerRigidbody.velocity = m_MovementDirection * m_MovementSpeed +  Vector3.up*m_PlayerRigidbody.velocity.y;

        //If the player is on a steep, adjust the movement direction. If it is on a slope, it should fall.
        if (m_FloorNormal != Vector3.up)
        {
            m_PlayerRigidbody.velocity = Vector3.ProjectOnPlane(m_PlayerRigidbody.velocity, m_FloorNormal);

            if (m_PlayerSliding)
            {  
                m_PlayerRigidbody.velocity = new Vector3(m_PlayerRigidbody.velocity.x, Mathf.Min(m_PlayerRigidbody.velocity.y, -m_MovementSpeed), m_PlayerRigidbody.velocity.z);
            }  
        }
    	
        //Rotate the player.
        if (m_PlayerRigidbody.velocity.magnitude > 0f)
        {
            if (!SystemAndData.IsEnemyLocked)
            {
                m_PlayerTransform.forward = Vector3.SmoothDamp(m_PlayerTransform.forward, Vector3.Scale(m_PlayerRigidbody.velocity,new Vector3(1,0,1)) , ref m_TurnSpeed, m_TurnSmoothTime);
            }
            else
            {
                m_PlayerTransform.forward = Vector3.SmoothDamp(m_PlayerTransform.forward, m_VerticalDirection, ref m_TurnSpeed, m_TurnSmoothTime);
            }
            
        }

        //Add gravity the player.
        m_PlayerRigidbody.AddForce(Physics.gravity*2f,ForceMode.Acceleration);


        // ANIMATION TEST
        m_PlayerWalking = (m_MovementInput.magnitude != 0) ? true : false;
        m_PlayerRunning = (m_MovementInput.magnitude > 0.55f) ? true : false;
        m_PlayerAnimator.SetBool("Walk", m_PlayerWalking);
        m_PlayerAnimator.SetBool("Run", m_PlayerRunning);
        m_PlayerAnimator.SetBool("Fall", !m_PlayerGrounded);
        m_PlayerAnimator.SetBool("Slide", m_PlayerSliding);

    }

    void UpdatePlayerCapsulePosition()
    {
        point1 = m_PlayerTransform.position + m_PlayerCapsuleCollider.center + m_PlayerTransform.up *( m_PlayerCapsuleCollider.height / 2 -radius);
        point2 = m_PlayerTransform.position + m_PlayerCapsuleCollider.center - m_PlayerTransform.up *( m_PlayerCapsuleCollider.height / 2 -radius);
    }

    RaycastHit[] CapsuleCastFromPlayer(float radiusScale,Vector3 direction, float distance, int layerMask)
    {
        return Physics.CapsuleCastAll(point1,point2, radius*radiusScale, direction, distance, layerMask);
    }
}
