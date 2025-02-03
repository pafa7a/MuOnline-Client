using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopUpMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button cancelButton;

    private Action onOkClicked;
    private Action onCancelClicked;

    private static GameObject previousSelectedObject; // Store last selected UI element
    public static bool IsActive = false;

    public enum ButtonsEnum
    {
        OK,
        CANCEL,
        OK_CANCEL
    }

    void Start()
    {
        // Set pop-up as active
        IsActive = true;
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OkButtonClicked();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelButtonClicked();
        }
    }

    public static PopUpMessage Show(string message, ButtonsEnum buttons = ButtonsEnum.OK, Action onOk = null, Action onCancel = null)
    {
        // Load prefab
        var prefab = Resources.Load<PopUpMessage>("Prefabs/PopUpMessage");
        if (prefab == null)
        {
            Debug.LogError("PopUpMessage prefab not found in Resources/Prefabs!");
            return null;
        }

        // Store currently selected UI element before opening the pop-up
        previousSelectedObject = EventSystem.current.currentSelectedGameObject;

        // Instantiate pop-up
        var instance = Instantiate(prefab, GetCanvasTransform());
        if (instance == null)
        {
            Debug.LogError("Failed to instantiate PopUpMessage!");
            return null;
        }

        instance.Initialize(message, buttons, onOk, onCancel);

        // Deselect any active input field to prevent accidental typing
        EventSystem.current.SetSelectedGameObject(null);
        return instance;
    }

    private void Initialize(string message, ButtonsEnum buttons, Action onOk, Action onCancel)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        onOkClicked = onOk;
        onCancelClicked = onCancel;

        // Show/Hide buttons & Assign event listeners
        if (okButton != null)
        {
            okButton.gameObject.SetActive(buttons != ButtonsEnum.CANCEL);
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(OkButtonClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(buttons != ButtonsEnum.OK);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelButtonClicked);
        }
    }

    private void OkButtonClicked()
    {
        onOkClicked?.Invoke();
        Invoke(nameof(ClosePopUp), 0.1f);
    }

    private void CancelButtonClicked()
    {
        onCancelClicked?.Invoke();
        Invoke(nameof(ClosePopUpWithCancel), 0.1f);
    }

    private void ClosePopUp()
    {
        Destroy(gameObject);
        IsActive = false;
    }

    private void ClosePopUpWithCancel()
    {
        Destroy(gameObject);
        IsActive = false;

        // Restore focus to the previously selected input field after closing
        if (previousSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(previousSelectedObject);
            previousSelectedObject = null; // Clear reference to avoid unexpected behavior
        }
    }

    private static Transform GetCanvasTransform()
    {
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject == null)
        {
            Debug.LogError("Canvas not found in the scene!");
            return null;
        }

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas component found on the Canvas GameObject!");
            return null;
        }

        return canvas.transform;
    }
}
