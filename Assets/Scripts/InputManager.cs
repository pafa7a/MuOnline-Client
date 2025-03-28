using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
  public List<Selectable> inputFields;
  public int currentInputIndex = 0;

  void Start()
  {
    Init();
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Tab) && inputFields.Count > 0)
    {
      currentInputIndex = (currentInputIndex + 1) % inputFields.Count;
      SelectInputField(currentInputIndex);
    }
  }

  public void Init()
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

  public void SelectInputField(int index)
  {
    if (index < 0 || index >= inputFields.Count) return;

    EventSystem.current.SetSelectedGameObject(inputFields[index].gameObject);
    TMP_InputField inputField = inputFields[index].GetComponent<TMP_InputField>();

    if (inputField != null)
    {
      inputField.ActivateInputField();
      inputField.caretPosition = inputField.text.Length;
    }

    currentInputIndex = index; // Ensure index is updated correctly
  }
}
