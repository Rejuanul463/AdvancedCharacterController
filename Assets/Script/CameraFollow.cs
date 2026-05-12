using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    private CharacterMovement characterMovement;

    [Header("Distance & Height")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 1.5f;

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 50f;
    [SerializeField] private float minY = -30f;
    [SerializeField] private float maxY = 60f;

    [Header("Smoothness")]
    [SerializeField] private float positionSmoothTime = 0.05f;
    [SerializeField] private float rotationSmoothTime = 0.05f;

    [Header("Focus Settings")]
    [SerializeField] private float focusPitchOffset = 30f;

    private float yaw;
    private float pitch;

    private float currentYaw;
    private float currentPitch;

    private float yawVelocity;
    private float pitchVelocity;

    private Vector3 positionVelocity;

    private bool wasFocused;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;
        pitch = angles.x;

        currentYaw = yaw;
        currentPitch = pitch;

        characterMovement = target.GetComponent<CharacterMovement>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        bool isFocused = characterMovement.getIsFocused();

        HandleFocusState(isFocused);
        HandleRotation(isFocused);
        HandlePosition();

        wasFocused = isFocused;
    }

    private void HandleFocusState(bool isFocused)
    {
        // Sync rotation when exiting focus (prevents snap)
        if (wasFocused && !isFocused)
        {
            Vector3 angles = transform.eulerAngles;

            yaw = angles.y;
            pitch = angles.x;

            currentYaw = yaw;
            currentPitch = pitch;
        }
    }

    private void HandleRotation(bool isFocused)
    {
        float targetYaw;
        float targetPitch;

        if (isFocused)
        {
            Vector3 targetEuler = target.eulerAngles;

            targetYaw = targetEuler.y;
            targetPitch = targetEuler.x + focusPitchOffset;
        }
        else
        {
            // Mouse only works when NOT focused
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 10f * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 10f * Time.deltaTime;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minY, maxY);

            targetYaw = yaw;
            targetPitch = pitch;
        }

        // Proper angle smoothing (fixes 360 spin issue)
        currentYaw = Mathf.SmoothDampAngle(
            currentYaw,
            targetYaw,
            ref yawVelocity,
            rotationSmoothTime
        );

        currentPitch = Mathf.SmoothDampAngle(
            currentPitch,
            targetPitch,
            ref pitchVelocity,
            rotationSmoothTime
        );

        transform.eulerAngles = new Vector3(currentPitch, currentYaw, 0f);
    }

    private void HandlePosition()
    {
        Vector3 targetPosition =
            target.position +
            Vector3.up * height -
            transform.forward * distance;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );
    }
}