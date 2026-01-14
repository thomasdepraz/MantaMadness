using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class DialogInteractDisplay : MonoBehaviour
{
    public Sprite inputSpriteKeyboard, inputSpritePlaystation, inputSpriteXbox;

    private InputActionMap playerActionsMap;
    [SerializeField] private SpriteRenderer dialogRenderer;
    public GameObject[] m_container;


    private void Start()
    {
        if (UIManager.Instance.dialogInteractDisplay == null)
        {
            UIManager.Instance.dialogInteractDisplay = this;
        }

        ToggleInterface(false);
    }

    private void OnEnable()
    {
        playerActionsMap = InputSystem.actions.FindActionMap("Player");
        playerActionsMap.actionTriggered += OnActionPerformed;
    }

    private void OnDisable()
    {
        playerActionsMap.actionTriggered -= OnActionPerformed;
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;

        if (device is Keyboard)
        {
            // SET MAT TO KEYBOARD
            dialogRenderer.sprite = inputSpriteKeyboard;
        }
        else if (device is Gamepad gamepad)
        {
            string displayName = gamepad.displayName.ToLower();

            if (displayName.Contains("dualshock") || displayName.Contains("dualsense") || displayName.Contains("playstation"))
            {
                //SET PLAYSTATION MAT
                dialogRenderer.sprite = inputSpritePlaystation;
            }
            else
            {
                //SET XBOX MAT
                dialogRenderer.sprite = inputSpriteXbox;
            }
        }
    }

    public void ToggleInterface(bool toggle)
    {
        foreach (GameObject container in m_container)
        {
            container.SetActive(toggle);
        }
    }
}
