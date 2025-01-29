using GameServerProto;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    private NavMeshAgent agent; // Reference to the NavMeshAgent
    private LineRenderer lineRenderer; // LineRenderer to visualize the path

    private Color ownPlayerColor = Color.green;
    private Color otherPlayerColor = Color.red;
    public float lineWidth = 0.2f; // Width of the path line
    private bool isLocalPlayer = false; // Indicates whether this is the local player
    private float lastRequestTime = 0f; // Time when the last request was sent
    public float requestInterval = 0.2f; // Minimum time between requests in seconds

    void Start()
    {
        // Get the NavMeshAgent component attached to the player
        agent = GetComponent<NavMeshAgent>();
        // Check if this GameObject is the local player
        if (PlayerManager.Instance != null && PlayerManager.Instance.ownPlayerId == gameObject.name)
        {
            isLocalPlayer = true;
        }

        // Add a LineRenderer component if not already attached
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material for the line
        lineRenderer.startColor = isLocalPlayer ? ownPlayerColor : otherPlayerColor;
        lineRenderer.endColor = isLocalPlayer ? ownPlayerColor : otherPlayerColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0; // No points initially
    }

    void Update()
    {
        // Only allow input and path drawing for the local player
        if (isLocalPlayer)
        {
            HandleInput();
        }
        DrawPath();
    }

    void HandleInput()
    {
        // Check for mouse click or hold
        if (Input.GetMouseButton(0))
        {
            // Only send a request if the minimum interval has passed
            if (Time.time - lastRequestTime >= requestInterval)
            {
                // Raycast to find the clicked point on the ground
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // Prepare and send the WalkRequest
                    WalkRequest walkRequest = new()
                    {
                        X = Mathf.Round(hit.point.x),
                        Y = Mathf.Round(hit.point.y),
                        Z = Mathf.Round(hit.point.z),
                    };

                    Wrapper wrapper = new()
                    {
                        Type = "WalkRequest",
                        Payload = walkRequest.ToByteString()
                    };

                    WebSocketClient.Send(wrapper.ToByteArray());

                    // Update the time of the last request
                    lastRequestTime = Time.time;
                }
            }
        }
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
