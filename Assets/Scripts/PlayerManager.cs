using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance; // Singleton instance
    private static Scene originalScene;

    void Awake()
    {
        originalScene = SceneManager.GetActiveScene();
        // Ensure this is a Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        DisplayPlayerCoordinatesInCanvas();
    }

    void DisplayPlayerCoordinatesInCanvas()
    {
        CanvasManager canvas = CanvasManager.Instance;
        if (canvas) {
            // Hide the coordinates if the current scene is not the original player scene.
            if (!originalScene.isLoaded) {
                canvas.PlayerCoordinatesObject.gameObject.SetActive(false);
            }
            else {
                canvas.PlayerCoordinatesObject.gameObject.SetActive(canvas.PlayerCoordinatesDisplay);
                canvas.PlayerCoordinatesObject.text = $"Player: X: {gameObject.transform.position.x:F0}, Y: {gameObject.transform.position.z:F0}";
            }
        }
    }
}
