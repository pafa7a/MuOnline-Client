using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class TerrainHeightsDecryptEditor : MonoBehaviour
{
    private const int TERRAIN_SIZE = 256; // OZB terrain resolution

    [MenuItem("Assets/MuOnline/Decrypt terrain heights (.OZB)", false, 1000)]
    private static void ExtractHeights()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".OZB"))
        {
            Debug.LogError("❌ Please select a valid TerrainHeight.OZB file.");
            return;
        }

        float[,] heightMap = ReadOZBHeightMap(path);
        if (heightMap == null)
        {
            Debug.LogError("❌ Failed to process OZB heightmap.");
            return;
        }

        SaveToCSV(path, heightMap);
    }

    private static float[,] ReadOZBHeightMap(string filePath)
    {
        float[,] heights = new float[TERRAIN_SIZE, TERRAIN_SIZE];

        byte[] bytes = File.ReadAllBytes(filePath);
        if (bytes.Length < (TERRAIN_SIZE * TERRAIN_SIZE))
        {
            Debug.LogError("❌ Invalid OZB file size.");
            return null;
        }

        int pos = 1082; // Offset correction

        for (int x = 0; x < TERRAIN_SIZE; x++)
        {
            pos += 2; // Offset per row
            for (int y = 0; y < TERRAIN_SIZE - 2; y++)
            {
                heights[x, y] = (float)bytes[pos] / 255; // Normalize height
                pos++;
            }
        }

        return heights;
    }

    private static void SaveToCSV(string originalFilePath, float[,] heights)
    {
        string outputPath = Path.Combine(Path.GetDirectoryName(originalFilePath), "TerrainHeights.csv");

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("X,Y,Height");

        for (int x = 0; x < TERRAIN_SIZE; x++)
        {
            for (int y = 0; y < TERRAIN_SIZE; y++)
            {
                sb.AppendLine($"{x},{y},{heights[x, y]:F3}");
            }
        }

        File.WriteAllText(outputPath, sb.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"✅ Terrain heights extracted and saved to: {outputPath}");
    }
}
