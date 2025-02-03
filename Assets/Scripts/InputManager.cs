using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public List<Selectable> inputFields;
    private int currentInputIndex = 0;
    public static InputManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (inputFields == null || inputFields.Count == 0) return;

        TMP_InputField usernameField = inputFields[0].GetComponent<TMP_InputField>();

        // If username is predefined, autofocus on password field (if exists)
        if (usernameField != null && !string.IsNullOrEmpty(usernameField.text) && inputFields.Count > 1)
        {
            SelectInputField(1); // Move to password field
        }
        else
        {
            SelectInputField(0); // Keep focus on username if empty
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && inputFields.Count > 0)
        {
            currentInputIndex = (currentInputIndex + 1) % inputFields.Count;
            SelectInputField(currentInputIndex);
        }
    }

    public static void SelectInputField(int index)
    {
        if (index < 0 || index >= instance.inputFields.Count) return;

        EventSystem.current.SetSelectedGameObject(instance.inputFields[index].gameObject);
        TMP_InputField inputField = instance.inputFields[index].GetComponent<TMP_InputField>();
        
        if (inputField != null)
        {
            inputField.ActivateInputField();
            inputField.caretPosition = inputField.text.Length;
        }

        instance.currentInputIndex = index; // Ensure index is updated correctly
    }
}
