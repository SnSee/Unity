using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTest : MonoBehaviour
{
    private Vector3 lastMousePosition;
    private float rotateSensity = 0.1f;
    // Update is called once per frame

    private void RotateCamera()
    {
        Vector3 mouseMovement = new Vector3(Input.mousePosition.y - lastMousePosition.y, Input.mousePosition.x - lastMousePosition.x, 0);
        mouseMovement *= rotateSensity;
        transform.Rotate(mouseMovement, Space.World);
        lastMousePosition = Input.mousePosition;
    }

    void Update()
    {
        RotateCamera();
    }
}
