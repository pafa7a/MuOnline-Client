using UnityEngine;

[ExecuteInEditMode]
public class MapObjectMaterialFixer : MonoBehaviour
{
    void OnEnable()
    {
        FixMaterials();
    }

    void Start()
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
                    material.SetInt("_ZWrite", 1);
                    material.SetFloat("_Mode", 0);
                    material.renderQueue = -1;
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                }
            }
        }
    }
}
