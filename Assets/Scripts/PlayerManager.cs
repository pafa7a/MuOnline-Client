using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public GameObject playerPrefab; // Prefab for players
    public string ownPlayerId; // ID of the local player
    public Dictionary<string, GameObject> players = new(); // All players in the scene

    private Transform playersParent; // Parent object for all players

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        }

        Debug.Log($"Spawned {(isLocal ? "local" : "other")} player with ID: {playerId}");
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

        Destroy(players[playerId]);
        players.Remove(playerId);

        Debug.Log($"Removed player with ID: {playerId}");
    }
}
