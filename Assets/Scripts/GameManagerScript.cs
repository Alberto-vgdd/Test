using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour 
{
	// Player and Camera variables, used to move them.
	private Transform playerTransform;
	private Transform playerCameraTransform;
	private FreeCameraMovementScript freeCameraMovementScript;
	private FixedCameraMovementScript fixedCameraMovementScript;


	// Checkpoint stuff, used to restore the state after a death
	private Transform checkPoint;
	private bool checkPointFreeCameraEnabled;
	private bool checkPointFixedCameraEnabled;

	// GameUI script
	private GameUIScript gameUIScript;

	// Use this for initialization
	void Awake()
	{
		if (GlobalData.GameManager == null)
		{
			GlobalData.GameManager = this;

			GlobalData.PlayerTransform = playerTransform = GameObject.Find("Player").transform;
			GlobalData.PlayerTargetTransform = playerTransform.Find("Target");
			GlobalData.PlayerCameraTransform = playerCameraTransform = GameObject.Find("Camera Horizontal Pivot").transform;
			GlobalData.PlayerCamera = playerCameraTransform.GetComponentInChildren<Camera>();

			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

	void Start()
	{
		freeCameraMovementScript = GlobalData.FreeCameraMovementScript;
		fixedCameraMovementScript = GlobalData.FixedCameraMovementScript;
		gameUIScript = GlobalData.GameUIScript;
	}

	void Update()
	{
		if ( Input.GetMouseButtonDown(0))
		{
			StartCoroutine(GameOver());

		}
	}
	public void UpdateCheckPoint(Transform newCheckPoint, bool freeCameraEnabled, bool fixedCameraEnabled)
	{
		checkPoint = newCheckPoint;
		checkPointFreeCameraEnabled = freeCameraEnabled;
		checkPointFixedCameraEnabled = fixedCameraEnabled;
		
	}



	IEnumerator GameOver()
	{
		// "Kill" the character
		GlobalData.PlayerDeath = true;

		// Fade out the game.
		gameUIScript.StartGameFadeOut();

		// Wait for the game to fade out, and then move the character and the camera to the checkpoint's position.
		yield return new WaitForSeconds(1f);
		playerTransform.position = playerCameraTransform.position = checkPoint.position;
		playerTransform.rotation = playerCameraTransform.rotation = checkPoint.rotation;

		if (checkPointFixedCameraEnabled)
		{
			fixedCameraMovementScript.enabled = true;
			fixedCameraMovementScript.StartCameraTransition();
		}
		if (checkPointFreeCameraEnabled)
		{
			freeCameraMovementScript.enabled = true;
			freeCameraMovementScript.CenterCamera();
		}

		// Wait for the camera to move properly to the character position and then fade in.
		yield return new WaitForSeconds(0.5f);
		gameUIScript.StartGameFadeIn();

		// "Revive" the character
		GlobalData.PlayerDeath = false;
	}
}
