using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithAccelerometer : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(Input.acceleration.y * 90, 0, -Input.acceleration.x * 90);
    }
}
