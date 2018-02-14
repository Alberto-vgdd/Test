using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character2DCameraFollowScript : MonoBehaviour
 {
	public Transform targetTransform;
	public float smoothTime;
	private Vector3 currentVelocity;


	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		transform.position = Vector3.SmoothDamp(transform.position,targetTransform.position,ref currentVelocity,smoothTime);
	}
}
