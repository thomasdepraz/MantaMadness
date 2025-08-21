using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class DecalsIndicator : MonoBehaviour
{
    [Header("Materials")]
    public Material[] decalTextures;

    private InputActionMap playerActionsMap;
    [SerializeField] private DecalProjector decal;

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
            SetMaterial(0);
        }
        else if (device is Gamepad gamepad) 
        {
            string displayName = gamepad.displayName.ToLower();

            if(displayName.Contains("dualshock") || displayName.Contains("dualsense") || displayName.Contains("playstation"))
            {
                //SET PLAYSTATION MAT
                SetMaterial(1);
            }
            else
            {
                //SET XBOX MAT
                SetMaterial(2);
            }
        }
    }
    private void SetMaterial(int matID)
    {
        if (decalTextures[matID] != null)
        {
            decal.material = decalTextures[matID];
        }
    }

}
