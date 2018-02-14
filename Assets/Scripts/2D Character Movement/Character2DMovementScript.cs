using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character2DMovementScript : MonoBehaviour
{
	private string SPRITE_NAME = "Sprite";
	private string HORIZONTAL_AXIS = "HorizontalJoystick";
	private string TURN_TRIGGER = "turn";
	private string WALKING_BOOL = "walking";
	private float TURN_TIME = 0.5f;

	private Rigidbody2D rigidbody2d;
	private CapsuleCollider2D capsuleCollider2d;
	private SpriteRenderer spriteRenderer;
	private Animator animator;

	private bool walking;
	private bool facingLeft;
	private bool turning;
	private float turnTimer;

	private float horizontalInput;
	public float maxSpeed;
	private Vector2 targetVelocity;
	private Vector2 currentVelocity;



	



	void Awake()
	{
		rigidbody2d = GetComponent<Rigidbody2D>();
		capsuleCollider2d = GetComponent<CapsuleCollider2D>();
		

		Transform spriteTransform = transform.Find(SPRITE_NAME);
		spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
		animator = spriteTransform.GetComponent<Animator>();

	}


	void Start () 
	{
		walking = false;
		facingLeft = false;
		turning = false;

		horizontalInput = 0;
		targetVelocity = new Vector2(0,0);

	}
	
	void Update () 
	{
		horizontalInput = (!turning) ? Input.GetAxis(HORIZONTAL_AXIS): 0f;


		UpdateAnimation();

	}

	void FixedUpdate()
	{
		targetVelocity = Vector2.right*maxSpeed*horizontalInput  + Vector2.up*rigidbody2d.velocity.y;

		rigidbody2d.velocity = Vector2.SmoothDamp(rigidbody2d.velocity,targetVelocity, ref currentVelocity,0.25f,float.MaxValue,Time.fixedDeltaTime);

	}

	void UpdateAnimation()
	{
		// Flip the sprite according to the direction of the movement.
		if (!turning)
		{
		
			if ((facingLeft && horizontalInput > 0) || (!facingLeft && horizontalInput < 0))
			{
				if ( Mathf.Abs(rigidbody2d.velocity.x) > 0.5f*maxSpeed )
				{	
					turning = true;
					turnTimer = 0f;
					animator.SetTrigger(TURN_TRIGGER);

				}
				else
				{
					facingLeft = !facingLeft;
					spriteRenderer.flipX = facingLeft;
				}
			}
			
			
	
		}
		else
		{
			turnTimer += Time.deltaTime;
			if (turnTimer >= TURN_TIME)
			{
				turning = false;
				facingLeft = !facingLeft;
				spriteRenderer.flipX = facingLeft;
			}
		}

		// Enable/Disable the walking animation
		walking = (rigidbody2d.velocity.x != 0);
		animator.SetBool(WALKING_BOOL,walking);


		
	}


}
