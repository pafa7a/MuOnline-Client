using System;
using GameServerProto;
using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;

[MessageType("RegisterResponse")]
public class RegisterResponseHandler : IMessageHandler
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ClosePage();
#endif

    public void HandleMessage(byte[] message, Action<byte[]> sendMessage)
    {
        RegisterResponse registerResponse = RegisterResponse.Parser.ParseFrom(message);
        string popUpText = "";
        if (registerResponse.ResponseCode == RegisterResponseEnum.RegisterOk)
        {
            PopUpMessage.Show("Account created successfully", PopUpMessage.ButtonsEnum.OK, () =>
            {
                TMP_InputField registerPasswordInput = SelectServerManager.Instance._registerWrapper.transform.Find("InputsWrapper/Password")?.GetComponent<TMP_InputField>();
                TMP_InputField loginPasswordInput = SelectServerManager.Instance._loginWrapper.transform.Find("InputsWrapper/Password")?.GetComponent<TMP_InputField>();
                loginPasswordInput.text = registerPasswordInput.text;
                SelectServerManager.Instance.CancelButtonRegisterPressed();
                SelectServerManager.Instance._loginWrapper.GetComponent<InputManager>().SelectInputField(1);
            });
            return;
        }

        Action onOk = null;
        switch (registerResponse.ResponseCode)
        {
            case RegisterResponseEnum.RegisterError:
                popUpText = "Registration error";
                break;
            case RegisterResponseEnum.RegisterInvalidInput:
                popUpText = "Invalid input";
                break;
            case RegisterResponseEnum.RegisterInvalidEmail:
                popUpText = "Invalid email";
                onOk = () => SelectServerManager.Instance._registerWrapper.GetComponent<InputManager>().SelectInputField(1);
                break;
            case RegisterResponseEnum.RegisterInvalidVersion:
                popUpText = "Invalid version";
                onOk = () => _ = SelectServerManager.Instance.CancelButtonPressed();
                break;
            case RegisterResponseEnum.RegisterInvalidSerial:
                popUpText = "Invalid serial";
                onOk = () => _ = SelectServerManager.Instance.CancelButtonPressed();
                break;
            case RegisterResponseEnum.RegisterUserExists:
                popUpText = "Username or email already taken";
                onOk = () => SelectServerManager.Instance._registerWrapper.GetComponent<InputManager>().SelectInputField(0);
                break;
            case RegisterResponseEnum.RegisterTooManyAttempts:
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
