using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private GameObject interactionText;
    private CharacterController characterController;
    private PlayerController playerController;
    private bool playerInRange = false;
    private bool isInteracting = false;
    private Animator animator;
    public bool isNPC = false;

    private void Start()
    {
        interactionCanvas.enabled = false; 
        characterController = player.GetComponent<CharacterController>(); 
        playerController = player.GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            interactionCanvas.enabled = true; 
            interactionText.SetActive(true);
            playerInRange = true;

            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            interactionCanvas.enabled = false;
            interactionText.SetActive(false);
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (isInteracting)
            {
                EndInteraction();
            }
            else
            {
                StartInteraction();
            }
        }
    }

    private void StartInteraction()
    {

        interactionText.SetActive(false);
        playerController.enabled = false;
        isInteracting = true;
        interactionPanel.SetActive(true);
        if (isNPC == true)
        {
            animator.SetBool("talking", true);
        }
    }

    private void EndInteraction()
    {

        interactionText.SetActive(true);
        playerController.enabled = true;
        isInteracting = false;
        interactionPanel.SetActive(false);
        animator.SetBool("talking", false);
    }
}
