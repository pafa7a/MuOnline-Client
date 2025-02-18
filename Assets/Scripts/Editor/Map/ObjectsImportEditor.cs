using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectsImportEditor : MonoBehaviour
{
    private static string mapNumber;

    [MenuItem("Assets/MuOnline/Import objects", false, 1201)]
    private static void ParseAndLoadObjects()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".txt"))
        {
            Debug.LogError("Please select a valid .txt file.");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        if (lines.Length < 5)
        {
            Debug.LogError("File does not contain enough lines to parse objects.");
            return;
        }

        // === FIND TERRAIN ===
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogWarning("No active terrain found! Objects will be placed at their original Y positions.");
        }

        // === STEP 1: MAP TYPE TO OBJECT NAME (CORRECTED) ===
        Dictionary<int, string> typeToObject = new Dictionary<int, string>
        {
            { 0, "Tree01" }, { 1, "Tree02" }, { 2, "Tree03" }, { 3, "Tree04" }, { 4, "Tree05" },
            { 5, "Tree06" }, { 6, "Tree07" }, { 7, "Tree08" }, { 8, "Tree09" }, { 9, "Tree10" },
            { 10, "Tree11" }, { 11, "Tree12" }, { 12, "Tree13" },
            { 20, "Grass01" }, { 21, "Grass02" }, { 22, "Grass03" }, { 23, "Grass04" }, { 24, "Grass05" },
            { 25, "Grass06" }, { 26, "Grass07" }, { 27, "Grass08" },
            { 30, "Stone01" }, { 31, "Stone02" }, { 32, "Stone03" }, { 33, "Stone04" }, { 34, "Stone05" },
            { 40, "StoneStatue01" }, { 41, "StoneStatue02" }, { 42, "StoneStatue03" },
            { 43, "SteelStatue01" }, { 44, "Tomb01" }, { 45, "Tomb02" }, { 46, "Tomb03" },
            { 50, "FireLight01" }, { 51, "FireLight02" }, { 52, "Bonfire01" },
            { 55, "DungeonGate01" }, { 56, "MerchantAnimal01" }, { 57, "MerchantAnimal02" },
            { 58, "TreasureDrum01" }, { 59, "TreasureChest01" }, { 60, "Ship01" },
            { 65, "SteelWall01" }, { 66, "SteelWall02" }, { 67, "SteelWall03" }, { 68, "SteelDoor01" },
            { 69, "StoneWall01" }, { 70, "StoneWall02" }, { 71, "StoneWall03" }, { 72, "StoneWall04" },
            { 73, "StoneWall05" }, { 74, "StoneWall06" }, { 75, "StoneMuWall01" }, { 76, "StoneMuWall02" },
            { 77, "StoneMuWall03" }, { 78, "StoneMuWall04" }, { 80, "Bridge01" }, { 81, "Fence01" }, { 82, "Fence02" },
            { 83, "Fence03" }, { 84, "Fence04" }, { 85, "BridgeStone01" },
            { 90, "StreetLight01" }, { 91, "Cannon01" }, { 92, "Cannon02" }, { 93, "Cannon03" },
            { 95, "Curtain01" }, { 96, "Sign01" }, { 97, "Sign02" }, { 98, "Carriage01" }, { 99, "Carriage02" },
            { 100, "Carriage03" }, { 101, "Carriage04" }, { 102, "Straw01" }, { 103, "Straw02" },
            { 105, "Waterspout01" }, { 106, "Well01" }, { 107, "Well02" }, { 108, "Well03" }, { 109, "Well04" },
            { 110, "Hanging01" }, { 111, "Stair01" },
            { 115, "House01" }, { 116, "House02" }, { 117, "House03" }, { 118, "House04" },
            { 119, "House05" }, { 120, "Tent01" }, { 121, "HouseWall01" }, { 122, "HouseWall02" },
            { 123, "HouseWall03" }, { 124, "HouseWall04" }, { 125, "HouseWall05" }, { 126, "HouseWall06" },
            { 127, "HouseEtc01" }, { 128, "HouseEtc02" }, { 129, "HouseEtc03" },
            { 130, "Light01" }, { 131, "Light02" }, { 132, "Light03" }, { 133, "PoseBox01" },
            { 140, "Furniture01" }, { 141, "Furniture02" }, { 142, "Furniture03" },
            { 143, "Furniture04" }, { 144, "Furniture05" }, { 145, "Furniture06" },
            { 146, "Furniture07" }, { 150, "Candle01" }, { 151, "Beer01" }, { 152, "Beer02" }, { 153, "Beer03" }
        };

        mapNumber = lines[1].Split(':')[1].Trim();
        string basePath = $"Assets/Resources/Maps/World{mapNumber}/Objects/";

        GameObject mapObject = GameObject.Find("Map");
        if (mapObject == null)
        {
            Debug.LogError("Map object not found in scene. Please create a Map object first.");
            return;
        }

        GameObject parentObject = GameObject.Find("Objects");
        if (parentObject == null)
        {
            parentObject = new GameObject("Objects");
            parentObject.transform.SetParent(mapObject.transform);
        }

        for (int i = 4; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length < 8) continue;

            int type = int.Parse(parts[0]);
            float posX = float.Parse(parts[1]);
            float posZ = float.Parse(parts[2]);
            float posY = float.Parse(parts[3]);
            float rotX = float.Parse(parts[4]);
            float rotZ = float.Parse(parts[5]);
            float rotY = float.Parse(parts[6]);
            float scale = float.Parse(parts[7]);

            string objectName;
            if (mapNumber == "1")
            {
                if (!typeToObject.TryGetValue(type, out objectName))
                {
                    Debug.LogWarning($"Type {type} not mapped, skipping.");
                    continue;
                }
            }
            else
            {
                objectName = (type + 1) < 10 ? $"Object0{type + 1}" : $"Object{type + 1}";
            }

            string folderName = $"_Fbx_{objectName}";
            string folderPath = $"{basePath}{folderName}";

            string modelPath = FindFBXModel(folderPath);
            if (string.IsNullOrEmpty(modelPath))
            {
                Debug.LogWarning($"Model not found in {folderPath}");
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                instance.name = objectName;
                instance.transform.position = new Vector3(posX, posY + 0.86f, posZ);
                instance.transform.eulerAngles = new Vector3(rotX, 180f - rotY, rotZ);
                instance.transform.localScale = new Vector3(scale, scale, scale);
                instance.transform.SetParent(parentObject.transform);
            }
        }
        Debug.Log("Finished loading objects.");
    }

    private static string FindFBXModel(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return null;
        string[] files = Directory.GetFiles(folderPath, "*.fbx");
        return files.Length > 0 ? files[0] : null;
    }
}
