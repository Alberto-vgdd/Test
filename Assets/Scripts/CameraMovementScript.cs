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
	[Range(60.0f, 240.0f)] //Degrees per second
	public float m_JoystickSensitivy; 
	[Range(0.01f, 1f)]   //Mouse sensitivity
	public float m_MouseSensitivity;
	public float m_ResetCameraSpeedMultipier;
	public float m_LockOnSpeedMultipier;
	public bool m_InvertJoystickHorizontalInput;
	public bool m_InvertJoystickVerticalInput;
	public bool m_InvertMouseHorizontalInput;
	public bool m_InvertMouseVerticalInput;

	[Header("Axes Management")]
	public string m_HorizontalAxesName;
	public string m_VerticalAxesName;
	public string m_JoystickSubfix;
	public string m_MouseSubfix;

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

	// Camera Inputs.
	private float m_ChangeTargetInput;
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

	// Variables to switch from mouse to joystick
	private bool m_InvertHorizontalInput;
	private bool m_InvertVerticalInput;
	private float m_CameraSpeed;
	private bool m_JoystickInUse;




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

		//TEST 
		Application.targetFrameRate = 999;
		Cursor.lockState = CursorLockMode.Locked;
		m_JoystickInUse = false;

	}
	

	void LateUpdate () 
	{
		UpdateInputs();
		RotateAroundPlayer();
		FollowPlayer();
		AvoidClipping();
	}


	void UpdateInputs()
	{
		m_JoystickInUse = SystemAndData.GetJoystickInUse();

		if  (!m_JoystickInUse)
		{
				m_InvertHorizontalInput = m_InvertMouseHorizontalInput;
				m_InvertVerticalInput = m_InvertMouseVerticalInput;
				m_CameraSpeed = m_MouseSensitivity;
		}
		else
		{
				m_InvertHorizontalInput = m_InvertJoystickHorizontalInput;
				m_InvertVerticalInput = m_InvertJoystickVerticalInput;
				m_CameraSpeed = m_JoystickSensitivy*Time.deltaTime;
		}
		m_ChangeTargetInput = SystemAndData.GetChangeTarget();m_ChangeTargetInput *= (m_InvertHorizontalInput)? -1f:1f;
		m_HorizontalInput = SystemAndData.GetHorizontalCameraInput(); m_HorizontalInput *= (m_InvertHorizontalInput)? -1f:1f;
		m_VerticalInput = SystemAndData.GetVerticalCameraInput(); m_VerticalInput *= (m_InvertVerticalInput)? -1f:1f;
	}

	void RotateAroundPlayer()
	{
		

		if (SystemAndData.IsEnemyLocked)
		{
			
			// If any input has been recieved, the lock on will be moved to another enemy.
			ChangeLockOn();
			
			// Manage Horizontal camera movement: Create a enemy to camera Vec3 with no height displacement. If the vector is 0, use the camera forward to avoid Quaternion problems.
			m_CameraToEnemy = SystemAndData.LockedEnemyTransform.position - m_CameraHorizontalPivot.position; m_CameraToEnemy.y = 0;
			if (m_CameraToEnemy == Vector3.zero) { m_CameraToEnemy = m_CameraHorizontalPivot.forward;}

			// Manage Vertical camera movement
			m_VerticalIncrement = Mathf.SmoothDamp(m_VerticalIncrement,m_VerticalInput,ref m_VerticalSmoothVelocity, m_CameraSmoothMultiplier);
			
			m_HorizontalAngle = m_CameraHorizontalPivot.eulerAngles.y;
			m_VerticalAngle += m_VerticalIncrement*m_CameraSpeed;
			m_VerticalAngle = Mathf.Clamp(m_VerticalAngle,m_MinimumVerticalAngle,m_MaximunmVerticalAngle);

			m_CameraHorizontalPivot.rotation = Quaternion.Slerp(m_CameraHorizontalPivot.rotation, Quaternion.LookRotation(m_CameraToEnemy),m_LockOnSpeedMultipier*Time.deltaTime);
			m_CameraVerticalPivot.localRotation = Quaternion.Euler(m_VerticalAngle,0,0);

			
		}
		else if (m_CenterCamera)
		{
			m_CameraHorizontalPivot.rotation = Quaternion.Slerp(m_CameraHorizontalPivot.rotation,Quaternion.Euler(0,m_PlayerHorizontalAngle,0),m_ResetCameraSpeedMultipier*Time.deltaTime);
			m_CameraVerticalPivot.localRotation = Quaternion.Slerp(m_CameraVerticalPivot.localRotation,Quaternion.Euler(0,0,0), m_ResetCameraSpeedMultipier*Time.deltaTime);
			
			m_HorizontalAngle = m_CameraHorizontalPivot.eulerAngles.y;
			m_VerticalAngle = m_CameraVerticalPivot.localEulerAngles.x;

			if (Mathf.Abs(m_PlayerHorizontalAngle-m_HorizontalAngle) < 1f && Mathf.Abs(m_VerticalAngle) < 1f)
			{
				m_CenterCamera = false;
			}
		}
		else
		{
			// If the user isn't moving the camera while the player is moving, auto rotate the camera with joystick's speed.
            if ( m_CameraAutoRotation && m_JoystickInUse && m_HorizontalInput ==  0 )
			{
				
				m_PlayerXInViewport = m_Camera.WorldToViewportPoint(m_PlayerTransform.position).x;
				m_HorizontalInput = (m_PlayerXInViewport - 0.5f)*m_CameraAutoRotateMultiplier;

				m_HorizontalIncrement = Mathf.SmoothDamp(m_HorizontalIncrement,m_HorizontalInput,ref m_HorizontalSmoothVelocity, m_CameraSmoothMultiplier);
				m_VerticalIncrement = Mathf.SmoothDamp(m_VerticalIncrement,m_VerticalInput,ref m_VerticalSmoothVelocity, m_CameraSmoothMultiplier);

				m_HorizontalAngle += m_HorizontalIncrement*m_JoystickSensitivy*Time.deltaTime;
				m_VerticalAngle += m_VerticalIncrement*m_JoystickSensitivy*Time.deltaTime;
				m_VerticalAngle = Mathf.Clamp(m_VerticalAngle,m_MinimumVerticalAngle,m_MaximunmVerticalAngle);

			}
			else
			{
				m_HorizontalIncrement = Mathf.SmoothDamp(m_HorizontalIncrement,m_HorizontalInput,ref m_HorizontalSmoothVelocity, m_CameraSmoothMultiplier);
				m_VerticalIncrement = Mathf.SmoothDamp(m_VerticalIncrement,m_VerticalInput,ref m_VerticalSmoothVelocity, m_CameraSmoothMultiplier);

				m_HorizontalAngle += m_HorizontalIncrement*m_CameraSpeed;
				m_VerticalAngle += m_VerticalIncrement*m_CameraSpeed;
				m_VerticalAngle = Mathf.Clamp(m_VerticalAngle,m_MinimumVerticalAngle,m_MaximunmVerticalAngle);
			}

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
		if( Mathf.Abs(m_ChangeTargetInput) > 0f && !m_CameraLockOnAxisInUse)
		{
			SystemAndData.ChangeLockOn(m_ChangeTargetInput);
			m_CameraLockOnAxisInUse = true;
		}
		if( m_ChangeTargetInput == 0f)
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
