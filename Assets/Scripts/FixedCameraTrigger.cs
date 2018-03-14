using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCameraTrigger : MonoBehaviour
{
	private FixedCameraMovementScript camera2D;
	private FreeCameraMovementScript camera3D;
	private string playerTag;

	[Header("Camera Parameters")]
	public float targetDistance;
	public float targetHeight = 1f;
	public float targetHoriontalAngle;
	public float targetVerticalAngle;

	[Header("Camera Movement Parameters")]
	public float cameraFollowSpeedMultiplier = 5f;
	public float cameraTransitionTime = 2f;

	[Header("Clipping Parameters")]
	public float cameraClippingOffset = 0.05f;

	void Start () 
	{
		camera2D = GlobalData.FixedCameraMovementScript;
		camera3D = GlobalData.FreeCameraMovementScript;
		playerTag = GlobalData.PlayerTag;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!camera2D.enabled && other.CompareTag(playerTag))
		{
			camera3D.enabled = false;
			
			camera2D.enabled = true;
			camera2D.SetUp(targetDistance,targetHeight,targetHoriontalAngle,targetVerticalAngle,cameraFollowSpeedMultiplier,cameraTransitionTime,cameraClippingOffset);
			camera2D.StartCameraTransition();
		}
	}
}
