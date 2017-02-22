using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementScript : MonoBehaviour
{
    //Camera transform and Target transform
    private Transform m_CameraTranform;
    public Transform m_CameraTargetTransform;

    //The distance between the Target and the Camera.
    public float m_CameraDistance;
    public float m_CameraAvoidClippingDistance;
    private Vector3 m_CameraToTargetDefaultVector;

    //These two variables are used to calculate the camera "Spherical" position around the player.
    private Vector3 m_CameraToTargetVector;
    private Quaternion m_CameraToTargetRotation;

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
    private float m_HitToTargetDistance;

    void Start ()
    {
        m_CameraTranform = transform;
        m_CameraToTargetDefaultVector = new Vector3(0f, 0f, m_CameraDistance);
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

        UpdateRotations();



        

        m_CameraRay.direction = -m_CameraTranform.forward;
        m_CameraRay.origin = m_CameraTargetTransform.position;


        if (Physics.Raycast(m_CameraRay, out m_CameraRayCastHit, m_CameraDistance))
        {
            m_HitToTargetDistance = Mathf.Abs(Vector3.Magnitude(m_CameraRayCastHit.point - m_CameraTargetTransform.position));
            m_CameraToTargetVector = -m_CameraToTargetDefaultVector.normalized * m_HitToTargetDistance * m_CameraAvoidClippingDistance;
        }
        else
        {
            m_CameraToTargetVector  = -m_CameraToTargetDefaultVector;
        }
  
        m_CameraToTargetRotation = Quaternion.Euler(m_VerticalRotation, m_HorizontalRotation, 0f);
        m_CameraTranform.position = m_CameraTargetTransform.position +m_CameraToTargetRotation * m_CameraToTargetVector;



        m_CameraTranform.LookAt(m_CameraTargetTransform);


    }

    void UpdateRotations()
    {
     
        m_HorizontalRotation += m_HorizontalInput * m_CameraRotationSpeed * Time.deltaTime;
        m_VerticalRotation += m_VerticalInput * m_CameraRotationSpeed * Time.deltaTime;

        m_VerticalRotation = Mathf.Min(m_VerticalRotation, m_MaxVerticalRotation);
        m_VerticalRotation = Mathf.Max(m_VerticalRotation, m_MinVerticalRotation);

        if (Mathf.Abs(m_HorizontalRotation) >= 360)
        {
            m_HorizontalRotation = 0f;
        }
    }
}
