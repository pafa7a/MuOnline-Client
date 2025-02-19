using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement; // Add this at the top with other using statements

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public GameObject playerPrefab; // Prefab for players
    public string ownPlayerId; // ID of the local player
    public Dictionary<string, GameObject> players = new(); // All players in the scene
    public string CurrentSceneName { get; private set; } // Add this with other properties

    private Transform playersParent; // Parent object for all players

    // Add new field for ceiling objects
    private List<GameObject> _ceilingObjects = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CurrentSceneName = SceneManager.GetActiveScene().name;

            // Cache ceiling objects if we're in World1
            if (CurrentSceneName == "World1")
            {
                _ceilingObjects.AddRange(GameObject.FindGameObjectsWithTag("Ceiling"));
                Debug.Log($"Found {_ceilingObjects.Count} ceiling objects in World1");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create the "Players" parent object dynamically
        GameObject parent = new("Players");
        playersParent = parent.transform;
    }

    public void SpawnPlayer(string playerId, Vector3 position, Quaternion rotation, bool isLocal = false)
    {
        if (players.ContainsKey(playerId))
        {
            Debug.LogWarning($"Player {playerId} is already spawned.");
            return;
        }

        GameObject player = Instantiate(playerPrefab, position, rotation, playersParent);
        player.name = playerId;
        player.GetComponentInChildren<TextMeshPro>().text = playerId;

        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.avoidancePriority = isLocal ? 50 : 0;
        }

        players[playerId] = player;

        if (isLocal)
        {
            ownPlayerId = playerId;
            // Subscribe to terrain and zone updates for local player
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.OnTerrainAndZoneUpdate += OnLocalPlayerTerrainUpdate;
            }
        }

        Debug.Log($"Spawned {(isLocal ? "local" : "other")} player with ID: {playerId}");
    }

    private void OnLocalPlayerTerrainUpdate(object sender, PlayerMovement.TerrainUpdateEventArgs args)
    {
        if (string.IsNullOrEmpty(ownPlayerId)) return;

        // Handle ceiling objects in World1
        if (CurrentSceneName == "World1" && _ceilingObjects.Count > 0)
        {
            bool shouldDisable = args.TerrainLayer == "TileGround03";
            foreach (GameObject ceilingParent in _ceilingObjects)
            {
                if (ceilingParent == null) continue;

                // Find the child with the same name as the parent
                Transform child = ceilingParent.transform.Find(ceilingParent.name);
                if (child != null)
                {
                    child.gameObject.SetActive(!shouldDisable);
                }
            }
        }
    }

    public void UpdatePlayer(string playerId, Vector3 position)
    {
        if (!players.ContainsKey(playerId))
        {
            Debug.LogWarning($"Player {playerId} not found. Cannot update.");
            return;
        }

        GameObject player = players[playerId];
        player.transform.SetPositionAndRotation(position, player.transform.rotation);
        Debug.Log($"Updated player with ID: {playerId} to position {position} and rotation {player.transform.rotation}");
    }

    public void MovePlayer(string playerId, Vector3 position)
    {
        if (!players.ContainsKey(playerId))
        {
            return;
        }

        GameObject player = players[playerId];

        // Get the NavMeshAgent component attached to the player
        NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            return;
        }
        agent.SetDestination(position);

        // Clear the path if the agent has reached its destination
        if (!agent.pathPending && agent.remainingDistance == 0f)
        {
            agent.ResetPath();
        }
    }

    public void RemovePlayer(string playerId)
    {
        if (!players.ContainsKey(playerId))
        {
            Debug.LogWarning($"Player {playerId} not found. Cannot remove.");
            return;
        }

        if (playerId == ownPlayerId)
        {
            // Unsubscribe from events when removing local player
            PlayerMovement movement = players[playerId].GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.OnTerrainAndZoneUpdate -= OnLocalPlayerTerrainUpdate;
            }
        }

        Destroy(players[playerId]);
        players.Remove(playerId);

        Debug.Log($"Removed player with ID: {playerId}");
    }
}
