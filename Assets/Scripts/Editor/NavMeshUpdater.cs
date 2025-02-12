using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine.AI;

public class NavMeshUpdater : MonoBehaviour
{
    private const int TERRAIN_SIZE = 256;
    private const int TW_NOMOVE = 0x0004; // Non-walkable flag
    private const int TW_SAFEZONE = 0x0001; // Safezone: Walkable but reduced speed

    [MenuItem("Assets/MuOnline/Update NavMesh from Terrain Data", false, 1005)]
    private static void UpdateNavMesh()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("❌ No active terrain found.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path) || !path.EndsWith(".txt"))
        {
            Debug.LogError("❌ Please select a valid EncTerrain1.txt file.");
            return;
        }

        List<(Bounds bounds, int areaType)> navMeshAreas = new List<(Bounds, int)>();

        using (StreamReader reader = new StreamReader(path))
        {
            string line;
            bool startReading = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (!startReading)
                {
                    if (line.StartsWith("x,y,type")) startReading = true;
                    continue;
                }

                string[] parts = line.Split(',');
                if (parts.Length < 3) continue;

                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                int type = int.Parse(parts[2]);

                float terrainHeight = terrain.SampleHeight(new Vector3(x, 0, y)) - 1;
                Vector3 position = new Vector3(x + 0.5f, terrainHeight, y + 0.5f);

                // ✅ Non-walkable areas
                if ((type & TW_NOMOVE) != 0)
                {
                    Bounds areaBounds = new Bounds(position, new Vector3(0.5f, 0.5f, 0.5f));
                    navMeshAreas.Add((areaBounds, NavMesh.GetAreaFromName("Not Walkable")));
                }
                // ✅ Safezone (walkable but reduces speed)
                else if ((type & TW_SAFEZONE) != 0)
                {
                    Bounds areaBounds = new Bounds(position, new Vector3(1f, 1f, 1f));
                    navMeshAreas.Add((areaBounds, NavMesh.GetAreaFromName("SafeZone")));
                }
            }
        }

        ApplyNavMeshModifier(navMeshAreas);
        Debug.Log("✅ NavMesh updated based on terrain attributes.");
    }

    private static void ApplyNavMeshModifier(List<(Bounds bounds, int areaType)> navMeshAreas)
    {
        GameObject parent = GameObject.Find("NavMeshModifiers");
        if (parent == null)
        {
            parent = new GameObject("NavMeshModifiers");
        }

        // Remove existing modifiers
        foreach (Transform child in parent.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // Add new modifiers
        foreach (var (area, areaType) in navMeshAreas)
        {
            GameObject volumeObject = new GameObject("NavMeshModifier");
            volumeObject.transform.SetParent(parent.transform);
            volumeObject.transform.position = area.center;

            NavMeshModifierVolume modifier = volumeObject.AddComponent<NavMeshModifierVolume>();
            modifier.size = area.size;
            modifier.area = areaType;
        }

        Debug.Log("✅ NavMesh Modifiers applied.");
    }
}
