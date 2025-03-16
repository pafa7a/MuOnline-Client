using GameServerProto;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;

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
    private int safeZoneArea; // Stores the default speed of the NavMeshAgent

    private CanvasManager _canvas;
    private bool _shouldDisplayPlayerCoordinates = false;

    // Add these new cached variables at the top with other private fields
    private Terrain _cachedTerrain;
    private TerrainData _cachedTerrainData;
    private TerrainLayer[] _cachedTerrainLayers;
    private Vector3 _cachedTerrainPosition;
    private float _cachedAlphamapWidth;
    private float _cachedAlphamapHeight;

    // Remove _terrainCheckInterval and _nextTerrainCheck
    // Remove _navMeshCheckInterval and _nextNavMeshCheck
    // Add these new frame-based check variables
    private const int CHECK_INTERVAL_FRAMES = 30;
    private int _currentFrame;
    private string _cachedTerrainLayerResult = "Unknown";
    private string _cachedNavMeshAreaResult = "Unknown";

    // Add event and args cache
    public class TerrainUpdateEventArgs : System.EventArgs
    {
        public string TerrainLayer { get; set; }
        public string ZoneType { get; set; }
    }
    
    public event System.EventHandler<TerrainUpdateEventArgs> OnTerrainAndZoneUpdate;
    private readonly TerrainUpdateEventArgs _updateArgs = new();

    // Add these new tracking variables after other private fields
    private string _previousTerrainLayer = "Unknown";
    private string _previousNavMeshArea = "Unknown";

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

        // Add this to the existing Start method
        CacheTerrainData();
    }

    void Update()
    {
        _currentFrame++;

        // Check every 30 frames
        if (_currentFrame % CHECK_INTERVAL_FRAMES == 0)
        {
            string currentTerrainLayer = GetCurrentTerrainLayer();
            string currentNavMeshArea = GetCurrentNavMeshAreaName();
            
            // Only trigger event if there's a change
            if (currentTerrainLayer != _previousTerrainLayer || currentNavMeshArea != _previousNavMeshArea)
            {
                _cachedTerrainLayerResult = currentTerrainLayer;
                _cachedNavMeshAreaResult = currentNavMeshArea;
                
                _updateArgs.TerrainLayer = _cachedTerrainLayerResult;
                _updateArgs.ZoneType = _cachedNavMeshAreaResult;
                OnTerrainAndZoneUpdate?.Invoke(this, _updateArgs);
                
                // Update previous values
                _previousTerrainLayer = currentTerrainLayer;
                _previousNavMeshArea = currentNavMeshArea;
            }
        }

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
                    Vector3 hitPoint = hit.point;
                    
                    // Check if the point is within terrain bounds
                    Vector3 terrainPosition = hitPoint - _cachedTerrainPosition;
                    float normalizedX = terrainPosition.x / _cachedTerrainData.size.x;
                    float normalizedZ = terrainPosition.z / _cachedTerrainData.size.z;

                    // Skip if out of bounds
                    if (normalizedX < 0 || normalizedX > 1 || normalizedZ < 0 || normalizedZ > 1)
                        return;

                    // Also verify the point is on the NavMesh
                    if (NavMesh.SamplePosition(hitPoint, out NavMeshHit navHit, 3.0f, NavMesh.AllAreas))
                    {
                        // Prepare and send the WalkRequest using the validated position
                        WalkRequest walkRequest = new()
                        {
                            X = Mathf.Round(navHit.position.x),
                            Y = Mathf.Round(navHit.position.y),
                            Z = Mathf.Round(navHit.position.z),
                        };

                        Wrapper wrapper = new()
                        {
                            Type = "WalkRequest",
                            Payload = walkRequest.ToByteString()
                        };

                        WebSocketClient.Send(wrapper.ToByteArray());
                        lastRequestTime = Time.time;
                    }
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

        // Update zone info every CHECK_INTERVAL_FRAMES
        if (_currentFrame % CHECK_INTERVAL_FRAMES == 0)
        {
            _cachedNavMeshAreaResult = GetCurrentNavMeshAreaName();
        }

        if (_cachedNavMeshAreaResult == "Non-PvP")
        {
            agent.speed = Mathf.Max(1f, defaultSpeed * 0.9f);
        }
        else
        {
            agent.speed = defaultSpeed;
        }
    }

    // Extracts the correct NavMesh Area
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
            _canvas.PlayerCoordinatesObject.text = $"Player: X: {gameObject.transform.position.x:F0}, Y: {gameObject.transform.position.z:F0}\nZone: {_cachedNavMeshAreaResult}\nTerrain layer: {_cachedTerrainLayerResult}\nSpeed: {agent.speed:F1}";
        }
    }

    // Get the area name from NavMesh position
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

    private void CacheTerrainData()
    {
        _cachedTerrain = Terrain.activeTerrain;
        if (_cachedTerrain != null)
        {
            _cachedTerrainData = _cachedTerrain.terrainData;
            _cachedTerrainLayers = _cachedTerrainData.terrainLayers;
            _cachedTerrainPosition = _cachedTerrain.transform.position;
            _cachedAlphamapWidth = _cachedTerrainData.alphamapWidth;
            _cachedAlphamapHeight = _cachedTerrainData.alphamapHeight;
        }
    }

    string GetCurrentTerrainLayer()
    {
        if (_cachedTerrain == null || _cachedTerrainData == null)
            return "Unknown";

        // Calculate normalized position
        Vector3 terrainPosition = transform.position - _cachedTerrainPosition;
        float normalizedX = terrainPosition.x / _cachedTerrainData.size.x;
        float normalizedZ = terrainPosition.z / _cachedTerrainData.size.z;

        // Early bounds check
        if (normalizedX < 0 || normalizedX > 1 || normalizedZ < 0 || normalizedZ > 1)
            return "Out of Bounds";

        // Convert to alphamap coordinates
        int alphamapX = Mathf.Clamp(Mathf.FloorToInt(normalizedX * _cachedAlphamapWidth), 0, (int)_cachedAlphamapWidth - 1);
        int alphamapZ = Mathf.Clamp(Mathf.FloorToInt(normalizedZ * _cachedAlphamapHeight), 0, (int)_cachedAlphamapHeight - 1);

        // Get splatmap data only for the exact point we need
        float[,,] splatmapData = _cachedTerrainData.GetAlphamaps(alphamapX, alphamapZ, 1, 1);

        // Find dominant texture using a more efficient loop
        int dominantIndex = 0;
        float strongestWeight = splatmapData[0, 0, 0];

        for (int i = 1; i < _cachedTerrainLayers.Length; i++)
        {
            float weight = splatmapData[0, 0, i];
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                dominantIndex = i;
            }
        }

        return _cachedTerrainLayers[dominantIndex].name;
    }
}
