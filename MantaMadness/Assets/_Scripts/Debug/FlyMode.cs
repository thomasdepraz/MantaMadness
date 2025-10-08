using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

public class FlyMode : MonoBehaviour
{
   public static FlyMode instance;
   [Header("Vitesse de déplacement")]
   public float moveSpeed = 10f;
   public float boostMultiplier = 2f;
    public float smoothSpeed = 10f;

    [Header("Sensibilité de la souris")]
   public float mouseSensitivity = 2f;
    public float rotationSmoothSpeed = 10f;

    float rotationX = 0f;
   float rotationY = 0f;

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

    void Update()
    {
        if (isEnabled == true)
        {
            // --- Rotation de la caméra avec la souris ---
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f); // Empêche la caméra de se retourner

            targetRotation = Quaternion.Euler(rotationY, rotationX, 0f);

            //transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

            // --- Déplacement avec ZQSD ---
            float speed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) speed *= boostMultiplier;

            Vector3 direction = new Vector3(
                Input.GetAxisRaw("Horizontal"), // Q/D
                0,
                Input.GetAxisRaw("Vertical")    // Z/S
            );

            Vector3 move = (transform.TransformDirection(direction).normalized) * speed * Time.deltaTime;

            if (Input.GetKey(KeyCode.E))
                move += Vector3.up * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A))
                move -= Vector3.up * speed * Time.deltaTime;

            targetPosition += move;

            // --- Appliquer le lissage ---
                transform.position = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref currentVelocity, 1f / smoothSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);

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
    }

    public void SwitchCamMode()
    {
        if (isEnabled == true)
        {
            Game.Instance.player.transform.position = flyCam.transform.position;
            Game.Instance.player.ForceLock(false);
            isEnabled = false;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            flyCam.enabled = false;
        }
        else if (isEnabled == false)
        {
            flyCam.transform.position = Game.Instance.player.transform.position;
            Game.Instance.player.ForceLock(true);
            isEnabled = true;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            flyCam.enabled = true;
            targetPosition = transform.position;
        }

    }
}
