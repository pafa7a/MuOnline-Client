using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

[ExecuteInEditMode]
public class NonWalkableAreasGenerator : MonoBehaviour
{
    public List<TerrainLayer> nonWalkableLayers = new List<TerrainLayer>(); // References to non-walkable Terrain Layers
    public Vector3 volumeSize = new Vector3(1, 1, 1); // Size of each NavMeshModifierVolume
    public string subParentName = "NavMeshModifierVolumes"; // Name for the sub-parent GameObject

    public void GenerateModifierVolumes()
    {
        // Get the Terrain component
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("This script must be attached to a Terrain GameObject.");
            return;
        }

        if (nonWalkableLayers == null || nonWalkableLayers.Count == 0)
        {
            Debug.LogError("No non-walkable Terrain Layers are assigned.");
            return;
        }

        // Find or create the sub-parent GameObject
        Transform subParent = transform.Find(subParentName);
        if (subParent == null)
        {
            GameObject subParentObject = new GameObject(subParentName);
            subParentObject.transform.parent = transform;
            subParent = subParentObject.transform;
        }

        // Clear all existing children of the sub-parent
        for (int i = subParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(subParent.GetChild(i).gameObject);
        }

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;

        int resolution = terrainData.alphamapResolution;
        float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, resolution, resolution);
        TerrainLayer[] terrainLayers = terrainData.terrainLayers;

        // Process each non-walkable layer
        foreach (TerrainLayer layer in nonWalkableLayers)
        {
            int layerIndex = System.Array.IndexOf(terrainLayers, layer);
            if (layerIndex == -1)
            {
                Debug.LogWarning($"The layer {layer.name} is not assigned to the terrain.");
                continue;
            }

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    // Check if the alphamap value for the layer is above the threshold
                    if (alphamaps[y, x, layerIndex] > 0.5f) // Adjust threshold as needed
                    {
                        // Calculate the world position of the center of this cell
                        Vector3 position = new Vector3(
                            terrainPosition.x + x * (terrainData.size.x / resolution) + (terrainData.size.x / resolution) / 2,
                            terrainPosition.y,
                            terrainPosition.z + y * (terrainData.size.z / resolution) + (terrainData.size.z / resolution) / 2
                        );

                        // Create a NavMeshModifierVolume
                        GameObject volumeObject = new GameObject($"NavMeshModifierVolume_{layer.name}");
                        volumeObject.transform.position = position;
                        volumeObject.transform.localScale = volumeSize;

                        NavMeshModifierVolume modifierVolume = volumeObject.AddComponent<NavMeshModifierVolume>();
                        modifierVolume.size = volumeSize;
                        modifierVolume.area = NavMesh.GetAreaFromName("Not Walkable");

                        // Parent it to the sub-parent GameObject
                        volumeObject.transform.parent = subParent;
                    }
                }
            }
        }

        Debug.Log("NavMeshModifierVolumes regenerated.");
    }
}
