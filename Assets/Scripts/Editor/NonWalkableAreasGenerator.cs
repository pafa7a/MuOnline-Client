using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NonWalkableAreasGenerator))]
public class NonWalkableAreasGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NonWalkableAreasGenerator generator = (NonWalkableAreasGenerator)target;

        if (GUILayout.Button("Generate Modifier Volumes"))
        {
            generator.GenerateModifierVolumes();
        }
    }
}
