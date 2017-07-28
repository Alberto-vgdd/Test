using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEnemyTracker : MonoBehaviour 
{
    private CameraMovementScript m_CameraMovementScript;

    //Raycast to avoid locking enemies through walls
    private Ray m_EnemyToPlayerRay;


    void Start()
    {
        m_CameraMovementScript = GetComponent<CameraMovementScript>();
    }
        
    void Awake()
    {
        GlobalData.LockableEnemies = new List<Transform>();
    }
	
	void LateUpdate () 
    {
        UpdateLockOn();
	}
        
    void UpdateLockOn()
    {
        if (Input.GetButtonDown("Right Thumb"))
        {
            if (GlobalData.EnemyLocked)
            {
                m_CameraMovementScript.LockOn();
                GlobalData.UnlockEnemy();
            }
            else
            {
                if (NumberOfEnemies() != 0)
                {
                    LockClosestEnemy();
                }
                else
                {
                    m_CameraMovementScript.CenterCamera();
                }
            }
        }

        if (GlobalData.EnemyLocked && NumberOfEnemies() == 0)
        {
            m_CameraMovementScript.LockOn();
            GlobalData.UnlockEnemy();
        }
        else if (GlobalData.EnemyLocked && !ContainsEnemy(GlobalData.LockedEnemyTransform))
        {
            m_CameraMovementScript.LockOn();
            GlobalData.UnlockEnemy();
        }
        //else if the enemy is behind a wall for more than 2 secs.
    }


    void LockClosestEnemy()
    {
        float closestDistance = float.MaxValue;
        float enemyDistance;

        foreach (Transform enemy in GlobalData.LockableEnemies)
        {
            enemyDistance = Vector3.Distance(GlobalData.PlayerTransform.position,enemy.position);
            if (closestDistance > enemyDistance)
            {
                m_EnemyToPlayerRay.origin = enemy.position;
                m_EnemyToPlayerRay.direction = GlobalData.PlayerTransform.position - enemy.position;

                if (!Physics.Raycast(m_EnemyToPlayerRay, Vector3.Distance(GlobalData.PlayerTransform.position,enemy.position), (1 << LayerMask.NameToLayer("Environment"))))
                {
                    closestDistance = enemyDistance;
                    GlobalData.LockedEnemyTransform = enemy;
                }
            }
        }

        if (GlobalData.LockedEnemyTransform != null)
        {
            m_CameraMovementScript.LockOn();
            GlobalData.EnemyLocked = true;
        }
    }







    void AddEnemy(Transform enemy)
    {
        GlobalData.LockableEnemies.Add(enemy);
    }

    void RemoveEnemy(Transform enemy)
    {
        GlobalData.LockableEnemies.Remove(enemy);
    }

    bool ContainsEnemy(Transform enemy)
    {
        return GlobalData.LockableEnemies.Contains(enemy);
    }

    int NumberOfEnemies()
    {
        return GlobalData.LockableEnemies.Count;
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
