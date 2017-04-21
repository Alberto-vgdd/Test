using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{

    public Transform m_Camera;
    public Transform m_Target;

    public float m_HorizontalInput;
    public float m_VerticalInput;

    public Vector3 m_HorizontalDirection;
    public Vector3 m_VerticalDirection;

    private Vector3 m_TurnSpeed;
    public float m_MovementSpeed;
    public float m_TurnSmooth;



    // Use this for initialization
    void Start ()
    {
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_HorizontalInput = Input.GetAxis("Horizontal");
        m_VerticalInput = Input.GetAxis("Vertical");

        if (m_HorizontalInput != 0 || m_VerticalInput != 0)
        {
            m_HorizontalDirection = Vector3.Scale(m_Camera.TransformVector(Vector3.right), new Vector3(1f, 0f, 1f)).normalized;
            m_VerticalDirection = Vector3.Scale(m_Camera.TransformVector(Vector3.forward), new Vector3(1f, 0f, 1f)).normalized;


            //m_Target.transform.forward = (m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput).normalized;
            m_Target.transform.forward = Vector3.SmoothDamp(m_Target.transform.forward, (m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput).normalized, ref m_TurnSpeed, m_TurnSmooth);
            m_Target.Translate(m_Target.InverseTransformVector(m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput).normalized * m_MovementSpeed * Time.deltaTime);
        }
    }
}
