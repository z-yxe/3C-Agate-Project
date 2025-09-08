using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputManager input;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float walkSprintTransition;
    [SerializeField] private float rotationSmoothTime;

    [SerializeField] private float jumpForce;
    [SerializeField] private Transform groundDetector;
    [SerializeField] private float detectorRadius;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 upperStepOffset;
    [SerializeField] private float stepCheckerDistance;
    [SerializeField] private float stepForce;

    [SerializeField] private Transform climbDetector;
    [SerializeField] private float climbCheckDistance;
    [SerializeField] private LayerMask climbableLayer;
    [SerializeField] private Vector3 climbOffset;
    [SerializeField] private float climbSpeed;

    private Rigidbody rb;
    private float rotationSmoothVelocity;
    private float speed;
    private bool isGrounded;
    private PlayerStance playerStance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        speed = walkSpeed;
        playerStance = PlayerStance.Stand;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
    }

    private void Start()
    {
        input.OnMoveInput += Move;
        input.OnSprintInput += Sprint;
        input.OnJumpInput += Jump;
        input.OnClimbInput += StartClimb;
        input.OnCancelClimb += CancelClimb;
    }

    private void OnDestroy()
    {
        input.OnMoveInput -= Move;
        input.OnSprintInput -= Sprint;
        input.OnJumpInput -= Jump;
        input.OnClimbInput -= StartClimb;
        input.OnCancelClimb -= CancelClimb;
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerstanding = playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = playerStance == PlayerStance.Climb;

        if (isPlayerstanding)
        {
            if (axisDirection.magnitude >= 0.1f)
            {
                float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref rotationSmoothVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                rb.AddForce(movementDirection * speed * Time.deltaTime);
            }
        }
        else if (isPlayerClimbing)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 verttical = axisDirection.y * transform.up;
            movementDirection = horizontal + verttical;
            rb.AddForce(movementDirection * speed * Time.deltaTime);
        }
    }

    private void Sprint(bool isSprint)
    {
        if (isSprint)
        {
            if (speed < sprintSpeed)
            {
                speed += walkSprintTransition * Time.deltaTime;
            }
        }
        else
        {
            if (speed > walkSpeed)
            {
                speed -= walkSprintTransition * Time.deltaTime;
            }
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            Vector3 jumpDirection = Vector3.up;
            rb.AddForce(jumpDirection * jumpForce * Time.deltaTime);
        }
    }

    private void CheckIsGrounded()
    {
        isGrounded = Physics.CheckSphere(groundDetector.position, detectorRadius, groundLayer);
    }

    private void CheckStep()
    {
        bool isHitLowerStep = Physics.Raycast(groundDetector.position, transform.forward, stepCheckerDistance);
        bool isHitUpperStep = Physics.Raycast(groundDetector.position + upperStepOffset, transform.forward, stepCheckerDistance);

        if (isHitLowerStep && !isHitUpperStep)
        {
            rb.AddForce(0f, stepForce * Time.deltaTime, 0f);
        }
    }

    private void StartClimb()
    {
        bool isInFrontOfClimbingWall = Physics.Raycast(climbDetector.position, transform.forward, out RaycastHit hit, climbCheckDistance, climbableLayer);
        bool isNotClimbing = playerStance != PlayerStance.Climb;

        if (isInFrontOfClimbingWall && isNotClimbing && isGrounded)
        {
            Vector3 offset = (transform.forward * climbOffset.z) + (Vector3.up * climbOffset.y);
            transform.position = hit.point - offset;
            playerStance = PlayerStance.Climb;
            rb.useGravity = false;
            speed = climbSpeed;
        }
    }

    private void CancelClimb()
    {
        if (playerStance == PlayerStance.Climb)
        {
            playerStance = PlayerStance.Stand;
            rb.useGravity = true;
            transform.position -= transform.forward * 0.5f;
            speed = walkSpeed;
        }
    }
}
