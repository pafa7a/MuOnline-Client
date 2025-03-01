using UnityEngine;

public class SelectServerGateHeightAnimation : MonoBehaviour
{
    private float originalY;
    private float targetY = -6f;
    private float detectionDistance = 9f;
    private float bufferZone = 4f; // Prevents rapid state changes
    private float transitionSpeed = 3f; // Adjust for faster/slower transitions
    private Camera mainCamera;
    private bool isLowered = false;
    private float currentTargetY;

    void Start()
    {
        originalY = transform.position.y;
        currentTargetY = originalY;
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null) return;

        float distance = Vector3.Distance(mainCamera.transform.position, transform.position);

        // Use buffer zone to prevent rapid state changes
        if (distance < detectionDistance - bufferZone && !isLowered)
        {
            currentTargetY = targetY;
            isLowered = true;
        }
        else if (distance > detectionDistance + bufferZone && isLowered)
        {
            currentTargetY = originalY;
            isLowered = false;
        }

        // Smoothly interpolate to target position
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, currentTargetY, Time.deltaTime * transitionSpeed);
        transform.position = pos;
    }
}
