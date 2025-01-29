using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private Transform player; // Reference to the Player object
    public Vector3 offset = new Vector3(0, 10, -10);   // Offset from the Player
    public float smoothSpeed = 1f; // Speed of the camera's smoothing
    public float scrollSensitivity = 2f; // Sensitivity of the scroll wheel
    public float minZoom = 5f;   // Minimum zoom distance
    public float maxZoom = 20f; // Maximum zoom distance
    public float rotationSpeed = 5f; // Speed of camera rotation
    public float minVerticalAngle = -30f; // Minimum vertical angle
    public float maxVerticalAngle = 15f;  // Maximum vertical angle

    private float currentYaw = -45f; // Current rotation around the Y-axis
    private float currentPitch = 0f; // Current rotation around the X-axis

    void LateUpdate()
    {
        // Find and follow the local player
        if (PlayerManager.Instance != null && PlayerManager.Instance.ownPlayerId != null)
        {
            GameObject localPlayer = GameObject.Find(PlayerManager.Instance.ownPlayerId);
            if (localPlayer != null)
            {
                player = localPlayer.transform;
            }
        }

        if (player == null)
        {
            return;
        }

        // Adjust the offset magnitude based on scroll input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        offset = offset.normalized * Mathf.Clamp(offset.magnitude - scroll * scrollSensitivity, minZoom, maxZoom);

        // Rotate the camera when the middle mouse button is pressed
        if (Input.GetMouseButton(2)) // 2 is the middle mouse button
        {
            float mouseX = Input.GetAxis("Mouse X"); // Horizontal mouse movement
            float mouseY = Input.GetAxis("Mouse Y"); // Vertical mouse movement

            currentYaw += mouseX * rotationSpeed; // Update the yaw (horizontal rotation)
            currentPitch -= mouseY * rotationSpeed; // Update the pitch (vertical rotation)
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle); // Clamp vertical angle
        }

        // Calculate the rotated offset
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0); // Combine vertical and horizontal rotation
        Vector3 rotatedOffset = rotation * offset;

        // Desired position of the camera
        Vector3 desiredPosition = player.position + rotatedOffset;

        // Smoothly interpolate between the camera's current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Update the camera's position
        transform.position = smoothedPosition;

        // Optionally, make the camera look at the Player
        transform.LookAt(player);
    }
}
