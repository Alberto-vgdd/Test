﻿using System.Collections;
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



    // Use this for initialization
    void Start ()
    {
        m_Camera = Camera.main.transform;
        m_Target = GameObject.Find("Player").transform;
        GlobalData.PlayerTransform = m_Target;
        m_TargetRigidbody = m_Target.GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void Update ()
    {

        m_HorizontalInput = Input.GetAxis("Horizontal");
        m_VerticalInput = Input.GetAxis("Vertical");


        if (!GlobalData.EnemyLocked)
        {
            m_HorizontalDirection = Vector3.Scale(m_Camera.TransformVector(Vector3.right), new Vector3(1f, 0f, 1f)).normalized;
            m_VerticalDirection = Vector3.Scale(m_Camera.TransformVector(Vector3.forward), new Vector3(1f, 0f, 1f)).normalized;
            m_MovementDirection = m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput;
            m_Target.transform.forward = Vector3.SmoothDamp(m_Target.transform.forward, m_MovementDirection.normalized, ref m_TurnSpeed, m_TurnSmooth*Time.deltaTime);
        }
        else
        {

            m_VerticalDirection = Vector3.Scale(GlobalData.LockedEnemyTransform.position - m_Target.position, new Vector3(1f, 0f, 1f)).normalized;
            m_HorizontalDirection = Vector3.Cross(m_VerticalDirection, new Vector3(0f, -1f, 0f)).normalized;
            m_MovementDirection = m_HorizontalDirection * m_HorizontalInput + m_VerticalDirection * m_VerticalInput;
            m_Target.transform.forward = Vector3.SmoothDamp(m_Target.transform.forward, m_VerticalDirection, ref m_TurnSpeed, m_TurnSmooth*Time.deltaTime);

        }


    }

    void FixedUpdate()
    {

        //m_Target.Translate(m_MovementDirection * m_MovementSpeed * Time.fixedDeltaTime, Space.World);
        m_TargetRigidbody.MovePosition(m_Target.position + m_MovementDirection * m_MovementSpeed * Time.fixedDeltaTime);

    }
       

}
