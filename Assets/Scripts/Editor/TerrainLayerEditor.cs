using UnityEngine;
using UnityEditor;
using System.IO;

public class TerrainLayerAssigner : MonoBehaviour
{
    private Terrain terrain;
    private TerrainLayer[] terrainLayers;

    [MenuItem("Assets/MuOnline/Apply Terrain Layers", false, 1004)]
    private static void ApplyTerrainLayersFromFile()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".txt"))
        {
            Debug.LogError("❌ Please select a valid EncTerrainLayers.txt file.");
            return;
        }

        TerrainLayerAssigner script = new TerrainLayerAssigner();
        script.ApplyTerrainLayers(path);
    }

    public void ApplyTerrainLayers(string filePath)
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("❌ No active terrain found!");
            return;
        }

        terrainLayers = terrain.terrainData.terrainLayers;
        if (terrainLayers == null || terrainLayers.Length == 0)
        {
            Debug.LogError("❌ No Terrain Layers found!");
            return;
        }

        if (!File.Exists(filePath))
        {
            Debug.LogError("❌ Terrain data file not found!");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1)
        {
            Debug.LogError("❌ Terrain data file is empty or corrupt!");
            return;
        }

        int terrainSize = 256; // Raw map size
        int splatmapResolution = terrain.terrainData.alphamapResolution; // Unity's splatmap size
        float[,,] splatmapData = new float[splatmapResolution, splatmapResolution, terrainLayers.Length];

        Debug.Log($"📏 Terrain Size: {terrainSize}x{terrainSize}, Splatmap Resolution: {splatmapResolution}x{splatmapResolution}");

        float xOffset = 0f;  // Offset correction for alignment
        float yOffset = 0f; 

        for (int i = 1; i < lines.Length; i++) // Skip header line
        {
            string[] parts = lines[i].Trim().Split(',');
            if (parts.Length != 5) continue;

            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            int layer1ID = int.Parse(parts[2]);
            int layer2ID = int.Parse(parts[3]);
            float alpha = float.Parse(parts[4]);

            float normalizedX = (x + xOffset) / (terrainSize - 1);
            float normalizedY = (y + yOffset) / (terrainSize - 1);

            int unityX = Mathf.Clamp(Mathf.RoundToInt(normalizedX * (splatmapResolution - 1)), 0, splatmapResolution - 1);
            int unityY = Mathf.Clamp(Mathf.RoundToInt(normalizedY * (splatmapResolution - 1)), 0, splatmapResolution - 1);

            if (layer1ID >= terrainLayers.Length) continue;
            if (layer2ID >= terrainLayers.Length) layer2ID = layer1ID;

            splatmapData[unityY, unityX, layer1ID] = 1.0f;
            if (alpha > 0.0f)
            {
                splatmapData[unityY, unityX, layer1ID] = 1.0f - alpha;
                splatmapData[unityY, unityX, layer2ID] = alpha;
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, splatmapData);
        Debug.Log("✅ Terrain layers successfully applied with heightmap correction!");
    }
}
