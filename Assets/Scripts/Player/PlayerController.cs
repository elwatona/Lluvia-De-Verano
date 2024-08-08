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

    private CharacterController controller;
    private Animator animator;
    [SerializeField] private Vector3 playerVelocity;
    private bool _isGroundedPlayer;

    [SerializeField] private float rotationSpeed = 4f;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float sprintSpeed = 4.0f;
    [SerializeField] private float crouchSpeed = 1.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;
    private float currentSpeed;
    [SerializeField] private bool isCrouching = false;


    private void OnEnable()
    {
        movementControl.action.Enable();
        crouchControl.action.Enable();
        sprintControl.action.Enable();
    }

    private void OnDisable()
    {
        movementControl.action.Disable();
        crouchControl.action.Disable();
        sprintControl.action.Disable();
    }
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // Obtenemos el Animator del GameObject
    }
    private void Start()
    {
        currentSpeed = playerSpeed;
    }

    void Update()
    {
        _isGroundedPlayer = controller.isGrounded;
        if (_isGroundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 movement = movementControl.action.ReadValue<Vector2>();
        Vector3 move = Vector3.zero;

        switch (CameraControl.CurrentType)
        {
            case CameraControl.CameraType.Isometric:
                move = new Vector3(movement.x, 0, movement.y);
                move = CameraControl.CameraTransform.forward * move.z + CameraControl.CameraTransform.right * move.x;
                move.y = 0f;

                // Rotaci�n en vista isom�trica
                if (movement != Vector2.zero)
                {
                    float targetAngle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg + CameraControl.CameraTransform.eulerAngles.y;
                    Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
                }
                break;

            case CameraControl.CameraType.TopDown:
                move = new Vector3(movement.x, 0, movement.y);
                move.y = 0f;

                // Rotaci�n en vista top-down
                if (movement != Vector2.zero)
                {
                    float targetAngle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
                }
                break;

            case CameraControl.CameraType.Side2D:
                move = new Vector3(movement.x, 0, 0);

                // Rotaci�n en vista 2D (solo eje Y)
                if (movement.x != 0)
                {
                    float targetAngle = movement.x > 0 ? 90f : -90f; // Rotar a la derecha o izquierda
                    Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
                }
                break;
        }

        // Sprint
        bool isRunning = sprintControl.action.IsPressed() && !isCrouching;
        if (isRunning)
        {
            currentSpeed = sprintSpeed;
        }
        else if (!isCrouching)
        {
            currentSpeed = playerSpeed;
        }

        // Crouch
        if (crouchControl.action.WasPressedThisFrame() && movement == Vector2.zero)
        {
            isCrouching = !isCrouching;
            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
                controller.height = 2f;
                controller.center = new Vector3(0, 1, 0);
            }
            else
            {
                currentSpeed = playerSpeed;
                controller.height = 4;
                controller.center = new Vector3(0, 2, 0);
            }
        }

        controller.Move(move * Time.deltaTime * currentSpeed);

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // Actualizar los bools en el Animator
        animator.SetBool("walking", IsMoving() && !isRunning);
        animator.SetBool("running", IsMoving() && isRunning);
        animator.SetBool("crouching", isCrouching);
    }
    private bool IsMoving()
    {
        Vector2 movement = movementControl.action.ReadValue<Vector2>();
        switch(CameraControl.CurrentType)
        {
            case CameraControl.CameraType.Side2D:
            return movement.x != 0;

            default:
            return movement != Vector2.zero;
        }
    }
}
