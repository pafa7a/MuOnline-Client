#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightmapToLights))]
public class LightmapToLightsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        LightmapToLights script = (LightmapToLights)target;

        // Add a button to the inspector
        if (GUILayout.Button("Generate Lights"))
        {
            script.GenerateLights();
        }

        // Add a button to clear lights
        if (GUILayout.Button("Clear Lights"))
        {
            if (script.lightsParent != null)
            {
                DestroyImmediate(script.lightsParent);
            }
        }
    }
}
#endif
