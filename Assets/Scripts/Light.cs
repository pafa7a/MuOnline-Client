using UnityEngine;

public class LightmapToLights : MonoBehaviour
{
    public Texture2D lightmap; // Assign your lightmap here
    public Terrain terrain; // Assign the terrain in the Inspector
    public GameObject lightPrefab; // Prefab for the point light (optional)
    public float intensityMultiplier = 1.0f; // Scale the light intensity
    public float lightRange = 10f; // Default range for the lights
    public float minBrightnessThreshold = 0.5f; // Minimum brightness to create a light
    public int lightDensity = 4; // Sample every nth pixel for performance
    public float colorSaturationFactor = 0.7f; // Factor to enhance the saturation of the light color

    public GameObject lightsParent; // Parent object to group lights

    public void GenerateLights()
    {
        if (lightmap == null || terrain == null)
        {
            Debug.LogError("Lightmap or Terrain not assigned.");
            return;
        }

        // Create a parent GameObject for the lights
        if (lightsParent == null)
        {
            lightsParent = new GameObject("GeneratedLights");
            lightsParent.transform.SetParent(this.transform.parent.transform);
        }
        else
        {
            // Clear all existing children of the sub-parent
            for (int i = lightsParent.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(lightsParent.transform.GetChild(i).gameObject);
            }
        }

        // Get terrain size
        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        if (lightDensity < 1) lightDensity = 1;

        // Generate lights from the lightmap
        for (int x = 0; x < lightmap.width; x += lightDensity)
        {
            for (int y = 0; y < lightmap.height; y += lightDensity)
            {
                // Get brightness and color from the lightmap
                Color pixelColor = lightmap.GetPixel(x, y);
                float brightness = pixelColor.grayscale; // Brightness based on grayscale value

                // Skip pixels below the brightness threshold
                if (brightness < minBrightnessThreshold) continue;

                // Enhance saturation
                pixelColor = EnhanceSaturation(pixelColor, colorSaturationFactor);

                // Scale color by brightness for more dynamic lighting
                Color finalColor = pixelColor * brightness;

                // Calculate world position
                float worldX = terrainPosition.x + (x / (float)lightmap.width) * terrainSize.x;
                float worldZ = terrainPosition.z + (y / (float)lightmap.height) * terrainSize.z;
                float worldY = terrain.SampleHeight(new Vector3(worldX, 0, worldZ)) + terrainPosition.y;

                // Create a point light with the enhanced color
                CreatePointLight(new Vector3(worldX, worldY + 5f, worldZ), brightness, finalColor);
            }
        }
    }


    void CreatePointLight(Vector3 position, float brightness, Color lightColor)
    {
        // Create a new light object
        GameObject lightObj;

        if (lightPrefab != null)
        {
            // Use a prefab for the light if provided
            lightObj = Instantiate(lightPrefab, position, Quaternion.identity, lightsParent.transform); // Assign parent here
        }
        else
        {
            // Create a new GameObject with a Light component
            lightObj = new GameObject("PointLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            var bakingOutput = light.bakingOutput;
            bakingOutput.lightmapBakeType = LightmapBakeType.Baked;
            light.bakingOutput = bakingOutput;


#if UNITY_EDITOR
            // Set the light to Baked mode
            light.lightmapBakeType = LightmapBakeType.Baked;
#endif

            // Set light properties
            light.intensity = brightness * intensityMultiplier; // Scale brightness
            light.range = lightRange;
            light.color = lightColor; // Use the enhanced color
        }

        // Set the light's position
        lightObj.transform.position = position;

        // Parent the light under the group
        lightObj.transform.parent = lightsParent.transform;
    }

    Color EnhanceSaturation(Color color, float factor)
    {
        // Interpolate between the color and white to adjust saturation
        return Color.Lerp(Color.white, color, factor);
    }
}
