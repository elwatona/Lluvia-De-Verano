using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionReference movementControl;
    [SerializeField] private InputActionReference crouchControl;
    [SerializeField] private InputActionReference sprintControl;

    private CharacterController _characterController;
    private Animator _animator;
    [SerializeField] private Vector3 playerVelocity;
    private bool _isGroundedPlayer;

    [SerializeField] private float rotationSpeed = 4f;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float sprintSpeed = 4.0f;
    [SerializeField] private float crouchSpeed = 1.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private bool isCrouching = false;
    private float _currentSpeed;


    private void OnEnable()
    {
        movementControl.action.Enable();
        crouchControl.action.Enable();
        sprintControl.action.Enable();
        InteractableObject.OnInteract += OnInteract;
    }

    private void OnDisable()
    {
        movementControl.action.Disable();
        crouchControl.action.Disable();
        sprintControl.action.Disable();
        InteractableObject.OnInteract -= OnInteract;
    }
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        _currentSpeed = playerSpeed;
    }

    void Update()
    {
        _isGroundedPlayer = _characterController.isGrounded;
        if (_isGroundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        Vector3 move = Vector3.zero;

        switch (CameraControl.CurrentType)
        {
            case CameraControl.CameraType.Isometric:
                move = new Vector3(_movement.x, 0, _movement.y);
                move = CameraControl.CameraTransform.forward * move.z + CameraControl.CameraTransform.right * move.x;
                move.y = 0f;

                // Rotaci�n en vista isom�trica
                if (_movement != Vector2.zero)
                {
                    float targetAngle = Mathf.Atan2(_movement.x, _movement.y) * Mathf.Rad2Deg + CameraControl.CameraTransform.eulerAngles.y;
                    Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
                }
                break;

            case CameraControl.CameraType.TopDown:
                move = new Vector3(_movement.x, 0, _movement.y);
                move.y = 0f;

                // Rotaci�n en vista top-down
                if (_movement != Vector2.zero)
                {
                    float targetAngle = Mathf.Atan2(_movement.x, _movement.y) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
                }
                break;

            case CameraControl.CameraType.Side2D:
                move = new Vector3(_movement.x, 0, 0);

                // Rotaci�n en vista 2D (solo eje Y)
                if (_movement.x != 0)
                {
                    float targetAngle = _movement.x > 0 ? 90f : -90f; // Rotar a la derecha o izquierda
                    Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
                }
                break;
        }

        // Sprint
        if (IsRunning)
        {
            _currentSpeed = sprintSpeed;
        }
        else if (!isCrouching)
        {
            _currentSpeed = playerSpeed;
        }

        // Crouch
        if (IsCrouching)
        {
            isCrouching = !isCrouching;
            if (isCrouching)
            {
                _currentSpeed = crouchSpeed;
                _characterController.height = 2f;
                _characterController.center = new Vector3(0, 1, 0);
            }
            else
            {
                _currentSpeed = playerSpeed;
                _characterController.height = 4;
                _characterController.center = new Vector3(0, 2, 0);
            }
        }

        _characterController.Move(move * Time.deltaTime * _currentSpeed);

        playerVelocity.y += gravityValue * Time.deltaTime;
        _characterController.Move(playerVelocity * Time.deltaTime);

        // Actualizar los bools en el Animator
        _animator.SetBool("walking", IsMoving && !IsRunning);
        _animator.SetBool("running", IsMoving && IsRunning);
        _animator.SetBool("crouching", isCrouching);
    }
    private void OnInteract(Transform interactuable, bool isInteracting, bool isNPC)
    {
        if(isInteracting)
        {
            transform.LookAt(interactuable);
            _animator.SetBool("walking", false);
            _animator.SetBool("running", false);
            if(!isNPC) _animator.SetBool("crouching", true);
            return; 
        }

        //En caso de terminar la interaccion, corre lo de abajo
        if(!isNPC)
        {
            _animator.SetBool("crouching", false);
        }
    }
    private Vector2 _movement => movementControl.action.ReadValue<Vector2>();
    private bool IsMoving
    {
        get
        {
            switch(CameraControl.CurrentType)
            {
                case CameraControl.CameraType.Side2D:
                return _movement.x != 0;

                case CameraControl.CameraType.Dialogue:
                return false;

                default:
                return _movement != Vector2.zero;
            }
        }
    }
    private bool IsRunning => sprintControl.action.IsPressed() && !isCrouching;
    private bool IsCrouching => crouchControl.action.WasPressedThisFrame() && _movement == Vector2.zero;
}
