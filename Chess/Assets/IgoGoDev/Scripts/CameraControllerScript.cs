using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerScript : MonoBehaviour
{
    public Transform cam;
    [Range(0,360)]public float speed;
    [Range(0,180)] public float maxVerticalAngle = 85;
    [Range(-180, 0)] public float minVerticalAngle = 0;

    private float currentVert, currentHor;
    private float horizontal, vertical;
    private bool rotate;


    void Start()
    {
        currentVert = transform.rotation.eulerAngles.x;
        currentHor = transform.rotation.eulerAngles.y;
    }

    void Update()
    {
        InputPC();
    }

    private void LateUpdate()
    {
        Rotate();
    }

    private void InputPC()
    {
        rotate = Input.GetMouseButton(1);
        horizontal = Input.GetAxis("Mouse X");
        vertical = Input.GetAxis("Mouse Y");
    }

    private void Rotate()
    {
        if(rotate)
        {
            if (horizontal != 0 || vertical != 0)
            {
                currentHor += horizontal * speed * Time.deltaTime;
                currentVert -= vertical * speed * Time.deltaTime;
                currentVert = Mathf.Clamp(currentVert, minVerticalAngle, maxVerticalAngle);
                transform.rotation = Quaternion.Euler(new Vector3(currentVert, currentHor, 0));
                cam.LookAt(transform.position);
            }
        }
    }
}
