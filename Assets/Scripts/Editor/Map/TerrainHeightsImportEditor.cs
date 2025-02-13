using UnityEngine;
using UnityEditor;
using System.IO;
using System.Globalization;

public class TerrainHeightsImportEditor : MonoBehaviour
{
    private const int TERRAIN_SIZE = 256;
    private const int UNITY_TERRAIN_RES = 257;

    [MenuItem("Assets/MuOnline/Import terrain heights", false, 1001)]
    private static void LoadHeights()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("❌ No active terrain found! Make sure there's a terrain in the scene.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".csv"))
        {
            Debug.LogError("❌ Please select a valid TerrainHeights.csv file.");
            return;
        }

        float[,] heightMap = ReadCSVHeightMap(path);
        if (heightMap == null)
        {
            Debug.LogError("❌ Failed to read CSV heightmap.");
            return;
        }

        ApplyHeightmapToTerrain(terrain, heightMap);
    }

    private static float[,] ReadCSVHeightMap(string filePath)
    {
        float[,] heights = new float[UNITY_TERRAIN_RES, UNITY_TERRAIN_RES];

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            bool startReading = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (!startReading)
                {
                    if (line.StartsWith("X,Y,Height")) startReading = true;
                    continue;
                }

                string[] parts = line.Split(',');
                if (parts.Length < 3) continue;

                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                float height = float.Parse(parts[2], CultureInfo.InvariantCulture);

                heights[x, y] = height;
            }
        }

        FixResolutionForUnity(ref heights);
        return heights;
    }

    private static void FixResolutionForUnity(ref float[,] heights)
    {
        for (int i = 0; i < UNITY_TERRAIN_RES; i++)
        {
            heights[i, TERRAIN_SIZE] = heights[i, TERRAIN_SIZE - 1];
            heights[TERRAIN_SIZE, i] = heights[TERRAIN_SIZE - 1, i];
        }
    }

    private static void ApplyHeightmapToTerrain(Terrain terrain, float[,] heightMap)
    {
        terrain.terrainData.SetHeights(0, 0, heightMap);
        Debug.Log("✅ Terrain heightmap applied successfully!");
    }
}
