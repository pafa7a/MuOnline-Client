using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EncTerrainDecryptorEditor : MonoBehaviour
{
    private static readonly byte[] BuxCode = { 0xFC, 0xCF, 0xAB };
    private static readonly byte[] byMapXorKey = { 0xD1, 0x73, 0x52, 0xF6, 0xD2, 0x9A, 0xCB, 0x27,
                                                   0x3E, 0xAF, 0x59, 0x31, 0x37, 0xB3, 0xE7, 0xA2 };

    private const int TERRAIN_SIZE = 256;

    [MenuItem("Assets/MuOnline/Decrypt Terrain Attribute", false, 1004)]
    private static void DecryptTerrainAttribute()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".att"))
        {
            Debug.LogError("Please select a valid .att file.");
            return;
        }

        byte[] encryptedData = File.ReadAllBytes(path);
        if (encryptedData.Length < 4)
        {
            Debug.LogError("Invalid terrain attribute file.");
            return;
        }

        // Decrypt file using MapFileDecrypt logic
        byte[] decryptedData = MapFileDecrypt(encryptedData);
        if (decryptedData == null)
        {
            Debug.LogError("Failed to decrypt the file.");
            return;
        }

        // Apply BuxConvert to the decrypted data
        BuxConvert(decryptedData);

        // Extract header values correctly
        int version = decryptedData[0];
        int mapId = decryptedData[1];
        int width = decryptedData[2];
        int height = decryptedData[3];

        if (version != 0 || width != 255 || height != 255)
        {
            Debug.LogError($"Invalid terrain attribute file. Version: {version}, Width: {width}, Height: {height}");
            return;
        }

        bool isExtended = decryptedData.Length == (TERRAIN_SIZE * TERRAIN_SIZE * sizeof(ushort) + 4);
        ushort[] terrainWall = new ushort[TERRAIN_SIZE * TERRAIN_SIZE];

        using (MemoryStream ms = new MemoryStream(decryptedData, 4, decryptedData.Length - 4))
        using (BinaryReader br = new BinaryReader(ms))
        {
            for (int i = 0; i < TERRAIN_SIZE * TERRAIN_SIZE; i++)
            {
                terrainWall[i] = isExtended ? br.ReadUInt16() : br.ReadByte();
            }
        }

        // ✅ WD_0LORENCIA Check
        bool error = false;
        if (mapId == 1) // Lorencia Map Check
        {
            if (terrainWall[123 * TERRAIN_SIZE + 135] != 5)
            {
                error = true;
            }
        }
        if (mapId == 2) // Dungeon Map Check
        {
            if (terrainWall[120 * TERRAIN_SIZE + 227] != 4)
            {
                error = true;
            }
        }

        // ✅ Apply final bitwise mask and validation
        for (int i = 0; i < TERRAIN_SIZE * TERRAIN_SIZE; i++)
        {
            terrainWall[i] &= 0xFF;
            if (terrainWall[i] >= 128)
                error = true;
        }

        if (error)
        {
            Debug.LogError("❌ Terrain data validation failed! Possible corruption detected.");
            return;
        }

        // Save output as `EncTerrain1.txt`
        string outputFilePath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".txt");
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            // ✅ Write Headers
            writer.WriteLine($"Version: {version}");
            writer.WriteLine($"Map ID: {mapId}");
            writer.WriteLine($"Width: {width}, Height: {height}");
            writer.WriteLine("x,y,type"); // ✅ CSV Header

            // ✅ Write Terrain Attributes
            for (int y = 0; y < TERRAIN_SIZE; y++)
            {
                for (int x = 0; x < TERRAIN_SIZE; x++)
                {
                    writer.WriteLine($"{x},{y},{terrainWall[y * TERRAIN_SIZE + x]}"); // ✅ CSV Row format
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"✅ Decryption complete. Saved to {outputFilePath}");
    }

    private static byte[] MapFileDecrypt(byte[] encryptedData)
    {
        byte[] decryptedData = new byte[encryptedData.Length];
        ushort wMapKey = 0x5E; // Rolling key

        for (int i = 0; i < encryptedData.Length; i++)
        {
            decryptedData[i] = (byte)((encryptedData[i] ^ byMapXorKey[i % 16]) - (byte)wMapKey);
            wMapKey = (ushort)((encryptedData[i] + 0x3D) & 0xFF);
        }

        return decryptedData;
    }

    private static void BuxConvert(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= BuxCode[i % 3];
        }
    }
}
