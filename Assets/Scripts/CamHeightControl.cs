using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slider = UnityEngine.UI.Slider;

public class CamHeightControl : MonoBehaviour
{
    public Slider slider;
    public float minHeight = -20.0f; // Minimum camera height
    public float maxHeight = 20.5f; // Maximum camera height
    // Start is called before the first frame update
    void Start()
    {
        //if (cameraTransform == null)
        //{
        //    cameraTransform = Camera.main.transform; // Default to main camera if not assigned
        //}

        slider.onValueChanged.AddListener(SetHeight);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveAllListeners();
    }

    void OnSliderValueChanged(float value)
    {
        transform.position = transform.position + new Vector3(0, value * 12, 0);
    }

    void SetHeight(float newHeight)
    {
        newHeight = newHeight * 12;

        //if (newHeight < 0.5)
        //{
        //    newHeight = -newHeight*12;
        //}
        //else
        //{
        //    newHeight = newHeight * 12;
        //}
        GameObject myObject = GameObject.Find("Camera Offset");
        // Clamp the new Y position to the specified range
        //var newYPosition = Mathf.Clamp(newHeight, minHeight, maxHeight);

        // Update the camera's Y position
        myObject.transform.position = new Vector3(myObject.transform.position.x, newHeight, myObject.transform.position.z);
    }
}