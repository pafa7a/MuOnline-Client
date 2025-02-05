using System.Runtime.InteropServices;
using System;
using GameServerProto;
using UnityEngine;
using UnityEngine.SceneManagement;
using Google.Protobuf;

[MessageType("LoginResponse")]
public class LoginResponseHandler : IMessageHandler
{
    [DllImport("__Internal")]
    private static extern void ClosePage();
    public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
    {
        LoginResponse loginResponse = LoginResponse.Parser.ParseFrom(message);
        string popUpText = "";
        if (loginResponse.ResponseCode == LoginResponseEnum.LoginOk)
        {
            SceneManager.LoadScene("World");
            Wrapper wrapper = new()
            {
                Type = "WorldEnter",
                Payload = ByteString.Empty,
            };
            WebSocketClient.Send(wrapper.ToByteArray());
            return;
        }
        Action onOk = null;
        switch (loginResponse.ResponseCode)
        {
            case LoginResponseEnum.LoginInvalidCredentials:
                popUpText = "Invalid credentials";
                onOk = () => SelectServerManager.Instance._loginWrapper.GetComponent<InputManager>().Init();
                break;
            case LoginResponseEnum.LoginInvalidVersion:
                popUpText = "Invalid version";
                onOk = () => _ = SelectServerManager.Instance.CancelButtonPressed();
                break;
            case LoginResponseEnum.LoginInvalidSerial:
                popUpText = "Invalid serial";
                onOk = () => _ = SelectServerManager.Instance.CancelButtonPressed();
                break;
            case LoginResponseEnum.LoginServerFull:
                popUpText = "The server is full";
                onOk = () => _ = SelectServerManager.Instance.CancelButtonPressed();
                break;
            case LoginResponseEnum.LoginAlreadyConnected:
                popUpText = "This account is already connected";
                onOk = () => SelectServerManager.Instance._loginWrapper.GetComponent<InputManager>().Init();
                break;
            case LoginResponseEnum.LoginTooManyAttempts:
                popUpText = "Too many failed attempts";
                Action quitAction = () =>
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                    ClosePage();
#else
                    Application.Quit();
#endif
                };
                onOk = quitAction;
                break;
        }
        PopUpMessage.Show(popUpText, PopUpMessage.ButtonsEnum.OK, onOk);
    }
}
