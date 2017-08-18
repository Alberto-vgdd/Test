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
        }
        else
        {

            m_VerticalDirection = Vector3.Scale(GlobalData.LockedEnemyTransform.position - m_Target.position, new Vector3(1f, 0f, 1f)).normalized;
            m_HorizontalDirection = Vector3.Cross(m_VerticalDirection, -m_Target.up).normalized;
        }

        m_MovementDirection = m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput;
    }

    void FixedUpdate()
    {

        //Check if the player is grounded/sliding.
        if (Physics.CapsuleCast(m_Target.position + m_TargetCapsuleCollider.center + Vector3.up *( m_TargetCapsuleCollider.height / 2 -m_TargetCapsuleCollider.radius), m_Target.position + m_TargetCapsuleCollider.center - Vector3.up *( m_TargetCapsuleCollider.height / 2 -m_TargetCapsuleCollider.radius), m_TargetCapsuleCollider.radius*0.95f, -m_Target.up, out m_TargetToGroundRaycastHit, 0.25f, (1 << LayerMask.NameToLayer("Environment"))))
        {
            m_TargetGrounded = true;
            m_TargetPlaneDirection = m_TargetToGroundRaycastHit.normal;

            if (Vector3.Angle(m_TargetPlaneDirection, Vector3.up) > 35f)
            {
                m_TargetSliding = true;
            }
            else
            {
                m_TargetSliding = false;
            }

            //This capsule cast is used to avoid the player to walk into slopes and start jittering when it is grounded.
            if (Physics.CapsuleCast(m_Target.position + m_TargetCapsuleCollider.center + Vector3.up *( m_TargetCapsuleCollider.height / 2 -m_TargetCapsuleCollider.radius), m_Target.position + m_TargetCapsuleCollider.center - Vector3.up *( m_TargetCapsuleCollider.height / 2 -m_TargetCapsuleCollider.radius), m_TargetCapsuleCollider.radius*0.95f, m_MovementDirection, out m_TargetToGroundRaycastHit, m_MovementSpeed*Time.fixedDeltaTime, (1 << LayerMask.NameToLayer("Environment"))))
            {
                if (Vector3.Angle(m_TargetToGroundRaycastHit.normal, Vector3.up) > 35f)
                {
                    m_MovementDirection += Vector3.Scale(m_MovementDirection,Vector3.Scale(m_TargetToGroundRaycastHit.normal,new Vector3(1f,0f,1f))) * Mathf.Sign(Vector3.Dot(m_Target.forward,Vector3.forward));
                }
            }
        }
        else
        {
            m_TargetGrounded = false;
            m_TargetSliding = false;
            m_TargetPlaneDirection = Vector3.up;
        }   


        //Move the player
        m_TargetRigidbody.velocity = m_MovementDirection * m_MovementSpeed +  Vector3.up*m_TargetRigidbody.velocity.y;

        //If the player is on a steep, adjust the movement direction. If it is on a slope, it should fall.
        if (m_TargetPlaneDirection != Vector3.up)
        {
            m_TargetRigidbody.velocity = Vector3.ProjectOnPlane(m_TargetRigidbody.velocity, m_TargetPlaneDirection);

            if (m_TargetSliding)
            {
                m_TargetRigidbody.velocity = new Vector3(m_TargetRigidbody.velocity.x, Mathf.Min(m_TargetRigidbody.velocity.y, -m_MovementSpeed), m_TargetRigidbody.velocity.z);
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

        //Add gravity the player.
        m_TargetRigidbody.AddForce(Physics.gravity*2f,ForceMode.Acceleration);

    }
}
