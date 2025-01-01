using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player; // Reference to the Player object
    public Vector3 offset = new Vector3(0, 10, -10);   // Offset from the Player
    public float smoothSpeed = 1f; // Speed of the camera's smoothing
    public float scrollSensitivity = 2f; // Sensitivity of the scroll wheel
    public float minZoom = 5f;   // Minimum zoom distance
    public float maxZoom = 20f; // Maximum zoom distance

    void LateUpdate()
    {
        // Adjust the offset magnitude based on scroll input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        offset = offset.normalized * Mathf.Clamp(offset.magnitude - scroll * scrollSensitivity, minZoom, maxZoom);

        if (player != null)
        {
            // Desired position of the camera
            Vector3 desiredPosition = player.position + offset;

            // Smoothly interpolate between the camera's current position and the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Update the camera's position
            transform.position = smoothedPosition;

            // Optionally, make the camera look at the Player
            transform.LookAt(player);
        }
    }
}
