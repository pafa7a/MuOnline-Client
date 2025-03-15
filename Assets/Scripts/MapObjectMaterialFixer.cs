using UnityEngine;

[ExecuteInEditMode]
public class MapObjectMaterialFixer : MonoBehaviour
{
  void OnEnable()
  {
    FixMaterials();
  }

  void Update()
  {
    FixMaterials();
  }

  void FixMaterials()
  {
    // Fix materials for all renderers in children
    foreach (var renderer in GetComponentsInChildren<Renderer>())
    {
      foreach (Material material in renderer.sharedMaterials)
      {
        if (material != null)
        {
          material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
          material.EnableKeyword("_ALPHATEST_ON");
        }
      }
    }
  }
}
