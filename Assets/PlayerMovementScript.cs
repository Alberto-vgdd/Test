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

    //ref value which is used to smoothly turn around.
    private Vector3 m_TurnSpeed;

    //values to create a smooth turning.
    public float m_MovementSpeed;
    public float m_TurnSmooth;


    //-----------------------------------------------
    //TEMPORARY ENEMY TARGET
    private bool m_EnemyLocked;
    //-----------------------------------------------


    // Use this for initialization
    void Start ()
    {
        m_Camera = Camera.main.transform;
        m_Target = GameObject.Find("Player").transform;

    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateLockOnObjectives();

        m_HorizontalInput = Input.GetAxis("Horizontal");
        m_VerticalInput = Input.GetAxis("Vertical");


            if (!m_EnemyLocked)
            {
                m_HorizontalDirection = Vector3.Scale(m_Camera.TransformVector(Vector3.right), new Vector3(1f, 0f, 1f)).normalized;
                m_VerticalDirection = Vector3.Scale(m_Camera.TransformVector(Vector3.forward), new Vector3(1f, 0f, 1f)).normalized;
                m_Target.transform.forward = Vector3.SmoothDamp(m_Target.transform.forward, (m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput).normalized, ref m_TurnSpeed, m_TurnSmooth);
   
            }
            else
            {

                m_VerticalDirection = Vector3.Scale(GlobalData.LockableEnemies.First.Value.position - m_Target.position, new Vector3(1f, 0f, 1f)).normalized;
                m_HorizontalDirection = Vector3.Cross(m_VerticalDirection, new Vector3(0f, -1f, 0f)).normalized;
                m_Target.transform.forward = Vector3.SmoothDamp(m_Target.transform.forward, m_VerticalDirection, ref m_TurnSpeed, m_TurnSmooth);

            }

            m_Target.Translate(m_Target.InverseTransformVector(m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput) * m_MovementSpeed * Time.deltaTime);
    }





    void UpdateLockOnObjectives()
    {
        m_EnemyLocked = GlobalData.EnemyLocked;
    }
}
