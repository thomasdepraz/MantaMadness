using UnityEngine;
using UnityEngine.InputSystem;

public class ShopInteractDisplay : MonoBehaviour
{
    public Sprite inputSpriteKeyboard, inputSpritePlaystation, inputSpriteXbox;

    [SerializeField] private UnityEngine.UI.Image holdProgress;

    private InputActionMap playerActionsMap;
    //[SerializeField] private SpriteRenderer dialogRenderer;
    public GameObject[] m_container;

    private ShopStand currentShop;
    private float holdTime;
    [SerializeField] private float holdDuration = 1.5f;

    private InputAction interactAction;

    private void Start()
    {
        if (UIManager.Instance.shopInteractDisplay== null)
        {
            UIManager.Instance.shopInteractDisplay = this;
        }

        ToggleInterface(false);
    }

    private void OnEnable()
    {
        playerActionsMap = InputSystem.actions.FindActionMap("Player");

        interactAction = playerActionsMap.FindAction("Interact");

        //playerActionsMap.actionTriggered += OnActionPerformed;
    }

    private void OnDisable()
    {
        //playerActionsMap.actionTriggered -= OnActionPerformed;
    }

    void Update()
    {
        if (currentShop == null)
            return;

        if (!currentShop.IsActive)
        {
            ClearShop();
            return;
        }

        if (interactAction.IsPressed())
        {
            holdTime += Time.deltaTime;

            holdProgress.fillAmount = holdTime / holdDuration;

            if (holdTime >= holdDuration)
            {
                currentShop.TryBuy();

                holdTime = 0;
                holdProgress.fillAmount = 0;
            }
        }
        else
        {
            holdTime = 0;
            holdProgress.fillAmount = 0;
        }
    }

    //private void OnActionPerformed(InputAction.CallbackContext context)
    //{
    //    InputDevice device = context.control.device;

    //    if (device is Keyboard)
    //    {
    //        // SET MAT TO KEYBOARD
    //        dialogRenderer.sprite = inputSpriteKeyboard;
    //    }
    //    else if (device is Gamepad gamepad)
    //    {
    //        string displayName = gamepad.displayName.ToLower();

    //        if (displayName.Contains("dualshock") || displayName.Contains("dualsense") || displayName.Contains("playstation"))
    //        {
    //            //SET PLAYSTATION MAT
    //            dialogRenderer.sprite = inputSpritePlaystation;
    //        }
    //        else
    //        {
    //            //SET XBOX MAT
    //            dialogRenderer.sprite = inputSpriteXbox;
    //        }
    //    }
    //}

    public void ToggleInterface(bool toggle)
    {
        foreach (GameObject container in m_container)
        {
            container.SetActive(toggle);
        }
    }

    public void ShowShop(ShopStand shop)
    {
        ToggleInterface(true);
        currentShop = shop;
    }

    public void ClearShop()
    {
        currentShop = null;
        holdTime = 0;
        holdProgress.fillAmount = 0;

        ToggleInterface(false);
    }
}
