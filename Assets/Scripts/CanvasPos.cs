using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasPos : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform cameraTransform; // Reference to the camera's transform
    public float distanceFromCamera = 2.0f; // Distance from the camera

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform; // Default to main camera if not assigned
        }
    }

    void Update()
    {
        // Update the position of the canvas to be in front of the camera
        Vector3 newPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera - new Vector3(0, 0.6f, 0);
        transform.position = newPosition;

        // Update the rotation of the canvas to face the camera
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }
}
