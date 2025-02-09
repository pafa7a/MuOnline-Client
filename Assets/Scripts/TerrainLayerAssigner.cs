using UnityEngine;
using System.IO;

public class TerrainLayerAssigner : MonoBehaviour
{
    public TextAsset terrainDataFile; // Drag & drop the .txt file in the Inspector
    private Terrain terrain;
    private TerrainLayer[] terrainLayers;

    public void ApplyTerrainLayers()
    {
        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("❌ No Terrain component found!");
            return;
        }

        terrainLayers = terrain.terrainData.terrainLayers;
        if (terrainLayers == null || terrainLayers.Length == 0)
        {
            Debug.LogError("❌ No Terrain Layers found!");
            return;
        }

        if (terrainDataFile == null)
        {
            Debug.LogError("❌ No terrain data file assigned!");
            return;
        }

        string[] lines = terrainDataFile.text.Split('\n'); // Read file contents
        if (lines.Length <= 1)
        {
            Debug.LogError("❌ Terrain data file is empty or corrupt!");
            return;
        }

        int terrainSize = 256; // Map size is 256x256
        float[,,] splatmapData = new float[terrainSize, terrainSize, terrainLayers.Length];

        for (int i = 1; i < lines.Length; i++) // Skip header line
        {
            string[] parts = lines[i].Trim().Split(',');
            if (parts.Length != 5) continue;

            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            int layer1ID = int.Parse(parts[2]);
            int layer2ID = int.Parse(parts[3]);
            float alpha = float.Parse(parts[4]);

            int unityY = terrainSize - 1 - y; // Flip Y for Unity terrain
            int scaledX = x; // X remains the same

            // Ensure valid layer indices
            if (layer1ID >= terrainLayers.Length) continue; // Skip if invalid layer1
            if (layer2ID >= terrainLayers.Length) layer2ID = layer1ID; // Use layer1 if layer2 is invalid

            // Assign splatmap values
            splatmapData[unityY, scaledX, layer1ID] = 1.0f; // Always full alpha for layer1

            if (alpha > 0.0f) // Blend layer2 only when alpha is present
            {
                splatmapData[unityY, scaledX, layer1ID] = 1.0f - alpha;
                splatmapData[unityY, scaledX, layer2ID] = alpha;
            }
        }

        // Apply splatmap data to terrain
        terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
        Debug.Log("✅ Terrain layers successfully applied!");
    }
}
