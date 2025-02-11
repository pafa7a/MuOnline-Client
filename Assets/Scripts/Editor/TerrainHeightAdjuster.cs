using UnityEditor;
using UnityEngine;

public class TerrainHeightAdjuster : MonoBehaviour
{
    [MenuItem("Tools/MuOnline/Fix Terrain XY Offset")]
    private static void FixTerrainXYOffset()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No active terrain found.");
            return;
        }

        int resolution = terrain.terrainData.heightmapResolution;
        float[,] heights = terrain.terrainData.GetHeights(0, 0, resolution, resolution);

        // Create a new heightmap array
        float[,] newHeights = new float[resolution, resolution];

        // Shift heightmap 2 pixels left and 1 pixel down
        for (int y = 1; y < resolution; y++)
        {
            for (int x = 0; x < resolution - 2; x++)
            {
                newHeights[y, x] = heights[y - 1, x + 2]; // Shift left by 2 & down by 1
            }
        }

        // Fill missing rightmost pixels using the last known value
        for (int y = 0; y < resolution; y++)
        {
            newHeights[y, resolution - 1] = newHeights[y, resolution - 3];
            newHeights[y, resolution - 2] = newHeights[y, resolution - 3];
        }

        // Fill missing top row using the second row values to avoid gaps
        for (int x = 0; x < resolution; x++)
        {
            newHeights[0, x] = newHeights[1, x];
        }

        // Apply the corrected heightmap
        terrain.terrainData.SetHeights(0, 0, newHeights);
        Debug.Log("Successfully fixed terrain X and Y offset (Shifted Left by 2 & Down by 1).");
    }
}
