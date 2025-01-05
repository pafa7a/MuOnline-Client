using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DisplayChildCount
{
    static DisplayChildCount()
    {
        // Subscribe to the hierarchy window GUI event
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        // Get the GameObject for the current hierarchy item
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (obj != null && obj.transform.childCount > 0)
        {
            // Append the child count to the GameObject's name in the Hierarchy
            string childCountText = $" [{obj.transform.childCount}]";

            // Calculate the position to draw the text
            Rect labelRect = new Rect(selectionRect.xMax - 50, selectionRect.y, 50, selectionRect.height);

            // Draw the child count in the Hierarchy
            GUI.Label(labelRect, childCountText);
        }
    }
}
