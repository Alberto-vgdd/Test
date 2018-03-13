using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIScript : MonoBehaviour
{
	private Animator animator;

	private const string gameFadeOut = "GameFadeOut";
	private const string gameFadeIn = "GameFadeIn";
	
	// Use this for initialization
	void Awake ()
	{
		GlobalData.GameUIScript = this;
		animator = GetComponent<Animator>();
		
	}
	

	public void StartGameFadeOut()
	{
		animator.SetTrigger(gameFadeOut);
	}

	public void StartGameFadeIn()
	{
		animator.SetTrigger(gameFadeIn);
	}
}
