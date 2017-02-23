using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgroScript : MonoBehaviour
{
    public Transform[] m_NearbyEnemies;

	// Use this for initialization
	void Start ()
    {
        m_NearbyEnemies = new Transform[10];

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider otherObject)
    {
        if (otherObject.tag.Contains("Enemy"))
        {
            m_NearbyEnemies[0] = otherObject.transform;
        }
    }

    void OnTriggerExit(Collider otherObject)
    {
        if (otherObject.tag.Contains("Enemy"))
        {
            m_NearbyEnemies[0] = null;
        }
    }
}
