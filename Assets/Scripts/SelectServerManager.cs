using UnityEngine;

public class SelectServerManager : MonoBehaviour
{
    public static SelectServerManager Instance;
    public Canvas Canvas;
    public GameObject ServerListGroupPrefab;
    public GameObject ServerButtonPrefab;
    private GameObject _serverListGroup;
    void Awake()
    {
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

    public void DisplayServersList()
    {
        if (_serverListGroup != null)
        {
            DestroyImmediate(_serverListGroup);
        }

        _serverListGroup = Instantiate(ServerListGroupPrefab, Vector3.zero, Quaternion.identity, Canvas.transform);

        // Set custom position, as it gets overrided by Unity bug...
        RectTransform ServerListGroupRectTransform = _serverListGroup.GetComponent<RectTransform>();
        ServerListGroupRectTransform.offsetMin = Vector2.zero;
        ServerListGroupRectTransform.offsetMax = Vector2.zero;
    }
}

