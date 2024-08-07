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

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineVirtualCamera isometricCamera;
    [SerializeField] private CinemachineVirtualCamera topDownCamera;
    [SerializeField] private CinemachineVirtualCamera side2DCamera;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField] private CinemachineVirtualCamera activeCamera;
    private Transform cameraMainTransform;

    [SerializeField] private float rotationSpeed = 4f;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float sprintSpeed = 4.0f;
    [SerializeField] private float crouchSpeed = 1.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;
    private float currentSpeed;
    private bool isCrouching = false;

    private CameraType currentCameraType = CameraType.Isometric;

    private enum CameraType
    {
        Isometric,
        TopDown,
        Side2D
    }

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

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
        DetermineCameraType();
        currentSpeed = playerSpeed;
    }

    private void DetermineCameraType()
    {
        CinemachineVirtualCamera[] cameras = { isometricCamera, topDownCamera, side2DCamera };
        CinemachineVirtualCamera highestPriorityCamera = null;
        int highestPriority = int.MinValue;

        foreach (var camera in cameras)
        {
            if (camera.Priority > highestPriority)
            {
                highestPriority = camera.Priority;
                highestPriorityCamera = camera;
            }
        }

        if (highestPriorityCamera != null)
        {
            activeCamera = highestPriorityCamera;
            cameraMainTransform = activeCamera.transform;

            if (activeCamera == isometricCamera)
            {
                currentCameraType = CameraType.Isometric;
            }
            else if (activeCamera == topDownCamera)
            {
                currentCameraType = CameraType.TopDown;
            }
            else if (activeCamera == side2DCamera)
            {
                currentCameraType = CameraType.Side2D;
            }
        }
        else
        {
            Debug.LogWarning("No hay cámara activa.");
        }
    }

    void Update()
    {
        DetermineCameraType();

        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 movement = movementControl.action.ReadValue<Vector2>();
        Vector3 move = Vector3.zero;

        switch (currentCameraType)
        {
            case CameraType.Isometric:
                move = new Vector3(movement.x, 0, movement.y);
                move = cameraMainTransform.forward * move.z + cameraMainTransform.right * move.x;
                move.y = 0f;

                // Rotación en vista isométrica
                if (movement != Vector2.zero)
                {
                    float targetAngle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg + cameraMainTransform.eulerAngles.y;
                    Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
                }
                break;

            case CameraType.TopDown:
                move = new Vector3(movement.x, 0, movement.y);
                move.y = 0f;

                // Rotación en vista top-down
                if (movement != Vector2.zero)
                {
                    float targetAngle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
                }
                break;

            case CameraType.Side2D:
                move = new Vector3(movement.x, 0, 0);

                // Rotación en vista 2D (solo eje Y)
                if (movement.x != 0)
                {
                    float targetAngle = movement.x > 0 ? 90f : -90f; // Rotar a la derecha o izquierda
                    Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
                }
                break;
        }

        // Sprint
        if (sprintControl.action.IsPressed() && !isCrouching)
        {
            currentSpeed = sprintSpeed;
        }
        else if (!isCrouching)
        {
            currentSpeed = playerSpeed;
        }

        // Crouch
        if (crouchControl.action.WasPressedThisFrame())
        {
            isCrouching = !isCrouching;
            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
                controller.height = 1.0f;
            }
            else
            {
                currentSpeed = playerSpeed;
                controller.height = 2.0f;
            }
        }

        controller.Move(move * Time.deltaTime * currentSpeed);

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
