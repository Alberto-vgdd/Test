using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEnemyTrackerScript : MonoBehaviour 
{
    void Awake()
    {
        // Reference to GlobalData 
        SystemAndData.m_CameraEnemyTrackerScript = this;
        SystemAndData.m_PlayerCamera = GetComponent<Camera>();
        
        // Clear the enemy array.
        SystemAndData.LockableEnemies = new List<Transform>();
    }

	void LateUpdate () 
    {
        UpdateLockOn();
	}
        
    void UpdateLockOn()
    {
        if (Input.GetButtonDown("Right Thumb"))
        {
            if (SystemAndData.EnemyLocked)
            {
                SystemAndData.UnlockEnemy();
                
            }
            else
            {
                SystemAndData.LockEnemy();
            }
        }

        if (SystemAndData.EnemyLocked && NumberOfEnemies() == 0)
        {
            SystemAndData.UnlockEnemy();
        }
        else if (SystemAndData.EnemyLocked && !ContainsEnemy(SystemAndData.LockedEnemyTransform))
        {
            SystemAndData.UnlockEnemy();
        }

        //else if the enemy is behind a wall for more than 2 secs.
    }


    void AddEnemy(Transform enemy)
    {
        SystemAndData.LockableEnemies.Add(enemy);
    }

    void RemoveEnemy(Transform enemy)
    {
        SystemAndData.LockableEnemies.Remove(enemy);
    }

    bool ContainsEnemy(Transform enemy)
    {
        return SystemAndData.LockableEnemies.Contains(enemy);
    }

    int NumberOfEnemies()
    {
        return SystemAndData.LockableEnemies.Count;
    }





        
    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            AddEnemy(collider.transform);
        }

    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Enemy") && ContainsEnemy(collider.transform) )
        {
            RemoveEnemy(collider.transform);
        }

    }


}
