﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementScript : MonoBehaviour
{
    //Camera transform and Player transform
    private Transform m_CameraTranform;
    public Transform m_PlayerTransform;

    //Enemies target
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


    //Lock On Variables
    private Vector3 m_CameraSpeed;
    private Vector3 m_CameraLookSpeed;
    private Vector3 m_CameraTarget;

    //This is used to smooth interpolate between camera targets. (Time.deltaTime required)
    public float m_CameraLockSmooth;
    public float m_CameraUnlockSmooth;

    private bool m_EnemyJustUnlocked;
    private float m_TimeSinceLastUnlock;
    public float m_RecoveryTimeAfterUnlock;

    void Start ()
    {
        m_CameraTranform = transform;
        m_CameraDistanceDefaultVector = new Vector3(0f, 0f, m_CameraDistance);
        m_HorizontalRotation = 0f;
        m_VerticalRotation = 15f;

        m_EnemyJustUnlocked = false;
        m_TimeSinceLastUnlock = 0;


    }

    void Update()
    {
        m_HorizontalInput = Input.GetAxis("CameraHorizontal");
        m_VerticalInput = Input.GetAxis("CameraVertical");
    }

	void LateUpdate ()
    {
        UpdateLockOn();
        UpdateRotations();
        SetCameraDistanceVector();
        SetCameraPositionAndRotation();
    }

    void UpdateRotations()
    {
        if (!m_EnemyLocked)
        {
            m_HorizontalRotation = m_HorizontalInput * m_CameraRotationSpeed * Time.deltaTime;
            m_VerticalRotation = m_VerticalInput * m_CameraRotationSpeed * Time.deltaTime;
        }
        

    }

    void SetCameraDistanceVector()
    {

        m_CameraRay.direction = -m_CameraTranform.forward;
        m_CameraRay.origin = m_PlayerTransform.position;

        if (Physics.Raycast(m_CameraRay, out m_CameraRayCastHit, m_CameraDistance))
        {
            m_HitToPlayerDistance = Mathf.Abs(Vector3.Magnitude(m_CameraRayCastHit.point - m_PlayerTransform.position));
            
            if (m_EnemyLocked)
            {
                m_CameraDistanceVector = (m_PlayerTransform.position-GlobalData.LockedEnemyTransform.position).normalized * (m_HitToPlayerDistance)  ;
            }
            else
            {
                m_CameraDistanceVector = -Vector3.forward * (m_HitToPlayerDistance);
            }
        }
        else
        {    
            if (m_EnemyLocked)
            {
                m_CameraDistanceVector = (m_PlayerTransform.position-GlobalData.LockedEnemyTransform.position).normalized * m_CameraDistance;
            }
            else
            {
                m_CameraDistanceVector = -m_CameraDistanceDefaultVector;
            }
        }
    }

    void SetCameraPositionAndRotation()
    {

       if (m_EnemyLocked)
        {
            m_CameraTranform.position = Vector3.SmoothDamp(m_CameraTranform.position, m_PlayerTransform.position + m_CameraDistanceVector , ref m_CameraSpeed, m_CameraLockSmooth * Time.deltaTime);
            m_CameraTarget = Vector3.SmoothDamp(m_CameraTarget, GlobalData.LockedEnemyTransform.position, ref m_CameraLookSpeed, m_CameraLockSmooth * Time.deltaTime);
            m_CameraTranform.LookAt(m_CameraTarget);
        }
        else
        {
            if (((m_CameraTranform.position.y >= m_PlayerTransform.position.y + 2f) && m_VerticalRotation > 0) || ((m_CameraTranform.position.y <= m_PlayerTransform.position.y -1f) && m_VerticalRotation < 0))
            {
                m_CameraDistanceRotation = m_CameraTranform.rotation * Quaternion.Euler(0, m_HorizontalRotation, 0f);
            }
            else
            {
                m_CameraDistanceRotation = m_CameraTranform.rotation * Quaternion.Euler(m_VerticalRotation, m_HorizontalRotation, 0f);
            }

                
           
            if (m_EnemyJustUnlocked)
            {
                m_CameraTranform.position = Vector3.SmoothDamp(m_CameraTranform.position, m_PlayerTransform.position + m_CameraDistanceRotation * m_CameraDistanceVector, ref m_CameraSpeed, m_CameraUnlockSmooth * Time.deltaTime);
                m_CameraTarget = Vector3.Lerp(m_CameraTarget, m_PlayerTransform.position, m_TimeSinceLastUnlock /m_RecoveryTimeAfterUnlock);
                m_CameraTranform.LookAt(m_CameraTarget);
       

                m_TimeSinceLastUnlock += Time.deltaTime;
                if (m_TimeSinceLastUnlock >= m_RecoveryTimeAfterUnlock)
                {
                    m_EnemyJustUnlocked = false;
                }
            }
            else
            {
                m_CameraTranform.position = m_PlayerTransform.position+ (m_CameraDistanceRotation * m_CameraDistanceVector);
                m_CameraTarget = m_PlayerTransform.position;
                m_CameraTranform.LookAt(m_CameraTarget);
            }
                
        }
            
    }

    void UpdateLockOn()
    {
        if (m_EnemyLocked && !GlobalData.EnemyLocked)
        {
            m_EnemyJustUnlocked = true;
            m_TimeSinceLastUnlock = 0.0f;
        }
         m_EnemyLocked = GlobalData.EnemyLocked;

    }
}
