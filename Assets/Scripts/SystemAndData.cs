using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SystemAndData 
{
    // Variables to Lock Enemies
    public static List<Transform> LockableEnemies;
    public static bool EnemyLocked;
    public static Transform LockedEnemyTransform;

    // Player and Camera Transform
    public static Transform PlayerTransform;
    public static Camera m_PlayerCamera;

    // Camera Transform and Scripts
    public static CameraMovementScript m_CameraMovementScript;
    public static CameraEnemyTrackerScript m_CameraEnemyTrackerScript;



    // Function to release the enemy lock-on
    public static void UnlockEnemy()
    {
        LockedEnemyTransform = null;
        EnemyLocked = false;

        m_CameraMovementScript.LockOn();
    }

    // Function that locks on the closest enemy available. If there are no enemies available, just center the camera.
    public static void LockEnemy()
    {
        if (LockableEnemies.Count != 0)
        {
            float closestDistance = float.MaxValue;
            float enemyDistance;

            foreach (Transform enemy in LockableEnemies)
            {
                enemyDistance = Vector3.Distance(PlayerTransform.position,enemy.position);
                if (closestDistance > enemyDistance)
                {
                    if (!Physics.Raycast(enemy.position,PlayerTransform.position - enemy.position , Vector3.Distance(SystemAndData.PlayerTransform.position,enemy.position), (1 << LayerMask.NameToLayer("Environment"))))
                    {
                        closestDistance = enemyDistance;
                        LockedEnemyTransform = enemy;
                    }
                }
            }

            if (LockedEnemyTransform != null)
            {
                EnemyLocked = true;
                m_CameraMovementScript.LockOn();
            }
            else
            {
                m_CameraMovementScript.CenterCamera();
            }
        }
        else
        {
            m_CameraMovementScript.CenterCamera();
        }
        
    }

    // Move to the closest enemy on the screen (given the direction)
    public static void ChangeLockOn(float input)
    {
        Transform newLockedEnemy = LockedEnemyTransform;
        float DistanceToPreviousEnemy = 999*Mathf.Sign(input);
        
        if (Mathf.Sign(input) > 0)
        {
            foreach(Transform enemy in LockableEnemies)
            {
                if (m_PlayerCamera.WorldToViewportPoint(LockedEnemyTransform.position).x < m_PlayerCamera.WorldToViewportPoint(enemy.position).x && m_PlayerCamera.WorldToViewportPoint(enemy.position).x < DistanceToPreviousEnemy )
                {
                    newLockedEnemy = enemy;
                    DistanceToPreviousEnemy = m_PlayerCamera.WorldToViewportPoint(enemy.position).x;
                }
            }
        }
        else if (Mathf.Sign(input) < 0)
        {
            foreach(Transform enemy in LockableEnemies)
            {
                if (m_PlayerCamera.WorldToViewportPoint(LockedEnemyTransform.position).x > m_PlayerCamera.WorldToViewportPoint(enemy.position).x && m_PlayerCamera.WorldToViewportPoint(enemy.position).x > DistanceToPreviousEnemy )
                {
                    newLockedEnemy = enemy;
                    DistanceToPreviousEnemy = m_PlayerCamera.WorldToViewportPoint(enemy.position).x;
                }
            }
        }

        LockedEnemyTransform = newLockedEnemy;
    }
}
