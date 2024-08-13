using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class InteractableObject : MonoBehaviour
{
    public static event Action<Transform, bool, bool> OnInteract;
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private GameObject interactionText;
    [SerializeField] private bool _isNPC = false;
    private PlayerController _playerController;
    private Transform _playerTransform;
    private bool _isPlayerInRange = false;
    private bool _isInteracting = false;
    private Animator _animator;
    private Quaternion _originalRotation;

    private void Start()
    {
        _originalRotation = transform.rotation;
        interactionCanvas.enabled = false;
        if(_isNPC) _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out _playerController)) 
        {
            if(!_playerTransform) _playerTransform = other.transform;
            interactionCanvas.enabled = true; 
            interactionText.SetActive(true);
            _isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionCanvas.enabled = false;
            interactionText.SetActive(false);
            _isPlayerInRange = false;
        }
    }

    private void Update()
    {
        if (_isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (_isInteracting)
            {
                OnInteract?.Invoke(transform, false, _isNPC);
                EndInteraction();
            }
            else
            {
                OnInteract?.Invoke(transform, true, _isNPC);
                StartInteraction();
            }
        }
    }

    private void StartInteraction()
    {
        if (_isNPC)
        {
            transform.LookAt(_playerTransform);
            _animator.SetBool("talking", true);
        }
        interactionText.SetActive(false);
        _playerController.enabled = false;
        _isInteracting = true;
        interactionPanel.SetActive(true);
    }

    private void EndInteraction()
    {
        if (_isNPC) 
        {
            transform.rotation = _originalRotation;
            _animator.SetBool("talking", false);
        }
        interactionText.SetActive(true);
        _playerController.enabled = true;
        _isInteracting = false;
        interactionPanel.SetActive(false);
    }
}
