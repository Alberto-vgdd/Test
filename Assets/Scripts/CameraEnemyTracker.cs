﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEnemyTracker : MonoBehaviour 
{
    private CameraMovementScript m_CameraMovementScript;


    void Start()
    {
        m_CameraMovementScript = GetComponent<CameraMovementScript>();
    }


    void Awake()
    {
        GlobalData.LockableEnemies = new List<Transform>();
    }
	

	void Update () 
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
                GlobalData.EnemyLocked = false;
            }
            else
            {
                if (NumberOfEnemies() != 0)
                {
                    LockClosestEnemy();
                    m_CameraMovementScript.LockOn();
                    GlobalData.EnemyLocked = true;
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
            GlobalData.EnemyLocked = false;
        }
        else if (GlobalData.EnemyLocked && !ContainsEnemy(GlobalData.LockedEnemyTransform))
        {
            m_CameraMovementScript.LockOn();
            GlobalData.EnemyLocked = false;
        }
    }


    void LockClosestEnemy()
    {
        float closestDistance = float.MaxValue;
        float enemyDistance;

        foreach (Transform enemy in GlobalData.LockableEnemies)
        {
            enemyDistance = Mathf.Abs((GlobalData.PlayerTransform.position - enemy.position).magnitude);
            if (closestDistance > enemyDistance)
            {
                closestDistance = enemyDistance;
                GlobalData.LockedEnemyTransform = enemy;
            }
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
