using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance; // Singleton instance

    [Header ("Debug info wrapper")]
    public Canvas DebugInfoWrapper;
    public bool DebugInfoWrapperDisplay = true;
    [Header("FPS")]
    public TMP_Text FPSObject;
    public float FPSUpdateInterval = 0.2f; //How often should the number update
    public bool FPSDisplay = true;
    float FPSTime = 0.0f;
    int FPSFrames = 0;
    [Header("Mouse coordinates")]
    public TMP_Text MouseCoordinates;
    public bool MouseCoordinatesDisplay = true;
    [Header("Player coordinates")]
    public TMP_Text PlayerCoordinatesObject;
    public bool PlayerCoordinatesDisplay = true;
    [Header("Loading screen")]
    public GameObject LoadingScreen;

    public static void LoadScene(string sceneName)
    {
        if (Instance != null && Instance.LoadingScreen != null)
        {
            Instance.LoadingScreen.SetActive(true);
        }
        SceneManager.LoadScene(sceneName);
    }

    void Awake()
    {
        // Ensure this is a Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Subscribe to scene events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Cleanup subscriptions
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (LoadingScreen != null)
        {
            LoadingScreen.SetActive(false);
        }
    }

    void Update()
    {
        RenderDebugInfo();
    }

    void RenderDebugInfo()
    {
        DebugInfoWrapper.gameObject.SetActive(DebugInfoWrapperDisplay);
        if (!DebugInfoWrapperDisplay) {
            return;
        }
        RenderFPS();
        RenderMouseCoordinates();
    }

    void RenderFPS()
    {
        FPSObject.gameObject.SetActive(FPSDisplay);
        if (!FPSDisplay) {
            return;
        }
        FPSTime += Time.unscaledDeltaTime;
        ++FPSFrames;

        // Interval ended - update GUI text and start new interval
        if (FPSTime >= FPSUpdateInterval)
        {
            float fps = (int)(FPSFrames / FPSTime);
            FPSTime = 0.0f;
            FPSFrames = 0;

            FPSObject.text = fps.ToString() + " FPS";
        }
    }

    void RenderMouseCoordinates()
    {
        MouseCoordinates.gameObject.SetActive(MouseCoordinatesDisplay);
        if (!MouseCoordinatesDisplay)
        {
            return;
        }

        // Get mouse position in screen coordinates
        Vector3 mousePosition = Input.mousePosition;

        // Convert to world position
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 worldPosition = hit.point;
            MouseCoordinates.text = $"Mouse: X: {worldPosition.x:F2}, Y: {worldPosition.y:F2}, Z: {worldPosition.z:F2}";
        }
        else
        {
            MouseCoordinates.text = $"Mouse: X: {mousePosition.x:F0}, Y: {mousePosition.y:F0}";
        }
    }
}
