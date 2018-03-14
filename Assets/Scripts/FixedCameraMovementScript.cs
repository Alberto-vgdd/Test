using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCameraMovementScript : MonoBehaviour 
{

	[Header("Player and Camera Transforms")]
	public Transform playerTarget;
	public Transform cameraTransform;

	// Camera Parameters
	private float targetDistance;
	private float targetHeight;
	private float targetHoriontalAngle;
	private float targetVerticalAngle;

	// Camera Movement Parameters
	private float cameraFollowSpeedMultiplier;
	private float cameraTransitionTime;

	// Clipping Parameters
	private float cameraClippingOffset;


	// Transforms to manage the camera rotations.
	private Transform cameraHorizontalPivot;
	private Transform cameraVerticalPivot;

	// Values used to smoothly transition the camera position/rotation
	private bool centerCamera;
	private float cameraTransitionTimer;
	private Quaternion previousHorizontalQuaternion;
	private Quaternion previousVerticalQuaternion;
	private Vector3 previousVerticalPosition;
	private Vector3 previousCameraPosition;

	// Variables to avoid wall clipping. (Unity warning disabled)
	#pragma warning disable 649
	private float cameraDistance;
	private Ray targetToCameraRay;
	private RaycastHit avoidClippingRaycastHit;




	// Can't use System and Data because OnEnable() is called before that Start()
	// Everything related with CenterCamera must be initialized here then.
	void Awake () 
	{
		// Reference to GlobalData
		GlobalData.FixedCameraMovementScript = this;

		// Set the camera's transforms. 
		cameraVerticalPivot = cameraTransform.parent;
		cameraHorizontalPivot = this.transform;

		// Place the camera in the desired position
		cameraHorizontalPivot.position = playerTarget.position;

		//TEST 
		Cursor.lockState = CursorLockMode.Locked;
	}
	
	// Stop any possible transition before shutting down the script
	void OnDisable()
	{
		centerCamera = false;
	}
	
	public void SetUp (float targetDistance,float targetHeight,float targetHorizontalAngle, float targetVerticalAngle, float cameraFollowSpeedMultiplier, float cameraTransitionTime, float cameraClippingOffset )
	{
		this.targetDistance = targetDistance;
		this.targetHeight = targetHeight;
		this.targetHoriontalAngle = targetHorizontalAngle;
		this.targetVerticalAngle = targetVerticalAngle;
		this.cameraFollowSpeedMultiplier = cameraFollowSpeedMultiplier;
		this.cameraTransitionTime = cameraTransitionTime;
		this.cameraClippingOffset = cameraClippingOffset;
	}

	
	void LateUpdate () 
	{
		CenterCamera();
		FollowPlayer();
		AvoidClipping();
	}

	void CenterCamera()
	{
		if (centerCamera)
		{
			cameraTransitionTimer += Time.deltaTime;
			float step = Mathf.SmoothStep(0,1,cameraTransitionTimer/cameraTransitionTime);

		
			// Place the camera in the desired position
			cameraVerticalPivot.localPosition = Vector3.Slerp(previousVerticalPosition,Vector3.up*targetHeight,step);
			cameraTransform.localPosition = Vector3.Slerp(previousCameraPosition,-Vector3.forward*targetDistance,step); 
			cameraDistance = Mathf.Abs(cameraTransform.localPosition.z);

			// Rotate the camera
			cameraHorizontalPivot.rotation = Quaternion.Slerp(previousHorizontalQuaternion,Quaternion.Euler(0,targetHoriontalAngle,0),step);
			cameraVerticalPivot.localRotation = Quaternion.Slerp(previousVerticalQuaternion,Quaternion.Euler(targetVerticalAngle,0,0),step);
	
			if (cameraTransitionTimer >= cameraTransitionTime)
			{
				
				centerCamera = false;
			}
		}
	}

	void FollowPlayer()
	{
		if (!GlobalData.PlayerDeath)
		{
			cameraHorizontalPivot.position = Vector3.Lerp(cameraHorizontalPivot.position,playerTarget.position,cameraFollowSpeedMultiplier*Time.deltaTime);
		}
	}

	public void StartCameraTransition()
	{
		if (!centerCamera)
		{
			centerCamera = true;
			cameraTransitionTimer = 0f;
			
			previousCameraPosition = cameraTransform.localPosition;
			previousHorizontalQuaternion = cameraHorizontalPivot.rotation;
			previousVerticalQuaternion = cameraVerticalPivot.localRotation;
			previousVerticalPosition = cameraVerticalPivot.localPosition;
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
