using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputManager input;
    [SerializeField] private PlayerAudioManager playerAudio;

    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float crouchSpeed;
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

    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private float glideSpeed;
    [SerializeField] private float airDrag;
    [SerializeField] private Vector3 glideRotationSpeed;
    [SerializeField] private float minGlideRotationX;
    [SerializeField] private float maxGlideRotationX;

    [SerializeField] private float resetComboInterval;
    [SerializeField] private Transform hidDetector;
    [SerializeField] private float hitDetectorradius;
    [SerializeField] private LayerMask hitLayer;

    [SerializeField] private Transform resetCheckpointPosition;

    private Rigidbody rb;
    private PlayerStance playerStance;
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private Coroutine resetCombo;
    private Vector3 rotationDegree = Vector3.zero;
    private float rotationSmoothVelocity;
    private float speed;
    private bool isGrounded;
    private bool isPunching;
    private int combo;

    public void ResetPositionToCheckPoint()
    {
        if (resetCheckpointPosition != null)
        {
            transform.position = resetCheckpointPosition.position;
            transform.rotation = resetCheckpointPosition.rotation;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        speed = walkSpeed;
        playerStance = PlayerStance.Stand;
        HideAndLockCursor();
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
        Glide();
    }

    private void OnEnable()
    {
        input.OnMoveInput += Move;
        input.OnSprintInput += Sprint;
        input.OnJumpInput += Jump;
        input.OnClimbInput += StartClimb;
        input.OnCancelClimb += CancelClimb;
        input.OnCrouchInput += Crouch;
        input.OnGlideInput += StartGlide;
        input.OnCancelGlide += CancelGlide;
        input.OnPunchInput += Punch;
        cameraManager.OnChangePrespective += ChangePrespective;
    }

    private void OnDestroy()
    {
        input.OnMoveInput -= Move;
        input.OnSprintInput -= Sprint;
        input.OnJumpInput -= Jump;
        input.OnClimbInput -= StartClimb;
        input.OnCancelClimb -= CancelClimb;
        input.OnCrouchInput -= Crouch;
        input.OnGlideInput -= StartGlide;
        input.OnCancelGlide -= CancelGlide;
        input.OnPunchInput -= Punch;
        cameraManager.OnChangePrespective -= ChangePrespective;
    }

    private void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerstanding = playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = playerStance == PlayerStance.Climb;
        bool isPlayerCrouch = playerStance == PlayerStance.Crouch;
        bool isPlayerGliding = playerStance == PlayerStance.Glide;

        if (!isPunching && (isPlayerstanding || isPlayerCrouch))
        {
            switch (cameraManager.cameraState)
            {
                case CameraState.ThirdPerson:
                    if (axisDirection.magnitude >= 0.1f)
                    {
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref rotationSmoothVelocity, rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        rb.AddForce(movementDirection.normalized * speed * Time.deltaTime);
                    }
                    break;

                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
                    Vector3 verticalDirection = axisDirection.y * transform.forward;
                    Vector3 horizontalDirection = axisDirection.x * transform.right;
                    movementDirection = verticalDirection + horizontalDirection;
                    rb.AddForce(movementDirection.normalized * speed * Time.deltaTime);
                    break;

                default:
                    break;
            }

            Vector3 velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            animator.SetFloat("Velocity", velocity.magnitude * axisDirection.magnitude);
            animator.SetFloat("VelocityX", velocity.magnitude * axisDirection.x);
            animator.SetFloat("VelocityZ", velocity.magnitude * axisDirection.y);
        }
        else if (isPlayerClimbing)
        {
            Vector3 horizontal = Vector3.zero;
            Vector3 verttical = Vector3.zero;

            Vector3 checkRightPosition = transform.position + (transform.up * 1) + (transform.right * 0.75f);
            Vector3 checkLeftPosition = transform.position + (transform.up * 1) + (-transform.right * 0.75f);
            Vector3 checkUpPosition = transform.position + (transform.up * 2.5f);
            Vector3 checkDownPosition = transform.position + (-transform.up * 0.25f);

            bool isAbleClimbRight = Physics.Raycast(checkRightPosition, transform.forward, climbCheckDistance, climbableLayer);
            bool isAbleClimbLeft = Physics.Raycast(checkLeftPosition, transform.forward, climbCheckDistance, climbableLayer);
            bool isAbleClimbUp = Physics.Raycast(checkUpPosition, transform.forward, climbCheckDistance, climbableLayer);
            bool isAbleClimbDown = Physics.Raycast(checkDownPosition, transform.forward, climbCheckDistance, climbableLayer);

            if ((isAbleClimbLeft && (axisDirection.x < 0)) || (isAbleClimbRight && (axisDirection.x > 0)))
            {
                horizontal = axisDirection.x * transform.right;
            }
            if ((isAbleClimbDown && (axisDirection.y < 0)) || (isAbleClimbUp && (axisDirection.y > 0)))
            {
                verttical = axisDirection.y * transform.up;
            }

            movementDirection = horizontal + verttical;
            rb.AddForce(movementDirection * speed * Time.deltaTime);

            Vector3 velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0f);
            animator.SetFloat("ClimbVelocityX", velocity.magnitude * axisDirection.x);
            animator.SetFloat("ClimbVelocityY", velocity.magnitude * axisDirection.y);
        }
        else if (isPlayerGliding)
        {
            rotationDegree.x += glideRotationSpeed.x * axisDirection.y * Time.deltaTime;
            rotationDegree.x = Mathf.Clamp(rotationDegree.x, minGlideRotationX, maxGlideRotationX);
            rotationDegree.z += glideRotationSpeed.z * axisDirection.x * Time.deltaTime;
            rotationDegree.y += glideRotationSpeed.y * axisDirection.x * Time.deltaTime;
            transform.rotation = Quaternion.Euler(rotationDegree);
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
        if (isGrounded && !isPunching)
        {
            Vector3 jumpDirection = Vector3.up;
            rb.AddForce(jumpDirection * jumpForce);
            animator.SetTrigger("Jump");
        }
    }

    private void CheckIsGrounded()
    {
        isGrounded = Physics.CheckSphere(groundDetector.position, detectorRadius, groundLayer);
        animator.SetBool("IsGrounded", isGrounded);

        if (isGrounded)
        {
            CancelGlide();
        }
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
            animator.SetBool("IsClimbing", true);
            capsuleCollider.center = Vector3.up * 1.3f;

            Vector3 climbablePoint = hit.collider.bounds.ClosestPoint(transform.position);
            Vector3 direction = (climbablePoint - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);

            Vector3 offset = (transform.forward * climbOffset.z) + (Vector3.up * climbOffset.y);
            transform.position = hit.point - offset;
            playerStance = PlayerStance.Climb;
            rb.useGravity = false;
            speed = climbSpeed;

            cameraManager.SetFPPClampedCamera(true, transform.rotation.eulerAngles);
            cameraManager.SetTPPFieldOfView(70f);
        }
    }

    private void CancelClimb()
    {
        if (playerStance == PlayerStance.Climb)
        {
            animator.SetBool("IsClimbing", false);
            capsuleCollider.center = Vector3.up * 0.9f;

            playerStance = PlayerStance.Stand;
            rb.useGravity = true;
            transform.position -= transform.forward * 0.5f;
            speed = walkSpeed;

            cameraManager.SetFPPClampedCamera(false, transform.rotation.eulerAngles);
            cameraManager.SetTPPFieldOfView(40f);
        }
    }

    private void ChangePrespective()
    {
        animator.SetTrigger("ChangePerspective");
    }

    private void Crouch()
    {
        Vector3 checkerUpPosition = transform.position + (transform.up * 1.4f);
        bool isCantStand = Physics.Raycast(checkerUpPosition, transform.up, 0.25f, groundLayer);

        if (playerStance == PlayerStance.Stand)
        {
            playerStance = PlayerStance.Crouch;
            animator.SetBool("IsCrouch", true);
            speed = crouchSpeed;
            capsuleCollider.height = 1.3f;
            capsuleCollider.center = Vector3.up * 0.66f;
        }
        else if ((playerStance == PlayerStance.Crouch) && !isCantStand)
        {
            capsuleCollider.height = 1.8f;
            playerStance = PlayerStance.Stand;
            animator.SetBool("IsCrouch", false);
            speed = walkSpeed;
            capsuleCollider.height = 1.8f;
            capsuleCollider.center = Vector3.up * 0.9f;
        }
    }

    private void Glide()
    {
        if (playerStance == PlayerStance.Glide)
        {
            Vector3 playerRotation = transform.rotation.eulerAngles;
            float lift = playerRotation.x;
            Vector3 upForce = transform.up * (lift + airDrag);
            Vector3 forwardForce = transform.forward * glideSpeed;
            Vector3 totalForce = upForce + forwardForce;
            rb.AddForce(totalForce * Time.deltaTime);
        }
    }

    private void StartGlide()
    {
        if (playerStance != PlayerStance.Glide && !isGrounded)
        {
            rotationDegree = transform.rotation.eulerAngles;
            playerStance = PlayerStance.Glide;
            animator.SetBool("IsGliding", true);
            playerAudio.PlayGlideSfx();

            cameraManager.SetFPPClampedCamera(true, transform.rotation.eulerAngles);
        }
    }

    private void CancelGlide()
    {
        if (playerStance == PlayerStance.Glide)
        {
            playerStance = PlayerStance.Stand;
            animator.SetBool("IsGliding", false);
            playerAudio.StopGlideSfx();

            cameraManager.SetFPPClampedCamera(false, transform.rotation.eulerAngles);
        }
    }

    private void Punch()
    {
        if (!isPunching && playerStance == PlayerStance.Stand && isGrounded)
        {
            isPunching = true;
            if (combo < 3)
            {
                combo++;
            }
            else
            {
                combo = 1;
            }

            animator.SetInteger("Combo", combo);
            animator.SetTrigger("Punch");
        }
    }

    private void EndPunch()
    {
        isPunching = false;
        if (resetCombo != null)
        {
            StopCoroutine(resetCombo);
        }
        resetCombo = StartCoroutine(ResetCombo());
    }

    private IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(resetComboInterval);
        combo = 0;
    }

    private void Hit()
    {
        Collider[] hitObjects = Physics.OverlapSphere(hidDetector.position, hitDetectorradius, hitLayer);
        foreach (Collider destroyableObjects in hitObjects)
        {
            if (destroyableObjects.gameObject != null)
            {
                Destroy(destroyableObjects.gameObject);
            }
        }
    }
}
