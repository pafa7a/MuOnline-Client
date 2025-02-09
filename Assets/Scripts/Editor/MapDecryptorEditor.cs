using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class MapDecryptorEditor
{
    private static readonly byte[] byMapXorKey = { 0xD1, 0x73, 0x52, 0xF6, 0xD2, 0x9A, 0xCB, 0x27,
                                                   0x3E, 0xAF, 0x59, 0x31, 0x37, 0xB3, 0xE7, 0xA2 };

    private const int mapSize = 256;
    private const int layerSize = mapSize * mapSize; // Each layer is 256x256 bytes


    [MenuItem("Assets/MuOnline/Decrypt Map File", false, 1001)]
    private static void DecryptSelectedMapFile()
    {
        string filePath = GetSelectedMapFile();
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("❌ No valid .map file selected!");
            return;
        }

        string outputPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".txt");

        try
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] decryptedData = MapFileDecrypt(encryptedData);

            if (decryptedData.Length < (layerSize * 3))
            {
                Debug.LogError("❌ Decrypted data is too small! Possibly corrupt.");
                return;
            }

            byte[] newData = new byte[decryptedData.Length - 2]; // Remove first 2 bytes
            System.Buffer.BlockCopy(decryptedData, 2, newData, 0, newData.Length);

            ExtractLayersAndSave(newData, outputPath);
            Debug.Log($"✅ Map data extracted and saved to: {outputPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error decrypting map file: {e.Message}");
        }
    }

    private static string GetSelectedMapFile()
    {
        foreach (Object obj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (assetPath.EndsWith(".map"))
            {
                return Path.GetFullPath(assetPath);
            }
        }
        return null;
    }

    private static byte[] MapFileDecrypt(byte[] encryptedData)
    {
        byte[] decryptedData = new byte[encryptedData.Length];
        ushort wMapKey = 0x5E; // Rolling key

        for (int i = 0; i < encryptedData.Length; i++)
        {
            decryptedData[i] = (byte)((encryptedData[i] ^ byMapXorKey[i % 16]) - (byte)wMapKey);
            wMapKey = (ushort)((encryptedData[i] + 0x3D) & 0xFF); // Update rolling key
        }

        return decryptedData;
    }

    private static void ExtractLayersAndSave(byte[] data, string outputPath)
    {
        byte[] layer1 = new byte[layerSize];
        byte[] layer2 = new byte[layerSize];
        float[] alpha = new float[layerSize]; // Alpha layer is stored as floats

        System.Buffer.BlockCopy(data, 0, layer1, 0, layerSize);
        System.Buffer.BlockCopy(data, layerSize, layer2, 0, layerSize);

        for (int i = 0; i < layerSize; i++)
        {
            alpha[i] = data[layerSize * 2 + i] / 255.0f; // Normalize alpha to float (0.0 - 1.0)
        }

        SaveToTextFile(outputPath, layer1, layer2, alpha);
    }

    private static void SaveToTextFile(string outputPath, byte[] layer1, byte[] layer2, float[] alpha)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("x,y,layer1,layer2,alpha"); // CSV header

        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                int index = y * mapSize + x;
                sb.AppendLine($"{x},{y},{layer1[index]},{layer2[index]},{alpha[index]:F2}");
            }
        }

        File.WriteAllText(outputPath, sb.ToString());
        Debug.Log("✔ Terrain data saved in structured format.");
    }
}
