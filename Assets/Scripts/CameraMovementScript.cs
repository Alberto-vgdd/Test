using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementScript : MonoBehaviour 
{
	[Header("Camera Transform")]
	public Transform m_CameraTransform;

	[Header("Camera Target")]
	public Transform m_PlayerTransform;

	[Header("Camera Parameters")]
	public float m_CameraDistance;
	public float m_CameraHeight;
	public float m_MaximunmVerticalAngle;
	public float m_MinimumVerticalAngle;

	[Header("Camera Movement Parameters")]
	public float m_CameraFollowSpeedMultiplier;
	public float m_CameraSmoothMultiplier;
	public float m_CameraSpeedMultiplier;
	public float m_TargetCameraSpeedMultipier;
	public bool m_InvertHorizontalInput;
	public bool m_InvertVerticalInput;

	[Header("Camera Auto Rotation Parameters")]
	public float m_CameraAutoRotateMultiplier; //2 is slow, 3 and 4 seem fine
	public bool m_CameraAutoRotation;
	private Camera m_Camera;
	private float m_PlayerXInViewport;

	[Header("Clipping Parameters")]
	public float m_CameraClippingOffset;


	// Transforms to manage the camera rotations. (Divided in 2 different axes.)
	private Transform m_CameraHorizontalPivot;
	private Transform m_CameraVerticalPivot;

	// Controller Inputs
	private float m_HorizontalInput;
	private float m_VerticalInput;

	// Values used to smoothly rotate the camera.
	private float m_HorizontalIncrement;
	private float m_VerticalIncrement;
	private float m_HorizontalSmoothVelocity;
	private float m_VerticalSmoothVelocity;

	// Camera's transforms current angle rotation.
	private float m_VerticalAngle;
	private float m_HorizontalAngle;

	// Variables used to lock an enemy
	private bool m_EnemyLockedOn;
	private Vector3 m_CameraToEnemy;
	private bool m_CameraLockOnAxisInUse;

	// Variables to center the camera
	private bool m_CenterCamera;
	private float m_PlayerHorizontalAngle;

	// Variables to avoid wall clipping. (Unity warning disabled)
	#pragma warning disable 649
	private Ray m_TargetToCameraRay;
	private RaycastHit m_AvoidClippingRaycastHit;






	void Awake () 
	{
		// Reference to GlobalData
		SystemAndData.CameraMovementScript = this;

		// Set the camera's transforms
		m_CameraVerticalPivot = m_CameraTransform.parent;
		m_CameraHorizontalPivot = this.transform;
		
		// Place the camera in the desired position
		m_CameraHorizontalPivot.position = m_PlayerTransform.position;
		m_CameraVerticalPivot.localPosition += Vector3.up*m_CameraHeight;
		m_CameraTransform.localPosition -= Vector3.forward*m_CameraDistance;

		// Get the camera.
		m_Camera = m_CameraTransform.GetComponent<Camera>();
	}
	
	void LateUpdate () 
	{
		RotateAroundPlayer();
		FollowPlayer();
		AvoidClipping();
	}


	void RotateAroundPlayer()
	{

		if (SystemAndData.IsEnemyLocked)
		{
			
			m_HorizontalInput = Input.GetAxis("CameraHorizontal"); 
			m_VerticalInput = Input.GetAxis("CameraVertical");m_VerticalInput *= (m_InvertVerticalInput)? -1:1;

			// If horizontal input is recieved, the lock on will be moved to another enemy.
			ChangeLockOn();
			
			// Manage Horizontal camera movement: Create a enemy to camera Vec3 with no height displacement. If the vector is 0, use the camera forward to avoid Quaternion problems.
			m_CameraToEnemy = SystemAndData.LockedEnemyTransform.position - m_CameraHorizontalPivot.position; m_CameraToEnemy.y = 0;
			if (m_CameraToEnemy == Vector3.zero) { m_CameraToEnemy = m_CameraHorizontalPivot.forward;}

			// Manage Vertical camera movement
			m_VerticalIncrement = Mathf.SmoothDamp(m_VerticalIncrement,m_VerticalInput,ref m_VerticalSmoothVelocity, m_CameraSmoothMultiplier*Time.deltaTime);
			
			m_HorizontalAngle = m_CameraHorizontalPivot.eulerAngles.y;
			m_VerticalAngle += m_VerticalIncrement*m_CameraSpeedMultiplier;
			m_VerticalAngle = Mathf.Clamp(m_VerticalAngle,m_MinimumVerticalAngle,m_MaximunmVerticalAngle);

			m_CameraHorizontalPivot.rotation = Quaternion.Slerp(m_CameraHorizontalPivot.rotation, Quaternion.LookRotation(m_CameraToEnemy),m_TargetCameraSpeedMultipier*Time.deltaTime);
			m_CameraVerticalPivot.localRotation = Quaternion.Euler(m_VerticalAngle,0,0);

			
		}
		else if (m_CenterCamera)
		{
			m_CameraHorizontalPivot.rotation = Quaternion.Slerp(m_CameraHorizontalPivot.rotation,Quaternion.Euler(0,m_PlayerHorizontalAngle,0),m_TargetCameraSpeedMultipier*Time.deltaTime);
			m_CameraVerticalPivot.localRotation = Quaternion.Slerp(m_CameraVerticalPivot.localRotation,Quaternion.Euler(0,0,0), m_TargetCameraSpeedMultipier*Time.deltaTime);
			
			m_HorizontalAngle = m_CameraHorizontalPivot.eulerAngles.y;
			m_VerticalAngle = m_CameraVerticalPivot.localEulerAngles.x;

			if (Mathf.Abs(m_PlayerHorizontalAngle-m_HorizontalAngle) < 1f && Mathf.Abs(m_VerticalAngle) < 1f)
			{
				m_CenterCamera = false;
			}
		}
		else
		{
			m_HorizontalInput = Input.GetAxis("CameraHorizontal"); m_HorizontalInput *= (m_InvertHorizontalInput)? -1:1;
			m_VerticalInput = Input.GetAxis("CameraVertical");m_VerticalInput *= (m_InvertVerticalInput)? -1:1;

			// If the user isn't moving the camera while the player is moving, auto rotate the camera.
            if (m_HorizontalInput ==  0 && m_CameraAutoRotation)
			{
				m_PlayerXInViewport = m_Camera.WorldToViewportPoint(m_PlayerTransform.position).x;

				if ( m_PlayerXInViewport != 0.5f )
				{
						m_HorizontalInput = (m_PlayerXInViewport - 0.5f)*m_CameraAutoRotateMultiplier;
				}
			}
			
			m_HorizontalIncrement = Mathf.SmoothDamp(m_HorizontalIncrement,m_HorizontalInput,ref m_HorizontalSmoothVelocity, m_CameraSmoothMultiplier*Time.deltaTime);
			m_VerticalIncrement = Mathf.SmoothDamp(m_VerticalIncrement,m_VerticalInput,ref m_VerticalSmoothVelocity, m_CameraSmoothMultiplier*Time.deltaTime);

			m_HorizontalAngle += m_HorizontalIncrement*m_CameraSpeedMultiplier;
			m_VerticalAngle += m_VerticalIncrement*m_CameraSpeedMultiplier;
			m_VerticalAngle = Mathf.Clamp(m_VerticalAngle,m_MinimumVerticalAngle,m_MaximunmVerticalAngle);

			m_CameraHorizontalPivot.rotation =  Quaternion.Euler(0,m_HorizontalAngle,0);	
			m_CameraVerticalPivot.localRotation = Quaternion.Euler(m_VerticalAngle,0,0);
			

			
		}
		
	}

	void FollowPlayer()
	{
		m_CameraHorizontalPivot.position = Vector3.Lerp(m_CameraHorizontalPivot.position,m_PlayerTransform.position,m_CameraFollowSpeedMultiplier*Time.deltaTime);
	}

	public void CenterCamera()
	{
		m_CenterCamera = true;
		m_PlayerHorizontalAngle = m_PlayerTransform.eulerAngles.y;
		
	}
	
	void ChangeLockOn()
	{
		if( Mathf.Abs(m_HorizontalInput) > 0.4f && !m_CameraLockOnAxisInUse)
		{
			SystemAndData.ChangeLockOn(m_HorizontalInput);
			m_CameraLockOnAxisInUse = true;
		}
		if( Mathf.Abs(m_HorizontalInput) <= 0.4f)
		{
			m_CameraLockOnAxisInUse = false;
		}
	}

	void AvoidClipping()
	{
		m_TargetToCameraRay.origin = m_CameraHorizontalPivot.position + m_CameraVerticalPivot.localPosition;
		m_TargetToCameraRay.direction = m_CameraTransform.position-m_TargetToCameraRay.origin;

		if (Physics.SphereCast(m_TargetToCameraRay,m_CameraClippingOffset, out m_AvoidClippingRaycastHit, m_CameraDistance, (1 << LayerMask.NameToLayer("Environment"))))
		{
			m_CameraTransform.localPosition = -Vector3.forward*(m_AvoidClippingRaycastHit.distance-m_CameraClippingOffset);
		}
		else
		{
			m_CameraTransform.localPosition = -Vector3.forward*m_CameraDistance;
		}

	}

}
