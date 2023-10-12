using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithGyro : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Input.gyro.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion gyro = Input.gyro.attitude;
        gyro = Quaternion.Euler(90, 0, 0) * new Quaternion(-gyro.x, -gyro.y, gyro.z, gyro.w);
        transform.rotation = gyro;
    }
}