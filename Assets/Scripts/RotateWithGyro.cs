using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateWithGyro : MonoBehaviour
{
    InputAction rotationAction;

    void Start()
    {
        rotationAction = InputSystem.actions.FindAction("Rotation");
        InputSystem.EnableDevice(AttitudeSensor.current);
    }

    // Update is called once per frame
    void Update()
    {
        InputSystem.EnableDevice(AttitudeSensor.current);

        Quaternion rotation = rotationAction.ReadValue<Quaternion>();
        transform.rotation = Quaternion.Euler(90, 0, 0) * rotation;
    }
}