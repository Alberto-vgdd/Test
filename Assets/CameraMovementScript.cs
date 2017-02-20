using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementScript : MonoBehaviour
{
    //Camera transform and Target transform
    private Transform m_CameraTranform;
    public Transform m_TargetTransform;

    //The distance between the Target and the Camera.
    public float m_CameraDistance;

    //These two variables are used to calculate the camera "Spherical" position around the player.
    private Vector3 m_CameraToPlayerDistance;
    private Quaternion m_CameraToPlayerRotation;

    //Inputs to control de camera
    private float m_HorizontalInput;
    private float m_VerticalInput;

    //The degrees per second the camera is going to turn.
    public float m_CameraRotationSpeed;

    //This is used to smooth Lerp between positions.
    public float m_CameraSmooth;

    //This values limit the Y axis of the camera
    public float m_MaxVerticalRotation;
    public float m_MinVerticalRotation;

    //Values that represents the amount of degrees in both Horizontal and Vertical axes
    private float  m_HorizontalRotation;
    private float m_VerticalRotation;


    //Raycast to shorten m_CameraToPlayerDistance in case of obstacle
    private RaycastHit m_CameraRayCastHit;
    private Ray m_CameraRay;


    void Start ()
    {
        m_CameraTranform = transform;

        m_HorizontalRotation = 0f;
        m_VerticalRotation = 15f;

    }

    void Update()
    {
        m_HorizontalInput = Input.GetAxis("CameraHorizontal");
        m_VerticalInput = Input.GetAxis("CameraVertical");
    }

	void LateUpdate ()
    {
        if (Mathf.Abs(m_HorizontalRotation) >= 360)
        {
            m_HorizontalRotation = 0f;
        }
        m_HorizontalRotation += m_HorizontalInput * m_CameraRotationSpeed * Time.deltaTime;
        m_VerticalRotation += m_VerticalInput * m_CameraRotationSpeed * Time.deltaTime;

        m_VerticalRotation = Mathf.Min(m_VerticalRotation, m_MaxVerticalRotation);
        m_VerticalRotation = Mathf.Max(m_VerticalRotation, m_MinVerticalRotation);


        m_CameraRay.direction = -m_CameraTranform.forward;
        m_CameraRay.origin = m_TargetTransform.position;

        if (Physics.Raycast(m_CameraRay, out m_CameraRayCastHit, m_CameraDistance))
        {
          m_CameraToPlayerDistance =  new Vector3(0f, 0f, -Mathf.Abs(Vector3.Magnitude(m_CameraRayCastHit.point - m_TargetTransform.position))+0.275f);
        }
        else
        {
            m_CameraToPlayerDistance  = Vector3.Lerp(m_CameraToPlayerDistance, new Vector3(0f, 0f, -m_CameraDistance), m_CameraSmooth);
        }
  




        m_CameraToPlayerRotation = Quaternion.Euler(m_VerticalRotation, m_HorizontalRotation, 0f);
        m_CameraTranform.position = m_TargetTransform.position +m_CameraToPlayerRotation * m_CameraToPlayerDistance;


        //para que la cámara no atraviese objetos se puede hacer un raycast con la dirección 
        //-forward de manera que si hay colision antes de el valor de m_CameraDistance,
        // el vector CameraDistanceToPlayer se ha de modificar


        m_CameraTranform.LookAt(m_TargetTransform);

    }
}
