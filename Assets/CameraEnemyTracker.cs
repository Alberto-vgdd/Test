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
}
