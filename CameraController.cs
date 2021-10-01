using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/*
https://wiki.unity3d.com/index.php/MouseOrbitImproved
*/
[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class CameraController : MonoBehaviour {
 
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float zoomSpeed = 1.0f;
 
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
 
    public float distanceMin = .5f;
    public float distanceMax = 15f;
    
    public bool isInCar = false;
 
    private Rigidbody rigidbod;
    
    private Vector2 movement;
 
    private float x = 0.0f;
    private float y = 0.0f;
    
    private float scroll;
    
    private bool orbit = false;
    
    public void OnLook(InputValue input)
    {
        movement = input.Get<Vector2>();
    }
    
    public void OnThirdPersonZoom(InputValue input)
    {
        scroll = -input.Get<float>() * zoomSpeed;
    }
    
    public void OnChangeCamera()
    {
        orbit = !orbit;
    }
 
    // Use this for initialization
    void Start () 
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
 
        rigidbod = GetComponent<Rigidbody>();
 
        // Make the rigid body not change rotation
        if (rigidbod != null)
        {
            rigidbod.freezeRotation = true;
        }
        
        if(isInCar)
            transform.position = transform.parent.GetComponent<CarController>().driverCameraLocation.position;
    }
 
    void LateUpdate () 
    {
        if (target) 
        {
            distance = Mathf.Clamp(distance - scroll, distanceMin, distanceMax);
            
            x += movement.x * xSpeed * 0.02f;
            y -= movement.y * ySpeed * 0.02f;
 
            y = ClampAngle(y, yMinLimit, yMaxLimit);
 
            Quaternion rotation = Quaternion.Euler(y, x, 0);

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;
 
            if(orbit)
            {
                transform.rotation = rotation;
                transform.position = position;
            }
            
            else if(isInCar) 
            {
                transform.position = transform.parent.GetComponent<CarController>().driverCameraLocation.position;
                transform.localRotation = rotation;
            }
        }
    }
 
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}