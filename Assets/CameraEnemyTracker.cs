using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEnemyTracker : MonoBehaviour 
{
    
	// Use this for initialization
    void Awake()
    {
        GlobalData.LockableEnemies = new LinkedList<Transform>();
    }
	
	// Update is called once per frame
	void Update () 
    {
        UpdateLockOnObjectives();
	}

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            GlobalData.LockableEnemies.AddLast(collider.transform);
        }

    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Enemy") && GlobalData.LockableEnemies.Contains(collider.transform))
        {
            GlobalData.LockableEnemies.Remove(collider.transform);
        }

    }

    void UpdateLockOnObjectives()
    {
        if (Input.GetButtonDown("Right Thumb"))
        {
            if (GlobalData.EnemyLocked)
            {
                GlobalData.EnemyLocked = false;
            }
            else
            {
                if (GlobalData.LockableEnemies.First != null )
                {
                    GlobalData.EnemyLocked = true;
                }
            }
        }

        if (GlobalData.EnemyLocked && GlobalData.LockableEnemies.First == null)
        {
            GlobalData.EnemyLocked = false;
        }
    }
}
