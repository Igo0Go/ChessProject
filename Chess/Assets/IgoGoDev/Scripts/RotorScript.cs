using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotorScript : MonoBehaviour
{
    public Vector3 axis;
    [Range(0,20)]public float speed;

    void FixedUpdate()
    {
        transform.Rotate(axis, speed * Time.fixedDeltaTime);
    }
}
