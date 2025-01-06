using System;
using UnityEngine;
using NativeWebSocket;
using Google.Protobuf;
using ConnectProto;

public class WebSocketClient : MonoBehaviour
{
    [NonSerialized]
    private static WebSocketClient instance;

    [NonSerialized]
    private WebSocket websocket;

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
            Debug.Log("Message received.");
            HandleMessage(bytes);
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"WebSocket error: {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket connection closed.");
        };

        await websocket.Connect();
    }

    private void Update()
    {
        websocket?.DispatchMessageQueue();
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
        Debug.Log($"Received message type: {wrapper.Type}");

        switch (wrapper.Type)
        {
            case "HelloResponse":
                var helloResponse = HelloResponse.Parser.ParseFrom(wrapper.Payload);
                Debug.Log($"Received HelloResponse: {helloResponse.Message}");
                SendHelloMessage();
                break;
            case "HelloResponse2":
                var helloResponse2 = HelloResponse.Parser.ParseFrom(wrapper.Payload);
                Debug.Log($"Received HelloResponse2: {helloResponse2.Message}");
                break;

            default:
                Debug.LogWarning($"Unknown message type: {wrapper.Type}");
                break;
        }
    }
}
