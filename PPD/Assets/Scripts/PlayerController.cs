using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 720f;
    
    private CharacterController _characterController;
    private Camera _mainCamera;
    private Vector2 _moveInput;
    private Vector3 _cameraOffset;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            _cameraOffset = _mainCamera.transform.position - transform.position;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        Move();
        if (_mainCamera != null)
        {
            _mainCamera.transform.position = transform.position + _cameraOffset;
        }
    }

    private void Move()
    {
        // Simple WASD logic for demonstration if PlayerInput component is not used
        // But better to use the values from InputSystem
        Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
        
        if (move.magnitude > 0.1f)
        {
            _characterController.Move(move * moveSpeed * Time.deltaTime);
            
            // Rotation
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        
        // Apply gravity
        if (!_characterController.isGrounded)
        {
            _characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }
    
    // Fallback for easy testing without PlayerInput component
    void OnEnable()
    {
        var controls = new InputAction("Move", binding: "<Gamepad>/leftStick");
        controls.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
            
        controls.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        controls.canceled += ctx => _moveInput = Vector2.zero;
        controls.Enable();
    }
}
