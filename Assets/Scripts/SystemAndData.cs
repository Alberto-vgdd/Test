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


    // Call the function in the CameraEnemyTrackerScript
    public static void ChangeLockOn(float input)
    {
        CameraEnemyTrackerScript.ChangeLockOn(input);
    }

    // Call the function in the CameraMovementScript
    public static void CenterCamera()
    {
        CameraMovementScript.CenterCamera();
    }

}
