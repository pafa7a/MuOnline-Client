using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TextPositionController : MonoBehaviour
{
    public void SetTop(float top)
    {
        // Get the RectTransform (no need for a null check because it's guaranteed to exist)
        RectTransform rectTransform = GetComponent<RectTransform>();

        // Modify the Top property by changing offsetMax.y
        rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, top);
    }
}
