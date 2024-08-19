using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
#region Components
    private CharacterController _characterController;
    private Animator _animator;
#endregion
#region Inspector Properties
    [SerializeField] private InputActionReference[] _actions;
    [SerializeField] private float _rotationSpeed = 4f, 
    _playerSpeed = 2.0f, 
    _sprintSpeed = 4.0f, 
    _crouchSpeed = 1.0f, 
    _jumpHeight = 1.0f, 
    _gravityValue = -9.81f;
    [SerializeField] private bool isCrouching = false;
#endregion
#region Private Properties
    private bool _isGroundedPlayer => _characterController.isGrounded;
    private float _currentSpeed;
    private bool IsMoving
    {
        get
        {
            switch(CameraControl.CurrentType)
            {
                case CameraControl.CameraType.Side2D:
                return _movementInput.x != 0;

                case CameraControl.CameraType.Dialogue:
                return false;

                default:
                return _movementInput != Vector2.zero;
            }
        }
    }
    private Vector2 _movementInput => _actions[0].action.ReadValue<Vector2>();
    private bool IsCrouching => _actions[1].action.WasPressedThisFrame() && _movementInput == Vector2.zero;
    private bool IsRunning => _actions[2].action.IsPressed() && !isCrouching;
    private Vector3 _desiredMovement 
    {
        get
        {
            Vector3 result;
            Vector3 forward;
            Vector3 right;
            switch(CameraControl.CurrentType)
            {
                case CameraControl.CameraType.Side2D:
                    result = CameraControl.CameraTransform.right * _movementInput.x;
                break;

                case CameraControl.CameraType.TopDown:
                    forward = CameraControl.CameraTransform.up * _movementInput.y;
                    right = CameraControl.CameraTransform.right * _movementInput.x;
                    result = forward + right; 
                break;

                case CameraControl.CameraType.Isometric:
                    forward = CameraControl.CameraTransform.forward * _movementInput.y;
                    right = CameraControl.CameraTransform.right * _movementInput.x;
                    result = forward + right;
                break;

                default:
                    result = Vector3.zero;
                    Debug.LogError("No hay movimiento deseado para esta camara");
                break;
            }
            result = result.normalized;
            result.y = _gravityValue;
            return result;
        }
    }
    private float _targetAngle
    {
        get
        {
            float result;
            switch(CameraControl.CurrentType)
            {
                case CameraControl.CameraType.Side2D:
                    result = _movementInput.x > 0 ? 180f : 0f; // Rotar a la derecha o izquierda
                break;

                case CameraControl.CameraType.TopDown:
                case CameraControl.CameraType.Isometric:
                    result = Mathf.Atan2(_movementInput.x, _movementInput.y) * Mathf.Rad2Deg + CameraControl.CameraTransform.eulerAngles.y;
                break;

                default:
                    Debug.LogError("No hay rotacion deseada para esta camara");
                    result = 0;
                break;
            }
            return result;
        }
    }
#endregion
    private void OnEnable()
    {
        EnableActions(true);
        InteractableObject.OnInteract += OnInteract;
    }
    private void OnDisable()
    {
        EnableActions(false);
        InteractableObject.OnInteract -= OnInteract;
    }
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        _currentSpeed = _playerSpeed;
    }
    void Update()
    {
        _currentSpeed = IsRunning ? _sprintSpeed : IsCrouching ? _crouchSpeed : _playerSpeed;
        OnCrouch();
        _characterController.Move(_desiredMovement * Time.deltaTime * _currentSpeed);
        Rotate();
        UpdateAnimator();
    }

    private void EnableActions(bool value)
    {
        foreach(var action in _actions)
        {
            if(value) action.action.Enable();
            else action.action.Disable();
        }
    }
    private void UpdateAnimator()
    {
        _animator.SetBool("walking", IsMoving && !IsRunning);
        _animator.SetBool("running", IsMoving && IsRunning);
        _animator.SetBool("crouching", isCrouching);
    }
    private void Rotate()
    {
        if(!IsMoving) return;
        Quaternion rotation = Quaternion.Euler(0f, _targetAngle, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * _rotationSpeed);
    }
    private void OnCrouch()
    {
        if (IsCrouching)
        {
            isCrouching = !isCrouching;
            _characterController.height = 2f;
            _characterController.center = new Vector3(0, 1, 0);
            return;
        }
        _characterController.height = 4;
        _characterController.center = new Vector3(0, 2, 0);
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
}
