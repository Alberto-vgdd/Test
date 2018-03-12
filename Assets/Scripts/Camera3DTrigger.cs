using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera3DTrigger : MonoBehaviour 
{

	private Camera2DMovementScript camera2D;
	private CameraMovementScript camera3D;

	// Use this for initialization
	void Start () 
	{
		camera2D = SystemAndData.Camera2DMovementScript;
		camera3D = SystemAndData.CameraMovementScript;
	}
	

	void OnTriggerEnter(Collider other)
	{
		if (other.tag.Equals("Player"))
		{
			camera2D.enabled = false;
			camera3D.enabled = true;
		}
	}
}
