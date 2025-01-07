using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using NativeWebSocket;
using Google.Protobuf;
using ConnectProto;

public class WebSocketClient : MonoBehaviour
{
    private static WebSocketClient instance;
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
        websocket.DispatchMessageQueue();
#endif
    }

    private async void InitializeWebSocket()
    {
        websocket = new WebSocket("ws://localhost:8080");

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connection opened.");
            SendHelloMessage(); // Optional: Send a Hello message upon connection
        };

        websocket.OnMessage += (bytes) =>
        {
            HandleMessage(bytes);
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"WebSocket error: {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket connection closed.");
            //@TODO: Handle with reconnect scene and test in WebGL!
            if (instance != null)
            {
                Invoke(nameof(InitializeWebSocket), 5);  // Reconnect after 5 seconds if the instance is still valid
            }
        };

        await websocket.Connect();
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
                handlerInstance.HandleMessage(message);
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

    private void SendHelloMessage()
    {
        HelloResponse helloMessage = new HelloResponse
        {
            Message = "Hello, server!"
        };

        Wrapper wrapper = new Wrapper
        {
            Type = "HelloResponse",
            Payload = ByteString.CopyFrom(helloMessage.ToByteArray())
        };

        websocket.Send(wrapper.ToByteArray());
        Debug.Log("Sent HelloResponse message to the server.");
    }

    private void HandleMessage(byte[] bytes)
    {
        Wrapper wrapper = Wrapper.Parser.ParseFrom(bytes);
        DispatchMessage(wrapper.Type, wrapper.Payload.ToByteArray());
    }
}
