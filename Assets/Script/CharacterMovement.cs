using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("Variables")] private float gravityScale = -9.8f;
    [SerializeField] private bool isFocused;
    [SerializeField] private Transform focusedTransform;
    [SerializeField] private float loseFocusDistance;
    [SerializeField] private bool isCrouched;
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private bool isPowerAttack = false;
    [SerializeField] private bool isParrying = false;
    
    [Header("Components")]
    private Animator playerAnimator;
    private CharacterController characterController;
    [Header("Movement Settings")]
    private float movementDamp = 0.1f;
    private float smoothTime = 0.1f;
    private float smoothedAngleVelocity;
    private float walkSpeed = 2f;
    private float runSpeed = 6f;
    private float currentSpeed = 0f;
    private float speedVelocity = 0f;
    private Vector3 smoothDirection = Vector3.zero;
    private Vector3 directionVelocity = Vector3.zero;
    private float directionSmoothTime = 0.1f;

    [Header("References")]
    [HideInInspector] public bool canMove = true; // This can be toggled by combat

    private void Start()
    {
        isFocused = false;
        playerAnimator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 rawDirection = new Vector3(horizontal, 0f, vertical);
        if (Input.GetKeyDown(KeyCode.C)) isCrouched = !isCrouched;

        HandleJumpandGravity();
        HandleAttack();
        if (!isFocused)
        {
            isAttacking = false;
            playerAnimator.SetLayerWeight(3, 0.5f);
            playerAnimator.SetBool("isFocused", isFocused);
            HandleMovement(rawDirection);
        }
        else
        {
            playerAnimator.SetBool("isFocused", isFocused);
            HandleFocusedMovement(rawDirection);
            if(Vector3.Distance(focusedTransform.position, transform.position) >= loseFocusDistance) isFocused = false;
        }
    }
    
    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) isPowerAttack = true;
        else if (Input.GetKeyUp(KeyCode.LeftShift)) isPowerAttack = false;
        
        if (Input.GetMouseButton(0) && !isParrying)
        {
            isFocused = true;
            isParrying = false;
            if (isAttacking)
            {
                playerAnimator.SetBool("DualSlash", true);
            }
            else
            {
                playerAnimator.SetLayerWeight(3, 1);
                if(isPowerAttack)
                    playerAnimator.Play("SinglePowerSlash");
                else
                    playerAnimator.Play("BasicSlash");
            }
        }

        if (Input.GetMouseButton(1))
        {
            //Perry
            isFocused = true;
            isAttacking = false;
            playerAnimator.SetBool("Parry", true);
            if(!isParrying) playerAnimator.Play("Parry");
            isParrying = true;
        }
        else
        {
            playerAnimator.SetBool("Parry", false);
            isParrying = false;
        }
        
    }

    public void animationTrigger(int animationID)
    {
        if (animationID == 0)
        {
            // Attack End...
            isAttacking = false;
            playerAnimator.SetBool("DualSlash", false);
        }else if (animationID == 1)
        {
            // On Attack, Normal Slash Single/Dual...
            isAttacking = true;
            Debug.Log("SingleDamage");
        }else if (animationID == 2)
        {
            // On Attack, Heavy Slash Single/Dual...
            isAttacking = true;
            Debug.Log("HeavyDamage");
        }
    }

    public void HandleJumpandGravity()
    {
        if (!characterController.isGrounded)
        {
            characterController.Move(new Vector3(0,gravityScale*Time.deltaTime,0));
        }
    }
    private void HandleFocusedMovement(Vector3 rawDirection)
    {
        transform.LookAt(focusedTransform);
        
        if (rawDirection.magnitude >= 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                float x = rawDirection.x;
                float z = rawDirection.z;
                playerAnimator.SetFloat("VelocityX", x, movementDamp, Time.deltaTime);
                playerAnimator.SetFloat("VelocityZ", z, movementDamp, Time.deltaTime);
            }
            else
            {
                float x = rawDirection.x * 0.5f;
                float z = rawDirection.z * 0.5f;
                playerAnimator.SetFloat("VelocityX", x, movementDamp, Time.deltaTime);
                playerAnimator.SetFloat("VelocityZ", z, movementDamp, Time.deltaTime);
            }
        }
        else
        {
            playerAnimator.SetFloat("VelocityX", 0f, movementDamp, Time.deltaTime);
            playerAnimator.SetFloat("VelocityZ", 0f, movementDamp, Time.deltaTime);
        }
        isCrouched = false;
    }
    private void HandleMovement(Vector3 rawDirection)
    {
        if (isCrouched)
        {
            playerAnimator.SetFloat("isCrouched", 1, movementDamp, Time.deltaTime);
        }
        else
        {
            playerAnimator.SetFloat("isCrouched", 0, movementDamp, Time.deltaTime);
        }
        rawDirection = rawDirection.normalized;
        smoothDirection = Vector3.SmoothDamp(smoothDirection, rawDirection, ref directionVelocity, directionSmoothTime);
        Vector3 direction = smoothDirection;
        if (direction.magnitude >= 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                float ts = direction.magnitude;
                playerAnimator.SetFloat("Velocity", ts, movementDamp, Time.deltaTime);
            }
            else
            {
                float ts = direction.magnitude * 0.5f;
                playerAnimator.SetFloat("Velocity", ts, movementDamp, Time.deltaTime);
            }
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothedAngleVelocity, smoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, smoothTime);
            
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            playerAnimator.SetFloat("Velocity", 0f, movementDamp, Time.deltaTime);
            currentSpeed = Mathf.SmoothDamp(currentSpeed, 0f, ref speedVelocity, smoothTime);
        }
    }
    
    public bool getIsFocused()
    {
        return isFocused;
    }
}

