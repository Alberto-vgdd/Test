using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEnemyTrackerScript : MonoBehaviour 
{
    // Variables to Lock Enemies
    private static List<Transform> m_LockableEnemies;
    private Collider[] m_NearbyEnemyColliders;
    private bool m_EnemyLocked;

    // This Camera, used to switch between targets.
    private Camera m_PlayerCamera;

    [Header("Player Transform")]
    public Transform m_PlayerTransform;

    [Header("Lock On Parameters")]
    public float m_MaximumLockDistance;
    public float m_FixedStepMultiplier;
    private float m_TimeSinceLastRefresh;
    

    void Awake()
    {
        // Reference to GlobalData 
        SystemAndData.CameraEnemyTrackerScript = this;
        SystemAndData.PlayerCamera = m_PlayerCamera = GetComponent<Camera>();
        
        // Clear the enemy array.
        m_LockableEnemies = new List<Transform>();
    }

	void LateUpdate () 
    {
        UpdateLockOn();
	}

    void FixedUpdate()
    {
        if (SystemAndData.IsEnemyLocked)
        {
            m_TimeSinceLastRefresh += Time.fixedDeltaTime;
            if (m_TimeSinceLastRefresh >= m_FixedStepMultiplier*Time.fixedDeltaTime)
            {
                RefreshNearEnemies();
                m_TimeSinceLastRefresh = 0;
            }
            
        } 
    }
        
    void UpdateLockOn()
    {
        if (Input.GetButtonDown("Right Thumb"))
        {
            if (SystemAndData.IsEnemyLocked)
            {
                UnlockEnemy();
                
            }
            else
            {
                RefreshNearEnemies();
                LockEnemy();
            }
        }

        if (SystemAndData.IsEnemyLocked && m_LockableEnemies.Count == 0)
        {
            UnlockEnemy();
        }
        else if (SystemAndData.IsEnemyLocked && !m_LockableEnemies.Contains(SystemAndData.LockedEnemyTransform))
        {
            UnlockEnemy();
        }

        //else if the enemy is behind a wall for more than 2 secs.
    }

    // This code realeses the enemey locked on.
    void UnlockEnemy()
    {
        SystemAndData.LockedEnemyTransform = null;
        SystemAndData.IsEnemyLocked = false;
    }

    // Function that locks on the closest enemy available. If there are no enemies available, just center the camera.
    void LockEnemy()
    {
        if (m_LockableEnemies.Count != 0)
        {
            float closestDistance = float.MaxValue;
            float enemyDistance;

            foreach (Transform enemy in m_LockableEnemies)
            {
                enemyDistance = Vector3.Distance(m_PlayerTransform.position,enemy.position);
                if (closestDistance > enemyDistance)
                {
                    if (!Physics.Raycast(enemy.position,m_PlayerTransform.position - enemy.position , Vector3.Distance(SystemAndData.PlayerTransform.position,enemy.position), (1 << LayerMask.NameToLayer("Environment"))))
                    {
                        closestDistance = enemyDistance;
                        SystemAndData.LockedEnemyTransform = enemy;
                    }
                }
            }

            if (SystemAndData.LockedEnemyTransform != null)
            {
                SystemAndData.IsEnemyLocked = true;
            }
            else
            {
                SystemAndData.CenterCamera();
            }
        }
        else
        {
            SystemAndData.CenterCamera();
        }
    }

    // This function updates the lockable enemeies by casting a sphere.
    void RefreshNearEnemies()
    {
        m_LockableEnemies = new List<Transform>();
        m_NearbyEnemyColliders =  Physics.OverlapSphere(SystemAndData.PlayerTransform.position,m_MaximumLockDistance,(1 << LayerMask.NameToLayer("Enemies")));
       
        foreach(Collider enemyCollider in m_NearbyEnemyColliders)
        {
             m_LockableEnemies.Add(enemyCollider.transform);
        }
    }


    // Move to the closest enemy on the screen (given the direction)
    public void ChangeLockOn(float input)
    {
        Transform newLockedEnemy = SystemAndData.LockedEnemyTransform;
        float DistanceToPreviousEnemy = 999*Mathf.Sign(input);
        
        if (Mathf.Sign(input) > 0)
        {
            foreach(Transform enemy in m_LockableEnemies)
            {
                if (m_PlayerCamera.WorldToViewportPoint(SystemAndData.LockedEnemyTransform.position).x < m_PlayerCamera.WorldToViewportPoint(enemy.position).x && m_PlayerCamera.WorldToViewportPoint(enemy.position).x < DistanceToPreviousEnemy )
                {
                    newLockedEnemy = enemy;
                    DistanceToPreviousEnemy = m_PlayerCamera.WorldToViewportPoint(enemy.position).x;
                }
            }
        }
        else if (Mathf.Sign(input) < 0)
        {
            foreach(Transform enemy in m_LockableEnemies)
            {
                if (m_PlayerCamera.WorldToViewportPoint(SystemAndData.LockedEnemyTransform.position).x > m_PlayerCamera.WorldToViewportPoint(enemy.position).x && m_PlayerCamera.WorldToViewportPoint(enemy.position).x > DistanceToPreviousEnemy )
                {
                    newLockedEnemy = enemy;
                    DistanceToPreviousEnemy = m_PlayerCamera.WorldToViewportPoint(enemy.position).x;
                }
            }
        }

        SystemAndData.LockedEnemyTransform = newLockedEnemy;
    }

}
