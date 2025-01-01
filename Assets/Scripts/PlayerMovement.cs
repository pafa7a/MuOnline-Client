using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  public float speed = 5f;
  private Vector3 targetPosition;
  private bool isMoving = false;

  void Start()
  {
    // Initialize the target position to the current position
    targetPosition = transform.position;
  }

  void Update()
  {
    // Check for mouse click or hold
    if (Input.GetMouseButton(0))
    {
      // Raycast to find the clicked point on the ground
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out RaycastHit hit))
      {
        // Calculate the difference between the current position and the clicked position
        Vector3 difference = new Vector3(
            Mathf.Abs(hit.point.x - transform.position.x),
            0,
            Mathf.Abs(hit.point.z - transform.position.z)
        );

        // Check if the click is at least 1 unit away on X or Z
        if (difference.x >= 1f || difference.z >= 1f)
        {
          // Update the target position, rounding X and Z to the nearest integers
          targetPosition = new Vector3(
              Mathf.Round(hit.point.x),   // Round X to the nearest whole number
              transform.position.y,      // Preserve the current Y position
              Mathf.Round(hit.point.z)   // Round Z to the nearest whole number
          );
          isMoving = true;
        }
      }
    }

    // Move the player towards the target position
    if (isMoving)
    {
      // Move the object directly towards the target, snapping to the rounded position
      transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

      // Stop moving when the target is reached
      if (transform.position == targetPosition)
      {
        isMoving = false;
      }
    }
  }
}
