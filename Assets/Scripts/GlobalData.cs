using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData
{
    //Variables to Lock Enemies
    public static List<Transform> LockableEnemies;
    public static bool EnemyLocked;
    public static Transform LockedEnemyTransform;


    //Player stats
    public static Transform PlayerTransform;


    //Function to release the enemy lock-on
    public static void UnlockEnemy()
    {
        LockedEnemyTransform = null;
        EnemyLocked = false;
    }
}
