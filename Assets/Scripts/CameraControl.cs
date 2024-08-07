using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera topDownCamera;
    [SerializeField] private CinemachineVirtualCamera isometricCamera;
    [SerializeField] private CinemachineVirtualCamera twoDCamera;

    private bool isTopDownActive = false;
    private bool isIsometricActive = false;
    private bool twoDActive = true;

    private void Start()
    {
        // Activar inicialmente la cámara de 3ra persona y desactivar las otras.
        ActivateIsometricCamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchCamera();
        }
    }

    private void SwitchCamera()
    {
        if (twoDActive)
        {
            ActivateTopDownCamera();
        }
        else if (isTopDownActive)
        {
            ActivateIsometricCamera();
        }
        else if (isIsometricActive)
        {
            Activate2DCamera();
        }
    }

    private void ActivateTopDownCamera()
    {
        topDownCamera.Priority = 10;
        isometricCamera.Priority = 0;
        twoDCamera.Priority = 0;

        isTopDownActive = true;
        isIsometricActive = false;
        twoDActive = false;
    }

    private void ActivateIsometricCamera()
    {
        topDownCamera.Priority = 0;
        isometricCamera.Priority = 10;
        twoDCamera.Priority = 0;

        isTopDownActive = false;
        isIsometricActive = true;
        twoDActive = false;
    }

    private void Activate2DCamera()
    {
        topDownCamera.Priority = 0;
        isometricCamera.Priority = 0;
        twoDCamera.Priority = 10;

        isTopDownActive = false;
        isIsometricActive = false;
        twoDActive = true;
    }
}
