using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera[] _cameras;
    private CinemachineVirtualCamera _currentCamera => _cameras[_cameraIndex];
    private int _cameraIndex;

    private void Start()
    {
        DeactivateCameras();
        ActivateCamera();
    }
    private void SwitchCurrentCamera()
    {
        _cameraIndex ++;
        _cameraIndex = _cameraIndex >= _cameras.Length ? 0 : _cameraIndex;
        DeactivateCameras();
        ActivateCamera();
    }
    private void DeactivateCameras()
    {
        foreach(var camera in _cameras)
        {
            camera.Priority = 0;
            camera.gameObject.SetActive(false);
        }
    }
    private void ActivateCamera()
    {
        _currentCamera.Priority = 10;
        _currentCamera.gameObject.SetActive(true);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchCurrentCamera();
        }
    }
}
