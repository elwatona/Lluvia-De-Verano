using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CameraControl : MonoBehaviour
{
    public static event Action OnCameraBlendStarted;
    public static event Action OnCameraBlendStopped;
    private bool _wasBlendingLastFrame;
    public enum CameraType
    {
        Side2D,
        TopDown,
        Isometric,
        Dialogue
    }
    [SerializeField] private CinemachineVirtualCamera[] _cameras;
    [SerializeField] private CinemachineVirtualCamera[] _interact;
    private CinemachineVirtualCamera _currentCamera => _cameras[_cameraIndex];
    private CinemachineBrain _brain;
    static public CameraType CurrentType {get; private set;}
    static public Transform CameraTransform {get; private set;}
    private int _cameraIndex;
    [SerializeField] private bool _canSwitch = true;
    private void Awake()
    {
        _brain = GetComponentInChildren<CinemachineBrain>();
    }
    private void OnEnable()
    {
        InteractableObject.OnInteract += InteractCamera;
        CameraSwaper.OnSwapCamera += ChangeCamera;
    }
    private void OnDisable()
    {
        InteractableObject.OnInteract -= InteractCamera;
        CameraSwaper.OnSwapCamera -= ChangeCamera;
    }
    private void Start()
    {
        DeactivateCameras(_cameras);
        DeactivateCameras(_interact);
        ActivateCamera();
    }
    private void Update()
    {
        if (_canSwitch && Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchCurrentCamera();
        }
        OnBlending();
    }
    public void CameraActivated(ICinemachineCamera from, ICinemachineCamera to)
    {
        Debug.Log(from.Name + to.Name);
    }
    private void SwitchCurrentCamera()
    {
        _cameraIndex ++;
        _cameraIndex = _cameraIndex >= _cameras.Length ? 0 : _cameraIndex;
        DeactivateCameras(_cameras);
        DeactivateCameras(_interact);
        ActivateCamera();
    }
    private void DeactivateCameras(CinemachineVirtualCamera[] cameras)
    {
        foreach(var camera in cameras)
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
    private void InteractCamera(Transform interactuable, bool active, bool npc)
    {
        _canSwitch = !active;
        int camera = npc ? 0 : 1;
        if(!active)
        {
            _interact[camera].LookAt = null;
            _interact[camera].gameObject.SetActive(false);
            ActivateCamera();
            return;
        }
        
        DeactivateCameras(_cameras);
        DeactivateCameras(_interact);
        CurrentType = CameraType.Dialogue;
        _interact[camera].gameObject.SetActive(true);
        _interact[camera].LookAt = interactuable;
    }
    private void ChangeCamera(CameraType desiredCamera)
    {
        _cameraIndex = (int)desiredCamera;
        DeactivateCameras(_cameras);
        DeactivateCameras(_interact);
        ActivateCamera();
    }
    private void OnBlending()
    {
        if(_brain.IsBlending)
        {
            if(!_wasBlendingLastFrame)
            {
                OnCameraBlendStarted?.Invoke();
            }
            _wasBlendingLastFrame = true;
        }
        else
        {
            if(_wasBlendingLastFrame)
            {
                OnCameraBlendStopped?.Invoke();
            }
            _wasBlendingLastFrame = false;
        }
    }
}
