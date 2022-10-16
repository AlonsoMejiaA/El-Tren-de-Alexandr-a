using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{

    ThirdPersonActionsAssets TPInputActions;
    CharacterController characterController;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    bool movementPressed;
    [SerializeField] float speed;
    [SerializeField] float rotationPerFrame;

    //Events to trigger the minimap tween

    public delegate void OnNotmoving(bool state);
    public event OnNotmoving onNotmoving;

    //variable to count the time when the player is not moving

    float tweenTimer = 0;
    [SerializeField] public float tweenLimit;
    bool isFadded;

    void Awake()
    {
        TPInputActions = new ThirdPersonActionsAssets();
        characterController = GetComponent<CharacterController>();

        TPInputActions.Player.Move.started += OnMovementInput;
        TPInputActions.Player.Move.canceled += OnMovementInput;
        TPInputActions.Player.Move.performed += OnMovementInput;

    }

    public void OnEnable()
    {
        TPInputActions.Player.Enable();
    }

    public void OnDisable()
    {
        TPInputActions.Player.Disable();
    }

    void Update()
    {
        HandleRotation();
        HandleGravity();
        characterController.Move(currentMovement * Time.deltaTime *speed);

        if (!movementPressed)
        {
            tweenTimer += Time.deltaTime;

            if (tweenTimer >= tweenLimit)
            {
                if (onNotmoving != null)
                {
                    onNotmoving(true);
                    tweenTimer = 0;
                    isFadded = true;
                }
            }

        }
        else if (movementPressed)
        {

            if (isFadded)
            {
                if (onNotmoving != null)
                {
                    onNotmoving(false);
                    isFadded = false;
                    tweenTimer = 0;
                }
            }
        }

    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        movementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 90f;
        positionToLookAt.z = currentMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (movementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationPerFrame);
        }
    }

    void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            float groundedGravity = -0.5f;
            currentMovement.y = groundedGravity;
        }
        else
        {
            float gravity = -9.8f;
            currentMovement.y = gravity;
        }
    }
}
