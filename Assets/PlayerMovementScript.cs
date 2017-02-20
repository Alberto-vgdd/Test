using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{

    public Transform m_Camera;
    public Transform m_Target;

    public float m_HorizontalInput;
    public float m_VerticalInput;

    public LineRenderer m_HorizontalLine;
    public LineRenderer m_VerticalLine;
    public float m_LineLength;

    public Vector3 m_HorizontalDirection;
    public Vector3 m_VerticalDirection;
    

	// Use this for initialization
	void Start ()
    {
        m_HorizontalLine.SetPosition(0, m_Target.position);
        m_VerticalLine.SetPosition(0, m_Target.position);
    }
	
	// Update is called once per frame
	void Update ()
    {
        m_HorizontalInput = Input.GetAxis("Horizontal");
        m_VerticalInput = Input.GetAxis("Vertical");

        m_HorizontalDirection = Vector3.Scale(m_Camera.TransformVector(Vector3.right), new Vector3(1f, 0f, 1f)).normalized;
        m_VerticalDirection = Vector3.Scale(m_Camera.TransformVector(Vector3.forward), new Vector3(1f, 0f, 1f)).normalized;

        m_Target.Translate((m_HorizontalDirection* m_HorizontalInput + m_VerticalDirection* m_VerticalInput  ).normalized* 10f* Time.deltaTime);
        m_HorizontalLine.SetPosition(1, m_HorizontalDirection * m_HorizontalInput * m_LineLength);
        m_VerticalLine.SetPosition(1, m_VerticalDirection * m_VerticalInput * m_LineLength);

    }
}
