using UnityEngine;

public enum FlyModeSpeed
{
    Default,
    Fast,
    Slow,
    VerySlow,
}

public class FlyMode : MonoBehaviour
{

    public static FlyMode instance;

    public bool disableFlyMode = false;
    [Header("Vitesse de déplacement")]
    private FlyModeSpeed speedMode;
    public float defaultSpeed = 50f;
    public float fastSpeed = 100f;
    public float slowSpeed = 20f;
    public float verySlowSpeed = 5f;

    public float boostMultiplier = 15f;
    public float smoothSpeed = 10f;
    public float smoothFactor = 10f;

    [Header("Sensibilité de la souris")]
    public float mouseSensitivity = 2f;
    public float rotationSmoothSpeed = 10f;

    float rotationX = 0f;
    float rotationY = 0f;
    bool smoothMode = false;

    Vector3 currentVelocity;
    Vector3 targetPosition;
    Quaternion targetRotation;


    bool isEnabled = false;
    public Camera flyCam;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Cacher le curseur et le verrouiller au centre de l'écran
        flyCam.enabled = false;
    }
#if UNITY_EDITOR
    void Update()
    {
        if(disableFlyMode == false)
        {
            if (isEnabled == true)
            {

                Game.Instance.player.transform.position = flyCam.transform.position;


                // --- Rotation de la caméra avec la souris ---
                rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
                rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                rotationY = Mathf.Clamp(rotationY, -90f, 90f); // Empêche la caméra de se retourner

                targetRotation = Quaternion.Euler(rotationY, rotationX, 0f);
                //transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
                float speed = 50f;
                // --- Déplacement avec ZQSD ---

                switch (speedMode)
                {
                    case FlyModeSpeed.Slow:
                        speed = slowSpeed;
                        break;
                    case FlyModeSpeed.Fast:
                        speed = fastSpeed;
                        break;
                    case FlyModeSpeed.VerySlow:
                        speed = verySlowSpeed;
                        break;
                    case FlyModeSpeed.Default:
                        speed = defaultSpeed;
                        break;
                }

                if (Input.GetKey(KeyCode.LeftShift)) speed *= boostMultiplier;
                if (Input.GetKey(KeyCode.LeftShift)) smoothSpeed *= boostMultiplier;


                Vector3 direction = new Vector3(
                    Input.GetAxisRaw("Horizontal"), // Q/D
                    0,
                    Input.GetAxisRaw("Vertical")    // Z/S
                );

                transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
                Vector3 move = transform.TransformDirection(direction).normalized * speed * Time.deltaTime;
                transform.position += move;

                if (Input.GetKey(KeyCode.E)) transform.position += Vector3.up * speed * Time.deltaTime;
                if (Input.GetKey(KeyCode.X)) transform.position += Vector3.down * speed * Time.deltaTime;


                // Quitter le mode "fly" en débloquant la souris avec ESC
                if (Input.GetKeyDown(KeyCode.Escape))
                {

                    UnityEngine.Cursor.lockState = CursorLockMode.None;
                    UnityEngine.Cursor.visible = true;
                }

            }
            else if (isEnabled == false)
            {
                flyCam.transform.position = Game.Instance.player.transform.position;
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                SwitchCamMode();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                SwitchCamSpeed();
            }
        }
    }

    public void SwitchCamMode()
    {
        if (isEnabled == true)
        {
            Game.Instance.player.transform.position = flyCam.transform.position;
            Game.Instance.player.State = ControllerState.FALLING;
            Game.Instance.player.ForceLock(false);
            Game.Instance.player.togglePlayerBodyVisual(true);
            isEnabled = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = true;
            flyCam.enabled = false;
            smoothMode = false;
        }
        else if (isEnabled == false)
        {
            flyCam.transform.position = Game.Instance.player.transform.position;
            Game.Instance.player.ForceLock(true);
            Game.Instance.player.State = ControllerState.DEBUG;
            Game.Instance.player.togglePlayerBodyVisual(false);
            isEnabled = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = false;
            flyCam.enabled = true;
            targetPosition = transform.position;
        }

    }
#endif
    public void SwitchCamSpeed()
    {
        int enumLength = System.Enum.GetValues(typeof(FlyModeSpeed)).Length;
        int next = ((int)speedMode + 1) % enumLength;
        speedMode = (FlyModeSpeed)next;
    }

}
