using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointScript : MonoBehaviour
{
	private bool checkPointVisited;
	private string playerTag;

	void Start()
	{
		checkPointVisited = false;
		playerTag = GlobalData.PlayerTag;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!checkPointVisited && other.tag.Equals(playerTag))
		{
			GlobalData.GameManager.UpdateCheckPoint(this.transform, GlobalData.FreeCameraMovementScript.enabled,GlobalData.FixedCameraMovementScript.enabled);
			checkPointVisited = true;
		}
	}
}
