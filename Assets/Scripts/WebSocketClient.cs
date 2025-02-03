using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using NativeWebSocket;
using ConnectProto;
using System.Threading.Tasks;

public class WebSocketClient : MonoBehaviour
{

    [SerializeField]
    public string gameServerIp = "localhost";
    [SerializeField]
    public string connectServerIp = "localhost";
    [SerializeField]
    public int connectServerPort = 44405;
    [SerializeField]
    public int gameServerPort = 0;
    [SerializeField]
    public int gameServerId = 0;
    [SerializeField]
    public string Version;
    [SerializeField]
    public string Serial;
    [SerializeField]
    public bool isGameServer = false;
    [SerializeField]
    private bool _debugConnection = false;
    public static WebSocketClient instance;
    private WebSocket websocket;
    private Dictionary<string, List<Type>> messageTypeToHandlerTypesCache = new Dictionary<string, List<Type>>();

    private void Awake()
    {
        // Ensure this GameObject is a singleton and persists across domain reloads
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Reinitialize WebSocket if necessary
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            InitializeWebSocket();
        }

        // Cache handler types after WebSocket initialization
        CacheHandlerTypes();
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
#endif
    }

    public async void InitializeWebSocket()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            return;
        }
        websocket = new WebSocket($"ws://{(isGameServer ? gameServerIp : connectServerIp)}:{(isGameServer ? gameServerPort : connectServerPort)}", new Dictionary<string, string>
            {
                {"clientType", Application.platform.ToString()},
                {"clientVersion", SystemInfo.operatingSystem}
            });

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connection opened.");
        };

        websocket.OnMessage += (bytes) =>
        {
            HandleMessage(bytes);
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"WebSocket error: {e}");
        };

        websocket.OnClose += HandleWebSocketClose;
        await websocket.Connect();
    }

    private void HandleWebSocketClose(WebSocketCloseCode closeCode)
    {
        Debug.Log("WebSocket connection closed.");
        if (instance != null)
        {
            Invoke(nameof(InitializeWebSocket), 5);  // Reconnect after 5 seconds if the instance is still valid
        }
    }
    public static void Send(byte[] data)
    {
        if (instance == null || instance.websocket == null || instance.websocket.State != WebSocketState.Open)
        {
            return;
        }
        if (instance._debugConnection)
        {
            // Log the message being sent
            Wrapper wrapper = Wrapper.Parser.ParseFrom(data);
            string direction = instance.isGameServer ? "C->GS" : "C->CS";
            Debug.Log($"{direction}: {wrapper.Type}");
        }
        // Send the message using the websocket instance
        instance.websocket.Send(data);
    }

    private void CacheHandlerTypes()
    {
        var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(IMessageHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Where(t => t.GetCustomAttributes<MessageTypeAttribute>().Any())
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var messageType = handlerType.GetCustomAttribute<MessageTypeAttribute>().MessageType;
            if (!messageTypeToHandlerTypesCache.ContainsKey(messageType))
            {
                messageTypeToHandlerTypesCache[messageType] = new List<Type>();
            }
            messageTypeToHandlerTypesCache[messageType].Add(handlerType);
        }
    }

    private void DispatchMessage(string messageType, byte[] message)
    {
        if (messageTypeToHandlerTypesCache.TryGetValue(messageType, out var handlerTypes))
        {
            foreach (var handlerType in handlerTypes)
            {
                var handlerInstance = (IMessageHandler)Activator.CreateInstance(handlerType);
                handlerInstance.HandleMessage(message, Send);
            }
        }
        else
        {
            Debug.LogWarning($"No handlers found for message type: {messageType}");
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            Debug.Log("Closing WebSocket connection.");
            await websocket.Close();
            websocket = null;
        }
    }

    private void HandleMessage(byte[] bytes)
    {
        Wrapper wrapper = Wrapper.Parser.ParseFrom(bytes);
        if (_debugConnection)
        {
            string direction = isGameServer ? "GS->C" : "CS->C";
            Debug.Log($"{direction}: {wrapper.Type}");
        }
        DispatchMessage(wrapper.Type, wrapper.Payload.ToByteArray());
    }

    public async Task CloseConnection()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            Debug.Log("Closing WebSocket connection intentionally.");
            await websocket.Close();
        }
    }

    public async void ConnectToGameServer(string IP, int port, int id)
    {
        await CloseConnection();
        // // Load the World scene asynchronously
        // AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("World");

        // // Wait until the scene is fully loaded
        // while (!asyncLoad.isDone)
        // {
        //     await Task.Yield();
        // }

        // Proceed with the rest of the logic
        gameServerIp = IP;
        gameServerPort = port;
        gameServerId = id;
        isGameServer = true;
        InitializeWebSocket();
    }

    public async Task ConnectToConnectServer()
    {
        
        await CloseConnection();
        // // Load the ServerSelect scene asynchronously
        // AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("ServerSelect");

        // // Wait until the scene is fully loaded
        // while (!asyncLoad.isDone)
        // {
        //     await Task.Yield();
        // }

        // Proceed with the rest of the logic
        gameServerIp = "";
        gameServerPort = 0;
        gameServerId = 0;
        isGameServer = false;
        InitializeWebSocket();
    }
}
