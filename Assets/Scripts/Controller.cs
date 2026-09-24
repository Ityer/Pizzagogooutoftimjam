using UnityEngine;
using UnityEngine.InputSystem;

public class DeliveryTruck : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private InputActionReference gas;
    [SerializeField] private InputActionReference left;
    [SerializeField] private InputActionReference right;
    [SerializeField] private InputActionReference brake;

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

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        gas.action.Enable();
        left.action.Enable();
        right.action.Enable();
        brake.action.Enable();
    }

    private void OnDisable()
    {
        gas.action.Disable();
        left.action.Disable();
        right.action.Disable();
        brake.action.Disable();
    }

    private void FixedUpdate()
    {
        Drive();
        Steer();
        ApplyLateralGrip();
        LimitAirTime();
    }

    private void Drive()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;

        float forwardSpeed = Vector3.Dot(velocity, transform.forward);

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

        if (!gas.action.IsPressed() && !brake.action.IsPressed())
        {
            if (velocity.magnitude > 0.1f)
            {
                rb.AddForce(
                    -velocity.normalized * coastDeceleration,
                    ForceMode.Acceleration
                );
            }
        }


        if (forwardSpeed > maxSpeed)
        {
            Vector3 limitedVelocity =
                transform.forward * maxSpeed;

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

        float forwardSpeed = Vector3.Dot(velocity, transform.forward);

        if (forwardSpeed < 0f)
        {
            steering *= -1f;
        }

        float turnAmount =
            steering *
            steeringSpeed *
            Time.fixedDeltaTime;

        transform.Rotate(0f, turnAmount, 0f);
    }

    private void ApplyLateralGrip()
    {
        Vector3 localVelocity =
            transform.InverseTransformDirection(rb.linearVelocity);

        localVelocity.x = Mathf.Lerp(
            localVelocity.x,
            0f,
            lateralGrip * Time.fixedDeltaTime
        );

        Vector3 correctedVelocity =
            transform.TransformDirection(localVelocity);

        correctedVelocity.y = rb.linearVelocity.y;

        rb.linearVelocity = correctedVelocity;
    }

    private void LimitAirTime()
    {
        Vector3 velocity = rb.linearVelocity;

        if (velocity.y > maxUpwardVelocity)
        {
            velocity.y = maxUpwardVelocity;
            rb.linearVelocity = velocity;
        }
    }
}