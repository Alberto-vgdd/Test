using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    // Transforms to calculate the direction of the movement.
    [Header("Camera Transform")]
    public Transform m_CameraTransform;
    private Transform m_PlayerTransform;

    // Input values.
    private Vector2 m_MovementInput;


    // Movement Axes for the player. M = V + H
    private Vector3 m_HorizontalDirection;
    private Vector3 m_VerticalDirection;
    private Vector3 m_MovementDirection;

    // Ref value used to smoothly turn around.
    private Vector3 m_TurnSpeed;

    [Header("Movement Parameters")]
    // Values to create a smooth movement.
    public float m_MovementSpeed;
    public float m_TurnSmooth;
    private Rigidbody m_PlayerRigidbody;
    private Vector3 m_PlayerCurrentVelocity;

    // A reference to the player's collider
    private CapsuleCollider m_PlayerCapsuleCollider;

    // Variables used to check if the player is grounded.
    private RaycastHit m_PlayerToGroundRaycastHit;
    public float m_PlayerToFloorOffset;
    private bool m_PlayerGrounded;
    private bool m_PlayerSliding;

    // Variables to manage the jump direction
    private Vector3 m_TargetPlaneDirection;



    // Use this for initialization
    void Awake ()
    {
        SystemAndData.PlayerTransform = m_PlayerTransform = transform;
        m_PlayerRigidbody = m_PlayerTransform.GetComponent<Rigidbody>();
        m_PlayerCapsuleCollider = m_PlayerTransform.GetComponent<CapsuleCollider>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        
        // Update movement input and normalize the  vector to avoid diagonal acceleration.
        m_MovementInput = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")) ;

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
        //                                         <---Height-->    
        //Check if the player is grounded/sliding. ( * ===== * ) 
        if (Physics.CapsuleCast(m_PlayerTransform.position + m_PlayerCapsuleCollider.center + m_PlayerTransform.up *( m_PlayerCapsuleCollider.height / 2 -m_PlayerCapsuleCollider.radius), m_PlayerTransform.position + m_PlayerCapsuleCollider.center - m_PlayerTransform.up *( m_PlayerCapsuleCollider.height / 2 -m_PlayerCapsuleCollider.radius), m_PlayerCapsuleCollider.radius*0.95f, -m_PlayerTransform.up, out m_PlayerToGroundRaycastHit, 0.25f, (1 << LayerMask.NameToLayer("Environment"))))
        {
            m_PlayerGrounded = true;
            m_TargetPlaneDirection = m_PlayerToGroundRaycastHit.normal;

            if (Vector3.Angle(m_TargetPlaneDirection, Vector3.up) > 35f)
            {
                m_PlayerSliding = true;
            }
            else
            {
                m_PlayerSliding = false;
            }

            //This capsule cast is used to avoid the player to walk into slopes and start jittering when it is grounded.
            if (Physics.CapsuleCast(m_PlayerTransform.position + m_PlayerCapsuleCollider.center + m_PlayerTransform.up *( m_PlayerCapsuleCollider.height / 2 -m_PlayerCapsuleCollider.radius), m_PlayerTransform.position + m_PlayerCapsuleCollider.center - m_PlayerTransform.up *( m_PlayerCapsuleCollider.height / 2 -m_PlayerCapsuleCollider.radius), m_PlayerCapsuleCollider.radius*0.95f, m_MovementDirection, out m_PlayerToGroundRaycastHit, m_MovementSpeed*Time.fixedDeltaTime, (1 << LayerMask.NameToLayer("Environment"))))
            {
                if (Vector3.Angle(m_PlayerToGroundRaycastHit.normal, Vector3.up) > 35f)
                {
                    m_MovementDirection += Vector3.Scale(m_MovementDirection,Vector3.Scale(m_PlayerToGroundRaycastHit.normal,new Vector3(1f,0f,1f))) * Mathf.Sign(Vector3.Dot(m_MovementDirection,Vector3.forward));
                }
            }
        }
        else
        {
            m_PlayerGrounded = false;
            m_PlayerSliding = false;
            m_TargetPlaneDirection = Vector3.up;
        }   


        //Move the player
        m_PlayerRigidbody.velocity = m_MovementDirection * m_MovementSpeed +  Vector3.up*m_PlayerRigidbody.velocity.y;

        //If the player is on a steep, adjust the movement direction. If it is on a slope, it should fall.
        if (m_TargetPlaneDirection != Vector3.up)
        {
            m_PlayerRigidbody.velocity = Vector3.ProjectOnPlane(m_PlayerRigidbody.velocity, m_TargetPlaneDirection);

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
                m_PlayerTransform.forward = Vector3.SmoothDamp(m_PlayerTransform.forward, Vector3.Scale(m_PlayerRigidbody.velocity,new Vector3(1,0,1)) , ref m_TurnSpeed, m_TurnSmooth);
            }
            else
            {
                m_PlayerTransform.forward = Vector3.SmoothDamp(m_PlayerTransform.forward, m_VerticalDirection, ref m_TurnSpeed, m_TurnSmooth);
            }
            
        }

        //Add gravity the player.
        m_PlayerRigidbody.AddForce(Physics.gravity*2f,ForceMode.Acceleration);

    }
}
