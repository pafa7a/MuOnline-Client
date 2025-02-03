using System;
using GameServerProto;
using UnityEngine;

[MessageType("LoginResponse")]
public class LoginResponseHandler : IMessageHandler
{
    public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
    {
        LoginResponse loginResponse = LoginResponse.Parser.ParseFrom(message);
        string popUpText = "";
        if (loginResponse.ResponseCode == LoginResponseEnum.LoginOk)
        {
            //@TODO: Character select.
            return;
        }
        Action onOk = null;
        switch (loginResponse.ResponseCode)
        {
            case LoginResponseEnum.LoginInvalidCredentials:
                popUpText = "Invalid credentials";
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
                break;
            case LoginResponseEnum.LoginTooManyAttempts:
                popUpText = "Too many failed attempts";
                onOk = () => Application.Quit();
                break;
        }
        PopUpMessage.Show(popUpText, PopUpMessage.ButtonsEnum.OK, onOk);
    }
}
