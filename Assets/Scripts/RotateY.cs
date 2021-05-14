using System;
using UnityEngine;

public class RotateY : MonoBehaviour
{
    public float rate = 1.0f;
    void Update()
    {
        transform.Rotate(Vector3.up, (float)(2.0f*Math.PI * rate * Time.deltaTime));
    }
}
