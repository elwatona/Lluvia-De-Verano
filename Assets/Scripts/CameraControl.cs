using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{

    public enum CameraType
    {
        Isometric,
        TopDown,
        Side2D
    }
    [SerializeField] private CinemachineVirtualCamera[] _cameras;
    private CinemachineVirtualCamera _currentCamera => _cameras[_cameraIndex];
    private CinemachineBrain _brain;
    static public CameraType CurrentType {get; private set;}
    static public Transform CameraTransform {get; private set;}
    private int _cameraIndex;
    private void Awake()
    {
        _brain = GetComponentInChildren<CinemachineBrain>();
    }
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
        CurrentType = (CameraType)_cameraIndex;
        CameraTransform = _currentCamera.transform;
        _currentCamera.Priority = 10;
        _currentCamera.gameObject.SetActive(true);
        ChangeWorldUp();
    }
    private void ChangeWorldUp()
    {
        Transform worldUp = null;
        if (CurrentType == CameraType.TopDown)
        {
            worldUp = transform.Find("WorldUp");
        }
        _brain.m_WorldUpOverride = worldUp;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchCurrentCamera();
        }
    }
}
