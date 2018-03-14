﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCameraMovementScript : MonoBehaviour 
{

	//[Header("Player and Camera Transforms")]
	private Transform playerTarget;
	private Transform cameraTransform;

	[Header("Camera Parameters")]
	public float targetDistance = 4f;
	public float targetHeight = 1f;
	public float maximunmVerticalAngle = 35f;
	public float minimumVerticalAngle = -25f;

	[Header("Camera Movement Parameters")]
	public float cameraFollowSpeedMultiplier = 5f;
	public float cameraSmoothMultiplier = 0.1f;
	[Range(60.0f, 240.0f)] //Degrees per second
	public float joystickSensitivy = 180f; 
	[Range(0.01f, 1f)]   //Mouse sensitivity
	public float mouseSensitivity = 0.2f;
	public float lockOnSpeedMultipier = 7.5f;


	[Header("Center and Reset Camera times")]
	public float resetCameraTime = 1f;
	public float centerCameraTime = 0.5f;

	[Header("Axes Management")]
	public bool invertJoystickHorizontalInput = false;
	public bool invertJoystickVerticalInput = false;
	public bool invertMouseHorizontalInput = false;
	public bool invertMouseVerticalInput = true;
	private const string horizontalAxesName = "CameraHorizontal";
	private const string verticalAxesName = "CameraVertical";
	private const string joystickSubfix = "Joystick";
	private const string mouseSubfix = "Mouse";


	[Header("Camera Auto Rotation Parameters")]
	public float cameraAutoRotateMultiplier = 5f; //2 is slow, 3 and 5 seem fine
	public bool cameraAutoRotation = true;
	private Camera m_Camera;
	private float m_PlayerXInViewport;

	[Header("Clipping Parameters")]
	public float cameraClippingOffset = 0.05f;


	// Transforms to manage the camera rotations.
	private Transform cameraHorizontalPivot;
	private Transform cameraVerticalPivot;

	// Camera Inputs.
	private float changeTargetInput;
	private float horizontalInput;
	private float verticalInput;

	// Values used to smoothly rotate the camera.
	private float horizontalIncrement;
	private float verticalIncrement;
	private float horizontalSmoothVelocity;
	private float verticalSmoothVelocity;

	// Camera's transforms current angle rotation.
	private float verticalAngle;
	private float horizontalAngle;

	// Variables used to lock an enemy
	private bool enemyLockedOn;
	private Vector3 cameraToEnemy;
	private bool cameraLockOnAxisInUse;


	// Variables to avoid wall clipping. (Unity warning disabled)
	#pragma warning disable 649
	private float cameraDistance;
	private Ray targetToCameraRay;
	private RaycastHit avoidClippingRaycastHit;

	// Variables to switch from mouse to joystick
	private bool invertHorizontalInput;
	private bool invertVerticalInput;
	private float cameraSpeed;
	private bool joystickInUse;


	// Variables used to smooth center and reset the camera.
	private bool resetCamera;
	private bool centerCamera;
	private float resetCameraTimer;
	private float centerCameraTimer;

	private float playerHorizontalAngle;
	private Quaternion previousHorizontalQuaternion;
	private Quaternion previousVerticalQuaternion;
	private Vector3 previousVerticalPosition;
	private Vector3 previousCameraPosition;


	void Awake () 
	{
		// Reference to GlobalData
		GlobalData.FreeCameraMovementScript = this;

		//TEST 
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Start()
	{
		// Get the transforms from the Global Data
		playerTarget = GlobalData.PlayerTargetTransform;
		m_Camera = GlobalData.PlayerCamera;

		// Set camera's transfoms
		cameraTransform = m_Camera.transform;		
		cameraVerticalPivot = cameraTransform.parent;	
		cameraHorizontalPivot = cameraVerticalPivot.parent;	

	
		// Place the camera in the desired position
		cameraHorizontalPivot.position = playerTarget.position;


		// Center the camera at the start of the game (Because the free camera is the default camera script)
		StartCameraTransition();
	}

	// Stop any possible transition before shutting down the script
	void OnDisable()
	{
		resetCamera = false;
		centerCamera = false;
	}

	public void StartCameraTransition()
	{
		centerCamera = false;

		if (!resetCamera)
		{
			resetCamera = true;
			resetCameraTimer = 0f;
			// Target where the player is looking
			//playerHorizontalAngle = playerTarget.eulerAngles.y;
			playerHorizontalAngle = cameraHorizontalPivot.eulerAngles.y;

			previousCameraPosition = cameraTransform.localPosition;
			previousHorizontalQuaternion = cameraHorizontalPivot.rotation;
			previousVerticalQuaternion = cameraVerticalPivot.localRotation;
			previousVerticalPosition = cameraVerticalPivot.localPosition;

		}
		

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
		joystickInUse = GlobalData.GetJoystickInUse();
		if (!GlobalData.PlayerDeath)
		{
			if  (!joystickInUse)
			{
					invertHorizontalInput = invertMouseHorizontalInput;
					invertVerticalInput = invertMouseVerticalInput;
					cameraSpeed = mouseSensitivity;
			}
			else
			{
					invertHorizontalInput = invertJoystickHorizontalInput;
					invertVerticalInput = invertJoystickVerticalInput;
					cameraSpeed = joystickSensitivy*Time.deltaTime;
			}
			changeTargetInput = GlobalData.GetChangeTarget();changeTargetInput *= (invertHorizontalInput)? -1f:1f;
			horizontalInput = GlobalData.GetHorizontalCameraInput(); horizontalInput *= (invertHorizontalInput)? -1f:1f;
			verticalInput = GlobalData.GetVerticalCameraInput(); verticalInput *= (invertVerticalInput)? -1f:1f;
		}
		else
		{
			changeTargetInput = 0f;
			horizontalInput = 0f;
			verticalInput = 0f;
		}
	}

	void RotateAroundPlayer()
	{
		

		if (GlobalData.IsEnemyLocked)
		{
			
			// If any input has been recieved, the lock on will be moved to another enemy.
			ChangeLockOn();
			
			// Manage Horizontal camera movement: Create a enemy to camera Vec3 with no height displacement. If the vector is 0, use the camera forward to avoid Quaternion problems.
			cameraToEnemy = GlobalData.LockedEnemyTransform.position - cameraHorizontalPivot.position; cameraToEnemy.y = 0;
			if (cameraToEnemy == Vector3.zero) { cameraToEnemy = cameraHorizontalPivot.forward;}

			// Manage Vertical camera movement
			verticalIncrement = Mathf.SmoothDamp(verticalIncrement,verticalInput,ref verticalSmoothVelocity, cameraSmoothMultiplier);
			
			horizontalAngle = cameraHorizontalPivot.eulerAngles.y;
			verticalAngle += verticalIncrement*cameraSpeed;
			verticalAngle = Mathf.Clamp(verticalAngle,minimumVerticalAngle,maximunmVerticalAngle);

			cameraHorizontalPivot.rotation = Quaternion.Slerp(cameraHorizontalPivot.rotation, Quaternion.LookRotation(cameraToEnemy),lockOnSpeedMultipier*Time.deltaTime);
			cameraVerticalPivot.localRotation = Quaternion.Euler(verticalAngle,0,0);

			
		}
		else if (resetCamera)
		{
			resetCameraTimer += Time.deltaTime;
			float step = Mathf.SmoothStep(0,1,resetCameraTimer/resetCameraTime);


			// Place the camera in the desired position
			cameraVerticalPivot.localPosition = Vector3.Slerp(previousVerticalPosition,Vector3.up*targetHeight,step);
			cameraTransform.localPosition = Vector3.Slerp(previousCameraPosition,-Vector3.forward*targetDistance,step); 
			cameraDistance = Mathf.Abs(cameraTransform.localPosition.z);

			// Rotate the camera
			cameraHorizontalPivot.rotation = Quaternion.Slerp(previousHorizontalQuaternion,Quaternion.Euler(0,playerHorizontalAngle,0),step);
			cameraVerticalPivot.localRotation = Quaternion.Slerp(previousVerticalQuaternion,Quaternion.Euler(0,0,0),step);
	
			horizontalAngle = cameraHorizontalPivot.eulerAngles.y;
			verticalAngle = cameraVerticalPivot.localEulerAngles.x;

			if (resetCameraTimer >= resetCameraTime)
			{
				resetCamera = false;
			}
		}
		else if (centerCamera)
		{
			centerCameraTimer += Time.deltaTime;
			float step = Mathf.SmoothStep(0,1,centerCameraTimer/centerCameraTime);
			
			// Rotate the camera
			cameraHorizontalPivot.rotation = Quaternion.Slerp(previousHorizontalQuaternion,Quaternion.Euler(0,playerHorizontalAngle,0),step);
			cameraVerticalPivot.localRotation = Quaternion.Slerp(previousVerticalQuaternion,Quaternion.Euler(0,0,0),step);
	
			horizontalAngle = cameraHorizontalPivot.eulerAngles.y;
			verticalAngle = cameraVerticalPivot.localEulerAngles.x;

			if (centerCameraTimer >= centerCameraTime)
			{
				centerCamera = false;
			}
		}
		else
		{
			// If the user isn't moving the camera while the player is moving, auto rotate the camera with joystick's speed.
            if ( cameraAutoRotation && joystickInUse && horizontalInput ==  0 )
			{
				
				m_PlayerXInViewport = m_Camera.WorldToViewportPoint(playerTarget.position).x;
				horizontalInput = (m_PlayerXInViewport - 0.5f)*cameraAutoRotateMultiplier;

				horizontalIncrement = Mathf.SmoothDamp(horizontalIncrement,horizontalInput,ref horizontalSmoothVelocity, cameraSmoothMultiplier);
				verticalIncrement = Mathf.SmoothDamp(verticalIncrement,verticalInput,ref verticalSmoothVelocity, cameraSmoothMultiplier);

				horizontalAngle += horizontalIncrement*joystickSensitivy*Time.deltaTime;
				verticalAngle += verticalIncrement*joystickSensitivy*Time.deltaTime;
				verticalAngle = Mathf.Clamp(verticalAngle,minimumVerticalAngle,maximunmVerticalAngle);

			}
			else
			{
				horizontalIncrement = Mathf.SmoothDamp(horizontalIncrement,horizontalInput,ref horizontalSmoothVelocity, cameraSmoothMultiplier);
				verticalIncrement = Mathf.SmoothDamp(verticalIncrement,verticalInput,ref verticalSmoothVelocity, cameraSmoothMultiplier);

				horizontalAngle += horizontalIncrement*cameraSpeed;
				verticalAngle += verticalIncrement*cameraSpeed;
				verticalAngle = Mathf.Clamp(verticalAngle,minimumVerticalAngle,maximunmVerticalAngle);
			}

			cameraHorizontalPivot.rotation =  Quaternion.Euler(0,horizontalAngle,0);	
			cameraVerticalPivot.localRotation = Quaternion.Euler(verticalAngle,0,0);
			

			
		}
		
	}

	void FollowPlayer()
	{
		if (!GlobalData.PlayerDeath)
		{
			cameraHorizontalPivot.position = Vector3.Lerp(cameraHorizontalPivot.position,playerTarget.position,cameraFollowSpeedMultiplier*Time.deltaTime);
		}
	}

	public void CenterCamera()
	{
		if (!resetCamera)
		{
			centerCamera = true;
			centerCameraTimer = 0f;

			playerHorizontalAngle = playerTarget.eulerAngles.y;

			previousCameraPosition = cameraTransform.localPosition;
			previousHorizontalQuaternion = cameraHorizontalPivot.rotation;
			previousVerticalQuaternion = cameraVerticalPivot.localRotation;
			previousVerticalPosition = cameraVerticalPivot.localPosition;
		}
	}
	
	void ChangeLockOn()
	{
		if( Mathf.Abs(changeTargetInput) > 0f && !cameraLockOnAxisInUse)
		{
			GlobalData.ChangeLockOn(changeTargetInput);
			cameraLockOnAxisInUse = true;
		}
		if( changeTargetInput == 0f)
		{
			cameraLockOnAxisInUse = false;
		}
		
		
	}

	void AvoidClipping()
	{
		targetToCameraRay.origin = cameraHorizontalPivot.position + cameraVerticalPivot.localPosition;
		targetToCameraRay.direction = cameraTransform.position-targetToCameraRay.origin;

		if (Physics.SphereCast(targetToCameraRay,cameraClippingOffset, out avoidClippingRaycastHit, cameraDistance, (1 << LayerMask.NameToLayer("Environment"))))
		{
			cameraTransform.localPosition = -Vector3.forward*(avoidClippingRaycastHit.distance-cameraClippingOffset);
		}
		else
		{
			cameraTransform.localPosition = -Vector3.forward*cameraDistance;
		}

	}

}
