using UnityEngine;
using UnityEngine.InputSystem;

public class DeliveryTruck : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private InputActionReference gas;
    [SerializeField] private InputActionReference left;
    [SerializeField] private InputActionReference right;
    [SerializeField] private InputActionReference brake;
    [SerializeField] private InputActionReference flip;
    [SerializeField] private InputActionReference horn;

    [Header("Driving")]
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float reverseAcceleration = 7f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float maxReverseSpeed = 8f;
    [SerializeField] private float brakeForce = 25f;
    [SerializeField] private float steeringSpeed = 80f;
    [SerializeField] private float steeringMinSpeed = 0.5f;
    [SerializeField] private float lateralGrip = 8f;
    [SerializeField] private float coastDeceleration = 3f;
    [SerializeField] private float maxUpwardVelocity = 2f;
    [SerializeField] private float speedPenaltyPerPizza = 1.5f;

    [Header("Ground Detection")]
    [SerializeField] private float groundNormalThreshold = 0.35f;

    [Header("Engine Sound")]
    [SerializeField] private AudioSource engineAudio;
    [SerializeField] private float minEnginePitch = 0.7f;
    [SerializeField] private float maxEnginePitch = 1.5f;
    [SerializeField] private float engineVolume = 0.435f;
    [SerializeField] private float engineSmoothSpeed = 5f;

    [Header("Horn")]
    [SerializeField] private AudioSource hornAudio;

    private bool isGrounded;
    private PizzaCargo pizzaCargo;

    private DeliveryManager deliveryManager;
    private Rigidbody rb;
    private Transform tf;

    private void Start()
    {
        deliveryManager = FindAnyObjectByType<DeliveryManager>();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        tf = GetComponent<Transform>();
        pizzaCargo = GetComponent<PizzaCargo>();

        rb.centerOfMass = new Vector3(
            0f,
            -0.5f,
            0f
        );

        rb.constraints |= RigidbodyConstraints.FreezeRotationZ;

        if (engineAudio != null)
        {
            engineAudio.volume = engineVolume;
            engineAudio.pitch = minEnginePitch;
        }
    }

    private void OnEnable()
    {
        gas.action.Enable();
        left.action.Enable();
        right.action.Enable();
        brake.action.Enable();
        flip.action.Enable();
        horn.action.Enable();
    }

    private void OnDisable()
    {
        gas.action.Disable();
        left.action.Disable();
        right.action.Disable();
        brake.action.Disable();
        flip.action.Disable();
        horn.action.Disable();

        if (hornAudio != null)
        {
            hornAudio.Stop();
        }
    }

    private void Update()
    {
        if (deliveryManager == null)
            return;

        if (!deliveryManager.IsGameStarted())
            return;

        if (horn.action.WasPressedThisFrame())
        {
            StartHorn();
        }

        if (horn.action.WasReleasedThisFrame())
        {
            StopHorn();
        }
    }

    private void FixedUpdate()
    {
        if (deliveryManager == null)
            return;

        if (deliveryManager.IsGameStarted())
        {
            Drive();
            Steer();
            ApplyLateralGrip();
            LimitAirTime();
            FlipTruck();
            UpdateEngineSound();
        }
    }

    private void Drive()
    {
        if (!isGrounded)
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;

        float forwardSpeed =
            Vector3.Dot(
                velocity,
                transform.forward
            );

        if (gas.action.IsPressed())
        {
            if (forwardSpeed > -0.1f)
            {
                rb.AddForce(
                    transform.forward * acceleration,
                    ForceMode.Acceleration
                );
            }
        }

        if (brake.action.IsPressed())
        {
            if (forwardSpeed > 0.2f)
            {
                rb.AddForce(
                    -transform.forward * brakeForce,
                    ForceMode.Acceleration
                );
            }
            else
            {
                rb.AddForce(
                    -transform.forward * reverseAcceleration,
                    ForceMode.Acceleration
                );
            }
        }

        if (!gas.action.IsPressed() &&
            !brake.action.IsPressed())
        {
            if (velocity.magnitude > 0.1f)
            {
                rb.AddForce(
                    -velocity.normalized * coastDeceleration,
                    ForceMode.Acceleration
                );
            }
        }

        float currentMaxSpeed = maxSpeed;

        if (pizzaCargo != null)
        {
            currentMaxSpeed -=
                pizzaCargo.GetPizzaCount() *
                speedPenaltyPerPizza;
        }

        currentMaxSpeed = Mathf.Max(
            5f,
            currentMaxSpeed
        );

        if (forwardSpeed > currentMaxSpeed)
        {
            Vector3 limitedVelocity =
                transform.forward * currentMaxSpeed;

            rb.linearVelocity = new Vector3(
                limitedVelocity.x,
                rb.linearVelocity.y,
                limitedVelocity.z
            );
        }

        if (forwardSpeed < -maxReverseSpeed)
        {
            Vector3 limitedVelocity =
                -transform.forward * maxReverseSpeed;

            rb.linearVelocity = new Vector3(
                limitedVelocity.x,
                rb.linearVelocity.y,
                limitedVelocity.z
            );
        }
    }

    private void Steer()
    {
        float steering = 0f;

        if (left.action.IsPressed())
        {
            steering -= 1f;
        }

        if (right.action.IsPressed())
        {
            steering += 1f;
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;

        if (velocity.magnitude < steeringMinSpeed)
        {
            return;
        }

        float forwardSpeed =
            Vector3.Dot(
                velocity,
                transform.forward
            );

        if (forwardSpeed < 0f)
        {
            steering *= -1f;
        }

        float turnAmount =
            steering *
            steeringSpeed *
            Time.fixedDeltaTime;

        Quaternion turnRotation =
            Quaternion.AngleAxis(
                turnAmount,
                Vector3.up
            );

        rb.MoveRotation(
            turnRotation * rb.rotation
        );
    }

    private void ApplyLateralGrip()
    {
        Vector3 localVelocity =
            transform.InverseTransformDirection(
                rb.linearVelocity
            );

        localVelocity.x = Mathf.Lerp(
            localVelocity.x,
            0f,
            lateralGrip *
            Time.fixedDeltaTime
        );

        Vector3 correctedVelocity =
            transform.TransformDirection(
                localVelocity
            );

        correctedVelocity.y =
            rb.linearVelocity.y;

        rb.linearVelocity =
            correctedVelocity;
    }

    private void LimitAirTime()
    {
        Vector3 velocity =
            rb.linearVelocity;

        if (velocity.y > maxUpwardVelocity)
        {
            velocity.y =
                maxUpwardVelocity;

            rb.linearVelocity =
                velocity;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y >
                groundNormalThreshold)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void FlipTruck()
    {
        if (!flip.action.WasPressedThisFrame())
            return;

        Vector3 currentRotation =
            transform.eulerAngles;

        transform.rotation =
            Quaternion.Euler(
                0f,
                currentRotation.y,
                0f
            );

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;
    }

    private void UpdateEngineSound()
    {
        if (engineAudio == null)
            return;

        float speed =
            rb.linearVelocity.magnitude;

        float maxPossibleSpeed =
            maxSpeed;

        if (pizzaCargo != null)
        {
            maxPossibleSpeed -=
                pizzaCargo.GetPizzaCount() *
                speedPenaltyPerPizza;
        }

        maxPossibleSpeed =
            Mathf.Max(
                5f,
                maxPossibleSpeed
            );

        float speedPercent =
            Mathf.Clamp01(
                speed / maxPossibleSpeed
            );

        float targetPitch =
            Mathf.Lerp(
                minEnginePitch,
                maxEnginePitch,
                speedPercent
            );

        engineAudio.pitch =
            Mathf.Lerp(
                engineAudio.pitch,
                targetPitch,
                engineSmoothSpeed *
                Time.fixedDeltaTime
            );

        if (!engineAudio.isPlaying)
        {
            engineAudio.Play();
        }
    }

    private void StartHorn()
    {
        if (hornAudio == null)
            return;

        hornAudio.Stop();
        hornAudio.Play();
    }

    private void StopHorn()
    {
        if (hornAudio == null)
            return;

        hornAudio.Stop();
    }
}