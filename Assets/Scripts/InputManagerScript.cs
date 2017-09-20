using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManagerScript : MonoBehaviour 
{
	[Header("Mouse/Keyboard Axes")]
	public string m_HorizontalMovementKeyboard;
	public string m_VerticalMovementKeyboard;
	public string m_HorizontalCameraMovementKeyboard;
	public string m_VerticalCameraMovementKeyboard;
	public string m_LockOnKeyboard;
	public string m_ChangeTargetKeyboard;



	[Header("Joystick Axes")]
	public string m_HorizontalMovementJoystick;
	public string m_VerticalMovementJoystick;
	public string m_HorizontalCameraMovementJoystick;
	public string m_VerticalCameraMovementJoystick;
	public string m_LockOnJoystick;
	public string m_ChangeTargetJoystick;




	// Variables the other scripts are going to use
	private bool m_JoystickInUse;
	private bool m_LockOnButton;
	private float m_ChangeTarget;
	private float m_HorizontalInput;
	private float m_VerticalInput;
	private float m_HorizontalCameraInput;
	private float m_VerticalCameraInput;



	void Start () 
	{
		SystemAndData.InputManagerScript = this;
	}
	
	void Update ()
	{
		m_JoystickInUse = IsJoystickInUse();
		
		if (m_JoystickInUse)
		{
			m_LockOnButton = Input.GetButtonDown(m_LockOnJoystick);
			m_ChangeTarget = Input.GetAxisRaw(m_ChangeTargetJoystick);
			m_HorizontalInput = Input.GetAxisRaw(m_HorizontalMovementJoystick);
			m_VerticalInput = Input.GetAxisRaw(m_VerticalMovementJoystick);
			m_HorizontalCameraInput = Input.GetAxis(m_HorizontalCameraMovementJoystick);
			m_VerticalCameraInput = Input.GetAxis(m_VerticalCameraMovementJoystick);
		}
		else
		{
			m_LockOnButton = Input.GetButtonDown(m_LockOnKeyboard);
			m_ChangeTarget = Input.GetAxisRaw(m_ChangeTargetKeyboard);
			m_HorizontalInput = Input.GetAxisRaw(m_HorizontalMovementKeyboard);
			m_VerticalInput = Input.GetAxisRaw(m_VerticalMovementKeyboard);
			m_HorizontalCameraInput = Input.GetAxis(m_HorizontalCameraMovementKeyboard);
			m_VerticalCameraInput = Input.GetAxis(m_VerticalCameraMovementKeyboard);
		}

	}

	bool IsJoystickInUse()
	{
		if(Input.GetKey(KeyCode.Joystick1Button0)  ||
		Input.GetKey(KeyCode.Joystick1Button1)  ||
		Input.GetKey(KeyCode.Joystick1Button2)  ||
		Input.GetKey(KeyCode.Joystick1Button3)  ||
		Input.GetKey(KeyCode.Joystick1Button4)  ||
		Input.GetKey(KeyCode.Joystick1Button5)  ||
		Input.GetKey(KeyCode.Joystick1Button6)  ||
		Input.GetKey(KeyCode.Joystick1Button7)  ||
		Input.GetKey(KeyCode.Joystick1Button8)  ||
		Input.GetKey(KeyCode.Joystick1Button9)  ||
		Input.GetKey(KeyCode.Joystick1Button10) ||
		Input.GetKey(KeyCode.Joystick1Button11) ||
		Input.GetKey(KeyCode.Joystick1Button12) ||
		Input.GetKey(KeyCode.Joystick1Button13) ||
		Input.GetKey(KeyCode.Joystick1Button14) ||
		Input.GetKey(KeyCode.Joystick1Button15) ||
		Input.GetKey(KeyCode.Joystick1Button16) ||
		Input.GetKey(KeyCode.Joystick1Button17) ||
		Input.GetKey(KeyCode.Joystick1Button18) ||
		Input.GetKey(KeyCode.Joystick1Button19) )
		{
			return true;
		}

		if(Input.GetAxis(m_HorizontalCameraMovementJoystick) != 0.0f ||
		Input.GetAxis(m_VerticalCameraMovementJoystick) != 0.0f ||
		Input.GetAxis(m_HorizontalMovementJoystick) != 0.0f ||
		Input.GetAxis(m_VerticalMovementJoystick) != 0.0f)
		{
			return true;
		}
		
		return false;
	}

	// Getters
	public bool GetJoystickInUse(){	return m_JoystickInUse;}
	public float GetHorizontalInput(){    return m_HorizontalInput;}
    public float GetVerticalInput(){    return m_VerticalInput;}
    public float GetHorizontalCameraInput(){    return m_HorizontalCameraInput;}
    public float GetVerticalCameraInput(){    return m_VerticalCameraInput;}
    public bool GetLockOnButton(){    return m_LockOnButton;}
	public float GetChangeTarget(){		return m_ChangeTarget;}
}
