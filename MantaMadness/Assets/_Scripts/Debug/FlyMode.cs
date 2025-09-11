using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

public class FlyMode : MonoBehaviour
{
   public static FlyMode instance;
   [Header("Vitesse de déplacement")]
   public float moveSpeed = 10f;
   public float boostMultiplier = 2f;

   [Header("Sensibilité de la souris")]
   public float mouseSensitivity = 2f;

   float rotationX = 0f;
   float rotationY = 0f;

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

            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

            // --- Déplacement avec ZQSD ---
            float speed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) speed *= boostMultiplier;

            Vector3 direction = new Vector3(
                Input.GetAxisRaw("Horizontal"), // Q/D
                0,
                Input.GetAxisRaw("Vertical")    // Z/S
            );

            Vector3 move = transform.TransformDirection(direction).normalized * speed * Time.deltaTime;
            transform.position += move;

            // Monter/Descendre avec espace/ctrl
            if (Input.GetKey(KeyCode.Space))
                transform.position += Vector3.up * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftControl))
                transform.position += Vector3.down * speed * Time.deltaTime;

            // Quitter le mode "fly" en débloquant la souris avec ESC
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
        }
    }

    public void SwitchCamMode()
    {
        if (isEnabled == true)
        {
            Game.Instance.player.ForceLock(false);
            isEnabled = false;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            flyCam.enabled = false;

        }
        else if (isEnabled == false)
        {
            Game.Instance.player.ForceLock(true);
            isEnabled = true;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            flyCam.enabled = true;
        }

    }
}
