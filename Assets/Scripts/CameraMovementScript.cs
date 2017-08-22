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

	[Header("Clipping Parameters")]
	public float m_CameraClippingOffset;


	// Transforms to manage the camera rotations. (Divided in 2 different axes.)
	private Transform m_CameraHorizontalPivot;
	private Transform m_CameraVerticalPivot;

	// Controller Inputs
	float m_HorizontalInput;
	float m_VerticalInput;

	// Values used to smoothly rotate the camera.
	float m_HorizontalIncrement;
	float m_VerticalIncrement;
	float m_HorizontalSmoothVelocity;
	float m_VerticalSmoothVelocity;

	// Camera's transforms current angle rotation.
	float m_VerticalAngle;
	float m_HorizontalAngle;

	// Variables used to lock an enemy
	bool m_EnemyLockedOn;
	Vector3 m_CameraToEnemy;

	// Variables to center the camera
	bool m_CenterCamera;
	float m_PlayerHorizontalAngle;

	// Variables to avoid wall clipping.
	Ray m_TargeToCameraRay;
	RaycastHit m_AvoidClippingRaycastHit;

	void Awake () 
	{
		//Set the camera's transforms
		m_CameraVerticalPivot = m_CameraTransform.parent;
		m_CameraHorizontalPivot = this.transform;
		
		//Place the camera in the desired position
		m_CameraHorizontalPivot.position = m_PlayerTransform.position;
		m_CameraVerticalPivot.localPosition += Vector3.up*m_CameraHeight;
		m_CameraTransform.localPosition -= Vector3.forward*m_CameraDistance;
	}
	
	void LateUpdate () 
	{
		RotateAroundPlayer();
		FollowPlayer();
		AvoidClipping();
	}


	void RotateAroundPlayer()
	{
		m_HorizontalInput = Input.GetAxis("CameraHorizontal"); m_HorizontalInput *= (m_InvertHorizontalInput)? -1:1;
		m_VerticalInput = Input.GetAxis("CameraVertical");m_VerticalInput *= (m_InvertVerticalInput)? -1:1;

		m_HorizontalIncrement = Mathf.SmoothDamp(m_HorizontalIncrement,m_HorizontalInput,ref m_HorizontalSmoothVelocity, m_CameraSmoothMultiplier*Time.deltaTime);
		m_VerticalIncrement = Mathf.SmoothDamp(m_VerticalIncrement,m_VerticalInput,ref m_VerticalSmoothVelocity, m_CameraSmoothMultiplier*Time.deltaTime);

		m_HorizontalAngle += m_HorizontalIncrement*m_CameraSpeedMultiplier;
		m_VerticalAngle += m_VerticalIncrement*m_CameraSpeedMultiplier;
		m_VerticalAngle = Mathf.Clamp(m_VerticalAngle,m_MinimumVerticalAngle,m_MaximunmVerticalAngle);
		
		if (m_EnemyLockedOn)
		{
			// Create a enemy to camera Vec3 with no height displacement. If the vector is 0, use the camera forward to avoid Quaternion problems.
			m_CameraToEnemy = GlobalData.LockedEnemyTransform.position - m_CameraHorizontalPivot.position; m_CameraToEnemy.y = 0;
			if (m_CameraToEnemy == Vector3.zero) { m_CameraToEnemy = m_CameraHorizontalPivot.forward;}

			m_CameraHorizontalPivot.rotation = Quaternion.Slerp(m_CameraHorizontalPivot.rotation, Quaternion.LookRotation(m_CameraToEnemy),m_TargetCameraSpeedMultipier*Time.deltaTime);
			m_CameraVerticalPivot.localRotation = Quaternion.Euler(m_VerticalAngle,0,0);

			m_HorizontalAngle = m_CameraHorizontalPivot.eulerAngles.y;
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
			m_CameraHorizontalPivot.rotation = Quaternion.Euler(0,m_HorizontalAngle,0);	
			m_CameraVerticalPivot.localRotation = Quaternion.Euler(m_VerticalAngle,0,0);
		}
		
	}

	void FollowPlayer()
	{
		m_CameraHorizontalPivot.position = Vector3.Lerp(m_CameraHorizontalPivot.position,m_PlayerTransform.position,m_CameraFollowSpeedMultiplier*Time.deltaTime);
	}

	public void LockOn()
	{
		m_EnemyLockedOn = !m_EnemyLockedOn;
	}

	public void CenterCamera()
	{
		m_CenterCamera = true;
		m_PlayerHorizontalAngle = m_PlayerTransform.eulerAngles.y;
		
	}

	void AvoidClipping()
	{
		m_TargeToCameraRay.origin = m_CameraHorizontalPivot.position + m_CameraVerticalPivot.localPosition;
		m_TargeToCameraRay.direction = m_CameraTransform.position-m_TargeToCameraRay.origin;

		if (Physics.SphereCast(m_TargeToCameraRay,m_CameraClippingOffset, out m_AvoidClippingRaycastHit, m_CameraDistance, (1 << LayerMask.NameToLayer("Environment"))))
		{
			m_CameraTransform.localPosition = -Vector3.forward*(m_AvoidClippingRaycastHit.distance-m_CameraClippingOffset);
		}
		else
		{
			m_CameraTransform.localPosition = -Vector3.forward*m_CameraDistance;
		}

	}
}
