using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    private NavMeshAgent agent; // Reference to the NavMeshAgent
    private LineRenderer lineRenderer; // LineRenderer to visualize the path

    public Color pathColor = Color.green; // Color of the path line
    public float lineWidth = 0.2f; // Width of the path line

    void Start()
    {
        // Get the NavMeshAgent component attached to the player
        agent = GetComponent<NavMeshAgent>();

        // Add a LineRenderer component if not already attached
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material for the line
        lineRenderer.startColor = pathColor;
        lineRenderer.endColor = pathColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0; // No points initially
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
                // Set the NavMeshAgent's destination to the clicked point
                agent.SetDestination(new Vector3(Mathf.Round(hit.point.x), hit.point.y, Mathf.Round(hit.point.z)));
            }
        }

        // Update the visual path
        DrawPath();
    }

    void DrawPath()
    {
        // Get the path from the NavMeshAgent
        NavMeshPath path = agent.path;

        if (path.corners.Length < 2)
        {
            lineRenderer.positionCount = 0; // No path to draw
            return;
        }

        // Update the LineRenderer with the path's corner points
        lineRenderer.positionCount = path.corners.Length;
        for (int i = 0; i < path.corners.Length; i++)
        {
            lineRenderer.SetPosition(i, path.corners[i]);
        }
    }
}
