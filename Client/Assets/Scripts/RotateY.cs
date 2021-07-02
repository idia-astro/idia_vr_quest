using System;
using UnityEngine;

public class RotateY : MonoBehaviour
{
    public float rate = 1.0f;
    
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0, (float) (2.0f * Math.PI * rate * Time.time), 0); 
    }
}
