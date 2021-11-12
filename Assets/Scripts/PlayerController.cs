using UnityEngine;

// require component automatically adds components specified
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    // serialize field shows up in the inspector even if private
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 10f;
    [SerializeField]
    private float thrusterForceInit = 50f;

    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;
    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;
    private float thrusterFuelAmount = 1f;

    public float GetThrusterFuelAmount()
    {
        return thrusterFuelAmount;
    }

    [SerializeField]
    private LayerMask environmentMask;

    [Header("Spring settings:")]
    [SerializeField]
    private JointProjectionMode jointMode = JointProjectionMode.PositionAndRotation;
    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;

    // Component caching
    private PlayerMotor motor;
    private ConfigurableJoint joint;
    private Animator animator;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();

        SetJointSettings(jointSpring);
    }

    // Calculate movement velocity as a 3D vector
    void Update()
    {
        if(Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        // Establish a floating distance so that the player is going to hover over any object that has the EnvironmentMask Layer
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 100f, environmentMask))
        {
            joint.targetPosition = new Vector3(0f, -hit.point.y, 0f);
        }
        else
        {
            joint.targetPosition = new Vector3(0f, 0f, 0f);
        }
        
        // Calculate movement velocity as a 3D vector (axis between -1 and 1)
        // Using zMovSmooth to smooth out movement animations on the animator
        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");
        float zMovSmooth = Input.GetAxis("Vertical");

        Vector3 movHorizontal = transform.right * xMov; // transform.right = (1,0,0)
        Vector3 movVertical = transform.forward * zMov; // transfor.forward = (0,0,1)

        // Final movement vector
        Vector3 velocity = (movHorizontal + movVertical).normalized * speed; // normalized returns 1 as the vector length

        // Animate movement
        animator.SetFloat("ForwardVelocity", zMovSmooth);

        // Apply movement
        motor.Move(velocity);

        // Calculate rotation as a 3D vector (looking around)
        float yRot = Input.GetAxisRaw("Mouse X");

        Vector3 rotation = new Vector3(0f, yRot, 0f) * lookSensitivity;

        // Apply rotation
        motor.Rotate(rotation);

        // Calculate camera rotation as a 3D vector (looking around)
        float xRot = Input.GetAxisRaw("Mouse Y");

        float cameraRotationX = xRot * lookSensitivity;

        // Apply camera rotation
        motor.RotateCamera(cameraRotationX);

        // Calculate Thruster Force based on player input
        // if we dont hit "Jump", set thruster to 0
        Vector3 thrusterForce = Vector3.zero;
        if (Input.GetButton("Jump") && thrusterFuelAmount > 0f)
        {
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

            if (thrusterFuelAmount >= 0.01f)
            {
                thrusterForce = Vector3.up * thrusterForceInit;
                // disable joint
                SetJointSettings(0f);
            }
        }
        else
        {
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;

            SetJointSettings(jointSpring);
        }

        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0f, 1f);

        // Apply the thruster force
        motor.ApplyThruster(thrusterForce);
    }

    // Enable or Disable joint by modifying jointSpring to either 0 or initial value
    private void SetJointSettings(float _jointSpring)
    {
        joint.projectionMode = jointMode;
        joint.yDrive = new JointDrive { positionSpring = _jointSpring, maximumForce = jointMaxForce };
    }    
}
