using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public InputActionReference rightClick;
    public InputActionReference move;
    public InputActionReference mouseDelta;
    public InputActionReference mouseScroll;

    public static CameraController Instance { get; private set; }

    private float rotateSpeed = 0.5f;
    private float moveSpeed = 3f;
    public float zoomSpeed = 10f;

    public float Xmax;
    public float Zmax;

    public float ZoomMax;
    public float ZoomMin;

    private Vector2 moveInput = Vector2.zero;
    private Vector2 lookInput;
    private bool isRotating = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (move != null)
        {
            move.action.started += OnMove;
            move.action.performed += OnMove;
            move.action.canceled += OnMove;
        }

        if (rightClick != null)
        {
            rightClick.action.performed += OnRotateButton;
            rightClick.action.canceled += OnRotateButton;
        }
    }

    private void OnEnable()
    {
        if (mouseDelta != null) mouseDelta.action.Enable();
        if (move != null) move.action.Enable();
        if (mouseScroll != null) mouseScroll.action.Enable();
        if (rightClick != null) rightClick.action.Enable();
    }

    private void OnDisable()
    {
        if (mouseDelta != null) mouseDelta.action.Disable();
        if (move != null) move.action.Disable();
        if (mouseScroll != null) mouseScroll.action.Disable();
        if (rightClick != null) rightClick.action.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        if (context.canceled)
        {
            moveInput = Vector2.zero;
        }
    }

    public void OnRotateButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRotating = true;
        }
        else if (context.canceled)
        {
            isRotating = false;
        }
    }

    void Update()
    {
        if (isRotating)
        {
            lookInput = mouseDelta.action.ReadValue<Vector2>();

            if (lookInput != Vector2.zero)
            {
                float horizontalRotation = lookInput.x * rotateSpeed;
                transform.Rotate(Vector3.up, horizontalRotation, Space.World);
            }
        }

        if (move != null && move.action.enabled)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            float horizontalMove = moveInput.x * moveSpeed * Time.unscaledDeltaTime;
            float verticalMove = moveInput.y * moveSpeed * Time.unscaledDeltaTime;

            Vector3 moveVector = (right * horizontalMove) + (forward * verticalMove);
            Vector3 targetPosition = transform.position + moveVector;

            float clampedX = Mathf.Clamp(targetPosition.x, -Xmax, Xmax);

            float clampedZ = Mathf.Clamp(targetPosition.z, -Zmax, Zmax);

            transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
        }

        if (mouseScroll != null && mouseScroll.action.enabled)
        {
            Vector2 scrollValue = mouseScroll.action.ReadValue<Vector2>();

            float zoomAmount = scrollValue.y * zoomSpeed * Time.unscaledDeltaTime;

            Vector3 targetPosition = transform.position + (transform.forward * zoomAmount);

            if (targetPosition.y < ZoomMin || targetPosition.y > ZoomMax)
            {
                float y = Mathf.Clamp(targetPosition.y, ZoomMin, ZoomMax);
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
            }

            else
            {
                transform.position = targetPosition;
            }
        }
    }
}
