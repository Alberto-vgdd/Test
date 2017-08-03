using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    //Transforms to calculate the direction of the movement.
    private Transform m_Camera;
    private Transform m_Target;

    //Input values.
    private float m_HorizontalInput;
    private float m_VerticalInput;

    //Movement Axes for the player.
    private Vector3 m_HorizontalDirection;
    private Vector3 m_VerticalDirection;
    private Vector3 m_MovementDirection;

    //ref value which is used to smoothly turn around.
    private Vector3 m_TurnSpeed;

    //values to create a smooth movement.
    public float m_MovementSpeed;
    public float m_TurnSmooth;
    private Rigidbody m_TargetRigidbody;
    private Vector3 m_TargetCurrentVelocity;

    // A reference to the player's collider
    private CapsuleCollider m_TargetCapsuleCollider;

    // Variables used to check if the player is grounded.
    private Ray m_TargetToGroundRay1;
    private Ray m_TargetToGroundRay2;
    private Ray m_TargetToGroundRay3;
    private Ray m_TargetToGroundRay4;
    private RaycastHit m_TargetToGroundRaycastHit;
    public float m_TargetToFloorOffset;
    public bool m_TargetGrounded;
    public bool m_TargetSliding;

    // Variables to manage the jump direction
    private Vector3 m_TargetPlaneDirection;



    // Use this for initialization
    void Start ()
    {
        m_Camera = Camera.main.transform;
        m_Target = GameObject.Find("Player").transform;
        GlobalData.PlayerTransform = m_Target;
        m_TargetRigidbody = m_Target.GetComponent<Rigidbody>();
        m_TargetCapsuleCollider = m_Target.GetComponent<CapsuleCollider>();

    }
	
	// Update is called once per frame
	void Update ()
    {

        m_HorizontalInput = Input.GetAxis("Horizontal");
        m_VerticalInput = Input.GetAxis("Vertical");


        if (!GlobalData.EnemyLocked)
        {
            m_HorizontalDirection = Vector3.Scale(m_Camera.right, new Vector3(1f, 0f, 1f)).normalized;
            m_VerticalDirection = Vector3.Scale(m_Camera.forward, new Vector3(1f, 0f, 1f)).normalized;
            m_MovementDirection = m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput;
        }
        else
        {

            m_VerticalDirection = Vector3.Scale(GlobalData.LockedEnemyTransform.position - m_Target.position, new Vector3(1f, 0f, 1f)).normalized;
            m_HorizontalDirection = Vector3.Cross(m_VerticalDirection, -m_Target.up).normalized;
            m_MovementDirection = m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput;
        }


    }

    void FixedUpdate()
    {
        // Check if the player is grounded
        if (Physics.CheckCapsule(m_TargetCapsuleCollider.bounds.center, new Vector3(m_TargetCapsuleCollider.bounds.center.x, m_TargetCapsuleCollider.bounds.min.y - m_TargetToFloorOffset, m_TargetCapsuleCollider.bounds.center.z), m_TargetCapsuleCollider.radius, (1 << LayerMask.NameToLayer("Environment"))))
        {
            m_TargetGrounded = true; 
        }
        else
        {
            m_TargetGrounded = false;
        }
            
        //Check if the player is sliding.
        if (m_TargetGrounded)
        {
            m_TargetToGroundRay1.origin = m_Target.position + m_TargetCapsuleCollider.center + m_Target.forward*m_TargetCapsuleCollider.radius/2;
            m_TargetToGroundRay2.origin = m_Target.position + m_TargetCapsuleCollider.center - m_Target.forward*m_TargetCapsuleCollider.radius/2;
            m_TargetToGroundRay3.origin = m_Target.position + m_TargetCapsuleCollider.center + m_Target.right*m_TargetCapsuleCollider.radius/2;
            m_TargetToGroundRay4.origin = m_Target.position + m_TargetCapsuleCollider.center - m_Target.right*m_TargetCapsuleCollider.radius/2;
            m_TargetToGroundRay1.direction = m_TargetToGroundRay2.direction = m_TargetToGroundRay3.direction = m_TargetToGroundRay4.direction = -Vector3.up;

            if (Physics.Raycast(m_TargetToGroundRay1, out m_TargetToGroundRaycastHit, m_TargetCapsuleCollider.bounds.size.y/2 + m_TargetToFloorOffset,  (1 << LayerMask.NameToLayer("Environment"))) || Physics.Raycast(m_TargetToGroundRay2, out m_TargetToGroundRaycastHit, m_TargetCapsuleCollider.bounds.size.y/2 + m_TargetToFloorOffset,  (1 << LayerMask.NameToLayer("Environment"))) || Physics.Raycast(m_TargetToGroundRay3, out m_TargetToGroundRaycastHit, m_TargetCapsuleCollider.bounds.size.y/2 + m_TargetToFloorOffset,  (1 << LayerMask.NameToLayer("Environment"))) || Physics.Raycast(m_TargetToGroundRay4, out m_TargetToGroundRaycastHit, m_TargetCapsuleCollider.bounds.size.y/2 + m_TargetToFloorOffset,  (1 << LayerMask.NameToLayer("Environment"))))
            {
                m_TargetPlaneDirection = m_TargetToGroundRaycastHit.normal;

                if (Vector3.Angle(m_TargetPlaneDirection, Vector3.up) > 35f)
                {
                    m_TargetSliding = true;
                }
                else
                {
                    m_TargetSliding = false;
                }

                Debug.Log(Vector3.Angle(m_TargetPlaneDirection, Vector3.up));
                Debug.DrawRay(m_TargetToGroundRay1.origin, m_TargetToGroundRay1.direction);
                Debug.DrawRay(m_TargetToGroundRay2.origin, m_TargetToGroundRay1.direction);
                Debug.DrawRay(m_TargetToGroundRay3.origin, m_TargetToGroundRay1.direction);
                Debug.DrawRay(m_TargetToGroundRay4.origin, m_TargetToGroundRay1.direction);
            }

        }
        else
        {
            m_TargetSliding = false;
            m_TargetPlaneDirection = Vector3.up;
        }


        //Move the player
        m_TargetRigidbody.velocity = m_MovementDirection * m_MovementSpeed + new Vector3(0f, m_TargetRigidbody.velocity.y -9.81f*Time.fixedDeltaTime, 0f);

        if ( m_TargetPlaneDirection != Vector3.up)
        {
            m_TargetRigidbody.velocity = Vector3.ProjectOnPlane(m_TargetRigidbody.velocity, m_TargetPlaneDirection);

            if(m_TargetSliding)
            {
                m_TargetRigidbody.velocity = new Vector3(m_TargetRigidbody.velocity.x,Mathf.Min(m_TargetRigidbody.velocity.y,-1),m_TargetRigidbody.velocity.z);
            }
        }


        //Rotate the player.
        if (m_TargetRigidbody.velocity.magnitude > 0f)
        {
            if (!GlobalData.EnemyLocked)
            {
                m_Target.forward = Vector3.SmoothDamp(m_Target.transform.forward, m_MovementDirection.normalized, ref m_TurnSpeed, m_TurnSmooth * Time.fixedDeltaTime);
            }
            else
            {
                m_Target.forward = Vector3.SmoothDamp(m_Target.transform.forward, m_VerticalDirection, ref m_TurnSpeed, m_TurnSmooth * Time.fixedDeltaTime);
            }
        }

       
    }
}
