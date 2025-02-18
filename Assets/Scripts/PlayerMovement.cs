using GameServerProto;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

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

    private float defaultSpeed; // Stores the default speed of the NavMeshAgent
    private int safeZoneArea; // Stores the NavMesh area index for SafeZone

    private CanvasManager _canvas;
    private bool _shouldDisplayPlayerCoordinates = false;

    void Start()
    {
        // Get the NavMeshAgent component attached to the player
        agent = GetComponent<NavMeshAgent>();
        defaultSpeed = 4f; // Store the default speed

        // Get the SafeZone area index
        safeZoneArea = NavMesh.GetAreaFromName("SafeZone");

        // Check if this GameObject is the local player
        if (PlayerManager.Instance != null && PlayerManager.Instance.ownPlayerId == gameObject.name)
        {
            isLocalPlayer = true;
            // Display coordinates only in World scene.
            Scene scene = SceneManager.GetActiveScene();
            _canvas = CanvasManager.Instance;
            if (_canvas && scene != null && scene.name != "ServerSelect")
            {
                _shouldDisplayPlayerCoordinates = true;
            }
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
        if (isLocalPlayer)
        {
            HandleInput();
            DisplayPlayerCoordinatesInCanvas();
        }

        DrawPath();
        AdjustSpeedForSafeZone();
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

    void AdjustSpeedForSafeZone()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"⚠️ Player is NOT on the NavMesh at {transform.position}!");
            return;
        }

        // ✅ Get the current NavMesh Area
        string zoneName = GetCurrentNavMeshAreaName();

        if (zoneName == "Non-PvP")
        {
            agent.speed = Mathf.Max(1f, defaultSpeed * 0.9f); // Reduce speed to 90%
        }
        else
        {
            agent.speed = defaultSpeed;
        }
    }

    // ✅ Extracts the correct NavMesh Area
    int GetAreaFromHit(NavMeshHit hit)
    {
        for (int i = 0; i < 32; i++)
        {
            if ((hit.mask & (1 << i)) != 0)
            {
                return i;
            }
        }
        return 0; // Default Walkable
    }

    void OnDestroy()
    {
        if (_canvas != null)
        {
            _canvas.PlayerCoordinatesObject.gameObject.SetActive(false);
        }
    }

    void DisplayPlayerCoordinatesInCanvas()
    {
        if (_shouldDisplayPlayerCoordinates)
        {
            _canvas.PlayerCoordinatesObject.gameObject.SetActive(_canvas.PlayerCoordinatesDisplay);

            // Get current NavMesh area
            string zoneName = GetCurrentNavMeshAreaName();

            _canvas.PlayerCoordinatesObject.text = $"Player: X: {gameObject.transform.position.x:F0}, Y: {gameObject.transform.position.z:F0}\nZone: {zoneName}\nSpeed: {agent.speed:F1}";
        }
    }

    // ✅ Function to get the area name from NavMesh position
    string GetCurrentNavMeshAreaName()
    {
        if (!agent.isOnNavMesh) return "Unknown";

        NavMeshHit hit;
        Vector3 checkPosition = transform.position + Vector3.down * 0.1f;

        if (!NavMesh.SamplePosition(checkPosition, out hit, 3.0f, NavMesh.AllAreas))
        {
            return "Unknown";
        }

        int currentArea = GetAreaFromHit(hit);

        if (currentArea == safeZoneArea) return "Non-PvP";

        return currentArea == 0 ? "PvP" : $"Area {currentArea}";
    }
}
