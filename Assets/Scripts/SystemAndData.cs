using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SystemAndData 
{

    // Variables to lock enemies
    public static bool IsEnemyLocked;
    public static Transform LockedEnemyTransform;

    // Player and Camera Transform
    public static Transform PlayerTransform;
    public static Camera PlayerCamera;

    // Camera Transform and Scripts
    public static CameraMovementScript CameraMovementScript;
    public static CameraEnemyTrackerScript CameraEnemyTrackerScript;

    // Input Manager script
    public static InputManagerScript InputManagerScript;


    // Call the function in the CameraEnemyTrackerScript
    public static void ChangeLockOn(float input)  {    CameraEnemyTrackerScript.ChangeLockOn(input);  }

    // Call the function in the CameraMovementScript
    public static void CenterCamera(){    CameraMovementScript.CenterCamera();}


    // Call the function in InputManagerScript
    public static bool GetJoystickInUse(){     return InputManagerScript.GetJoystickInUse();}
    public static float GetHorizontalInput(){    return InputManagerScript.GetHorizontalInput();}
    public static float GetVerticalInput(){    return InputManagerScript.GetVerticalInput();}
    public static float GetHorizontalCameraInput(){    return InputManagerScript.GetHorizontalCameraInput();}
    public static float GetVerticalCameraInput(){    return InputManagerScript.GetVerticalCameraInput();}
    public static bool GetLockOnButton(){    return InputManagerScript.GetLockOnButton();}

}
