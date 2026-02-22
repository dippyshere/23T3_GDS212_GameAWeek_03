using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOutOfBounds : MonoBehaviour
{
    public PlayerController playerControllerScript;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(CheckBounds), Random.Range(0f, 2f), 2);
    }

    private void CheckBounds()
    {
        if (!(transform.position.y < -0) && !(transform.position.x < -24) && !(transform.position.x > 24))
        {
            return;
        }

        playerControllerScript.RemoveTowerComponent(gameObject);
        Destroy(gameObject);
    }
}
