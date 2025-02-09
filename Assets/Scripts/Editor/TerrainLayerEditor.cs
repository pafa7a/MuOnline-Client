using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainLayerAssigner))]
public class TerrainLayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainLayerAssigner script = (TerrainLayerAssigner)target;

        if (GUILayout.Button("Apply Terrain Layers"))
        {
            script.ApplyTerrainLayers();
        }
    }
}
