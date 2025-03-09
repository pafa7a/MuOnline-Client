using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    public Sprite[] hoverCursorFrames;     // Animation frames for hover cursor
    public Texture2D defaultCursor;       // Default cursor texture
    public Texture2D clickCursor;         // Cursor texture for left mouse click
    public Vector2 hotSpot = Vector2.zero; // Cursor hotspot position
    public CursorMode cursorMode = CursorMode.Auto;

    private bool isHovering = false;       // Tracks if hovering over an object
    private Coroutine hoverCoroutine;      // Reference to the hover coroutine
    private Texture2D[] hoverTextures;     // Converted Texture2D array
    private int currentFrame = 0;
    private float frameRate = 10f;
    public static CursorManager Instance; // Singleton instance for global access

    void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Prevent destruction on scene load
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    void Start()
    {
        Cursor.SetCursor(defaultCursor, hotSpot, cursorMode);
        hoverTextures = ConvertSpritesToTextures(hoverCursorFrames);
    }
    async void Update()
    {
        HandleRaycast();
        await HandleClickAsync();
    }

    private void HandleRaycast()
    {
        Vector3 mousePosition = Input.mousePosition;

        // Check if mouse is within screen bounds
        if (mousePosition.x < 0 || mousePosition.x > Screen.width ||
            mousePosition.y < 0 || mousePosition.y > Screen.height ||
            Camera.main == null)
        {
            if (isHovering)
            {
                StopHoverAnimation();
            }
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetComponent<TalkCursorMarker>() != null)
        {
            if (!isHovering) {
                StartHoverAnimation();
            }
        }
        else if (isHovering)
        {
            StopHoverAnimation();
        }
    }

    private async Task HandleClickAsync()
    {
        // Change cursor only if it's not already hovering.
        if (!isHovering) {
            if (Input.GetMouseButtonDown(0)) Cursor.SetCursor(clickCursor, hotSpot, cursorMode);
            else if (Input.GetMouseButtonUp(0) && !isHovering) Cursor.SetCursor(defaultCursor, hotSpot, cursorMode);
        }
        else {
            
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetComponent<TalkCursorMarker>() != null) {
            if (hit.collider.GetComponent<TalkCursorMarker>().name == "Capsule") {
                WebSocketClient.instance.ConnectToGameServer("localhost", 55901, 1);
            }
            if (hit.collider.GetComponent<TalkCursorMarker>().name == "Cube") {
                await WebSocketClient.instance.ConnectToConnectServer();
                CanvasManager.LoadScene("World75");
            }
        }
        }
    }

    private void StartHoverAnimation()
    {
        isHovering = true;
        hoverCoroutine = StartCoroutine(HoverCursorAnimation());
    }

    private void StopHoverAnimation()
    {
        isHovering = false;
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        Cursor.SetCursor(defaultCursor, hotSpot, cursorMode);
    }

    private IEnumerator HoverCursorAnimation()
    {
        while (isHovering)
        {
            Cursor.SetCursor(hoverTextures[currentFrame], hotSpot, cursorMode);
            yield return new WaitForSeconds(1f / frameRate);
            currentFrame = (currentFrame + 1) % hoverTextures.Length;
        }
    }

    private Texture2D[] ConvertSpritesToTextures(Sprite[] sprites)
    {
        Texture2D[] textures = new Texture2D[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            Rect rect = sprites[i].rect;
            Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            texture.SetPixels(sprites[i].texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
            texture.Apply();
            textures[i] = texture;
        }
        return textures;
    }
}
