using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class ObjectDecryptorEditor
{
    private static readonly byte[] byObjectXorKey = { 0xD1, 0x73, 0x52, 0xF6, 0xD2, 0x9A, 0xCB, 0x27,
                                                      0x3E, 0xAF, 0x59, 0x31, 0x37, 0xB3, 0xE7, 0xA2 };

    [MenuItem("Assets/MuOnline/Decrypt Object File", false, 1002)]
    private static void DecryptSelectedObjectFile()
    {
        string filePath = GetSelectedObjectFile();
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("❌ No valid .obj file selected!");
            return;
        }

        string outputPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".txt");

        try
        {
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] decryptedData = ObjectFileDecrypt(encryptedData);

            ExtractObjectsAndSave(decryptedData, outputPath);
            Debug.Log($"✅ Object data extracted and saved to: {outputPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error decrypting object file: {e.Message}");
        }
    }

    private static string GetSelectedObjectFile()
    {
        foreach (Object obj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (assetPath.EndsWith(".obj"))
            {
                return Path.GetFullPath(assetPath);
            }
        }
        return null;
    }

    private static byte[] ObjectFileDecrypt(byte[] encryptedData)
    {
        byte[] decryptedData = new byte[encryptedData.Length];
        ushort wObjectKey = 0x5E; // Rolling key

        for (int i = 0; i < encryptedData.Length; i++)
        {
            decryptedData[i] = (byte)((encryptedData[i] ^ byObjectXorKey[i % 16]) - (byte)wObjectKey);
            wObjectKey = (ushort)((encryptedData[i] + 0x3D) & 0xFF); // Update rolling key
        }

        return decryptedData;
    }

    private static void ExtractObjectsAndSave(byte[] data, string outputPath)
    {
        int dataPtr = 0;  // Start at the beginning to get header info

        // Read the header (1 byte)
        byte header = data[dataPtr]; dataPtr += 1;

        // Read the map number (1 byte)
        byte mapNumber = data[dataPtr]; dataPtr += 1;

        // Read the object count (2 bytes, short)
        short objectCount = System.BitConverter.ToInt16(data, dataPtr); dataPtr += 2;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Header: {header}");
        sb.AppendLine($"Map Number: {mapNumber}");
        sb.AppendLine($"Object Count: {objectCount}");
        sb.AppendLine("Type,PositionX,PositionY,PositionZ,AngleX,AngleY,AngleZ,Scale");

        for (int i = 0; i < objectCount; i++)
        {
            if (dataPtr + 30 > data.Length) break; // Ensure we don't read beyond the data length

            short type = System.BitConverter.ToInt16(data, dataPtr); dataPtr += 2;
            float posX = System.BitConverter.ToSingle(data, dataPtr) / 100; dataPtr += 4;
            float posY = System.BitConverter.ToSingle(data, dataPtr) / 100; dataPtr += 4;
            float posZ = System.BitConverter.ToSingle(data, dataPtr); dataPtr += 4;
            float angX = System.BitConverter.ToSingle(data, dataPtr); dataPtr += 4;
            float angY = System.BitConverter.ToSingle(data, dataPtr); dataPtr += 4;
            float angZ = System.BitConverter.ToSingle(data, dataPtr); dataPtr += 4;
            float scale = System.BitConverter.ToSingle(data, dataPtr); dataPtr += 4;

            sb.AppendLine($"{type},{posX},{posY},{posZ},{angX},{angY},{angZ},{scale}");
        }

        File.WriteAllText(outputPath, sb.ToString());
        Debug.Log($"✔ Data saved with Header: {header}, Map Number: {mapNumber}, Object Count: {objectCount}");
    }
}
