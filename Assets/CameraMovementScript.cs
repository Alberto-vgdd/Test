﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementScript : MonoBehaviour
{
    //Camera transform and Player transform
    private Transform m_CameraTranform;
    public Transform m_PlayerTransform;

    //Enemies target
    private PlayerAgroScript m_PlayerAgroScript;
    public Transform[] m_EnemiesTransform;
    private bool m_EnemyLocked;

    //The distance between the Target and the Camera.
    public float m_CameraDistance;
    public float m_CameraOffsetDistance; //This variable avoids wall clipping.
    private Vector3 m_CameraDistanceDefaultVector;

    //These two variables are used to calculate the camera "Spherical" position around the player.
    private Vector3 m_CameraDistanceVector;
    private Quaternion m_CameraDistanceRotation;

    //Inputs to control de camera
    private float m_HorizontalInput;
    private float m_VerticalInput;

    //The degrees per second the camera is going to turn.
    public float m_CameraRotationSpeed;

    //This is used to smooth Lerp between positions.
    public float m_CameraSmooth;

    //This values limit the Y axis of the camera
    public float m_MaxVerticalRotation;
    public float m_MinVerticalRotation;

    //Values that represents the amount of degrees in both Horizontal and Vertical axes
    private float  m_HorizontalRotation;
    private float m_VerticalRotation;


    //Raycast to shorten m_CameraToPlayerDistance in case of obstacle
    private RaycastHit m_CameraRayCastHit;
    private Ray m_CameraRay;
    private float m_HitToPlayerDistance;

    void Start ()
    {
        m_CameraTranform = transform;
        m_CameraDistanceDefaultVector = new Vector3(0f, 0f, m_CameraDistance);
        m_HorizontalRotation = 0f;
        m_VerticalRotation = 15f;

        m_PlayerAgroScript = m_PlayerTransform.GetComponent<PlayerAgroScript>();

    }

    void Update()
    {
        m_HorizontalInput = Input.GetAxis("CameraHorizontal");
        m_VerticalInput = Input.GetAxis("CameraVertical");
    }

	void LateUpdate ()
    {
        UpdateLockOnObjectives();

        UpdateRotations();
        SetCameraDistanceVector();
        SetCameraPositionAndRotation();
    }

    void UpdateRotations()
    {
        if (m_EnemyLocked)
        {
            Vector3 PlayerToEnemy = Vector3.Scale(m_EnemiesTransform[0].position - m_PlayerTransform.position, new Vector3(1f, 0f, 1f)).normalized;
            Vector3 PlayerToCamera = Vector3.Scale( m_CameraTranform.position - m_PlayerTransform.position, new Vector3(1f, 0f, 1f)).normalized;

            float angle= Vector3.Angle( PlayerToEnemy, PlayerToCamera);
            float sign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross( PlayerToEnemy, PlayerToCamera)));
           
           m_HorizontalRotation = ( (angle * sign) + 180) % 360;


        }
        else
        {
            m_HorizontalRotation += m_HorizontalInput * m_CameraRotationSpeed * Time.deltaTime;
        }


        m_VerticalRotation += m_VerticalInput * m_CameraRotationSpeed * Time.deltaTime;


        m_VerticalRotation = Mathf.Min(m_VerticalRotation, m_MaxVerticalRotation);
        m_VerticalRotation = Mathf.Max(m_VerticalRotation, m_MinVerticalRotation);

        if (Mathf.Abs(m_HorizontalRotation) >= 360)
        {
            m_HorizontalRotation = 0f;
        }

        
    }

    void SetCameraDistanceVector()
    {
        if (!m_EnemyLocked)
        {
            m_CameraRay.direction = -m_CameraTranform.forward;
            m_CameraRay.origin = m_PlayerTransform.position;

            if (Physics.Raycast(m_CameraRay, out m_CameraRayCastHit, m_CameraDistance))
            {
                m_HitToPlayerDistance = Mathf.Abs(Vector3.Magnitude(m_CameraRayCastHit.point - m_PlayerTransform.position));
                m_CameraDistanceVector = -Vector3.forward * (m_HitToPlayerDistance);
            }
            else
            {
                 m_CameraDistanceVector = -m_CameraDistanceDefaultVector;
            }
        }
    }

    void SetCameraPositionAndRotation()
    {

       

        m_CameraDistanceRotation = Quaternion.Euler(m_VerticalRotation, m_HorizontalRotation, 0f);
        m_CameraTranform.position = m_PlayerTransform.position + m_CameraDistanceRotation * m_CameraDistanceVector;

        if (m_EnemyLocked)
        {
            m_CameraTranform.LookAt(m_EnemiesTransform[0]);
        }
        else
        {
            m_CameraTranform.LookAt(m_PlayerTransform);
        }
           
        
    }

    void UpdateLockOnObjectives()
    {
        m_EnemiesTransform = m_PlayerAgroScript.m_NearbyEnemies;

        if (m_EnemiesTransform[0] != null )
        {
            m_EnemyLocked = true;
           
          
        }
        else
        {
            m_EnemyLocked = false;
        
        }
    }
}
