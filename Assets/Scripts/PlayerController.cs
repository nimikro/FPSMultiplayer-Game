using UnityEngine;

// require component automatically adds components specified
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    // serialize field shows up in the inspector even if private
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 10f;

    private PlayerMotor motor;

    private void Start()
    {
        motor = GetComponent<PlayerMotor>();
    }

    // Calculate movement velocity as a 3D vector
    private void Update()
    {
        // axis between -1 and 1
        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");

        Vector3 movHorizontal = transform.right * xMov; // transform.right = (1,0,0)
        Vector3 movVertical = transform.forward * zMov; // transfor.forward = (0,0,1)

        // Final movement vector
        Vector3 velocity = (movHorizontal + movVertical).normalized * speed; // normalized returns 1 as the vector length

        // Apply movement
        motor.Move(velocity);

        // Calculate rotation as a 3D vector (looking around)
        float yRot = Input.GetAxisRaw("Mouse X");

        Vector3 rotation = new Vector3(0f, yRot, 0f) * lookSensitivity;

        // Apply rotation
        motor.Rotate(rotation);

        // Calculate camera rotation as a 3D vector (looking around)
        float xRot = Input.GetAxisRaw("Mouse Y");

        Vector3 cameraRotation = new Vector3(xRot, 0f, 0f) * lookSensitivity;

        // Apply camera rotation
        motor.RotateCamera(cameraRotation);
    }
}
