using UnityEngine;
using System.Collections;

public class FirstPersonCam : MonoBehaviour
{

    public Camera cam;
    public PreProcess processing;

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private void Start()
    {
        cam = Camera.main;
        processing = cam.GetComponent<PreProcess>();
    }
    void Update()
    {
        

        
        if (Input.GetKey(KeyCode.S))
        {

            Debug.Log("suo");
            Vector3 point = new Vector3();
            point = new Vector3(Camera.main.pixelWidth, 
                Camera.main.pixelHeight,0);
            Ray ray = Camera.main.ScreenPointToRay(point);
            Debug.Log("pw: "+Camera.main.pixelWidth/2+" ph: "+Camera.main.pixelHeight/2);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

            Quaternion c = Camera.main.transform.rotation;
            Quaternion rotation = Quaternion.LookRotation(ray.direction);
            Camera.main.transform.rotation = rotation;
        } 
        if(!Input.GetMouseButton(0))return;
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        
       
    }
}