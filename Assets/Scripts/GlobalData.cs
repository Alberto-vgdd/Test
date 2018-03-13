using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData
{
    //  GameManager variables
    public static GameManagerScript GameManager;
    public static bool PlayerDeath;

    // Reference to the GameUJI
    public static GameUIScript GameUIScript;
    
    // Layer Masks
    public static LayerMask EnvironmentLayerMask = LayerMask.GetMask("Environment");

    // Tags
    public const string PlayerTag = "Player";

    // Variables to lock enemies
    public static bool IsEnemyLocked;
    public static Transform LockedEnemyTransform;

    // Player Transforms
    public static Transform PlayerTransform;
    public static Transform PlayerTargetTransform;

    // Player Camera
    public static Camera PlayerCamera;
    public static Transform PlayerCameraTransform;

    // Camera Scripts
    public static FreeCameraMovementScript FreeCameraMovementScript;
    public static FixedCameraMovementScript FixedCameraMovementScript;
    public static CameraEnemyTrackerScript CameraEnemyTrackerScript;

    // Input Manager script
    public static InputManagerScript InputManagerScript;


    // Call the function in the CameraEnemyTrackerScript
    public static void ChangeLockOn(float input)  {    CameraEnemyTrackerScript.ChangeLockOn(input);  }

    // Call the function in the CameraMovementScript
    public static void CenterCamera(){    FreeCameraMovementScript.CenterCamera();}


    // Call the function in InputManagerScript
    public static bool GetJoystickInUse(){     return InputManagerScript.GetJoystickInUse();}
    public static float GetHorizontalInput(){    return InputManagerScript.GetHorizontalInput();}
    public static float GetVerticalInput(){    return InputManagerScript.GetVerticalInput();}
    public static float GetHorizontalCameraInput(){    return InputManagerScript.GetHorizontalCameraInput();}
    public static float GetVerticalCameraInput(){    return InputManagerScript.GetVerticalCameraInput();}
    public static bool GetLockOnButton(){    return InputManagerScript.GetLockOnButton();}
    public static float GetChangeTarget(){		return InputManagerScript.GetChangeTarget();}
    public static bool GetJumpButtonDown(){		return InputManagerScript.GetJumpButtonDown();}

    

}
