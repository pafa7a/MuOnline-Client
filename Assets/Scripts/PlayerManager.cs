using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private CanvasManager _canvas;
    private bool _shouldDisplayPlayerCoordinates = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Display coordinates only in World scene.
        Scene scene = SceneManager.GetActiveScene();
        _canvas = CanvasManager.Instance;
        if (_canvas && scene != null && scene.name == "World")
        {
            _shouldDisplayPlayerCoordinates = true;
        }

    }

    void Update()
    {
        DisplayPlayerCoordinatesInCanvas();
    }

    void OnDestroy()
    {
        if (_canvas != null)
        {
            _canvas.PlayerCoordinatesObject.gameObject.SetActive(false);
        }
    }

    void DisplayPlayerCoordinatesInCanvas()
    {
        if (_shouldDisplayPlayerCoordinates)
        {
            _canvas.PlayerCoordinatesObject.gameObject.SetActive(_canvas.PlayerCoordinatesDisplay);
            _canvas.PlayerCoordinatesObject.text = $"Player: X: {gameObject.transform.position.x:F0}, Y: {gameObject.transform.position.z:F0}";
        }
    }
}
