using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Action<Vector2> OnMoveInput;
    public Action<bool> OnSprintInput;
    public Action OnJumpInput;
    public Action OnClimbInput;
    public Action OnCancelClimb;
    public Action OnChangePOV;

    private void Update()
    {
        CheckJumpInput();
        CheckSprintInput();
        CheckCrouchInput();
        CheckChangePOVInput();
        CheckClimbInput();
        CheckGlideInput();
        CheckCancelInput();
        CheckPunchInput();
        CheckMainMenuInput();
        CheckMovementInput();
    }

    private void CheckMovementInput()
    {
        float verticalAxis = Input.GetAxis("Vertical");
        float horizontalAxis = Input.GetAxis("Horizontal");
        Vector2 inputAxis = new Vector2(horizontalAxis, verticalAxis);

        if (OnMoveInput != null)
        {
            OnMoveInput(inputAxis);
        }
    }

    private void CheckJumpInput()
    {
        bool isPressJumpInput = Input.GetKeyDown(KeyCode.Space);
        if (isPressJumpInput)
        {
            OnJumpInput();
        }
    }

    private void CheckSprintInput()
    {
        bool isHoldSprintInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (isHoldSprintInput)
        {
            OnSprintInput(true);
        }
        else
        {
            OnSprintInput(false);
        }
    }

    private void CheckCrouchInput()
    {
        bool isPressCrouchInput = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
        if (isPressCrouchInput)
        {

        }
    }

    private void CheckChangePOVInput()
    {
        bool isPressChangePOVInput = Input.GetKeyDown(KeyCode.Q);
        if (isPressChangePOVInput)
        {
            if (OnChangePOV != null)
            {
                OnChangePOV();
            }
        }
    }

    private void CheckClimbInput()
    {
        bool isPressClimbInput = Input.GetKeyDown(KeyCode.E);
        if (isPressClimbInput)
        {
            OnClimbInput();
        }
    }

    private void CheckGlideInput()
    {
        bool isPressGlideInput = Input.GetKeyDown(KeyCode.G);
        if (isPressGlideInput)
        {

        }
    }

    private void CheckCancelInput()
    {
        bool isPressCancelInput = Input.GetKeyDown(KeyCode.C);
        if (isPressCancelInput)
        {
            if (OnCancelClimb != null)
            {
                OnCancelClimb();            
            }
        }
    }

    private void CheckPunchInput()
    {
        bool isPressPunchInput = Input.GetKeyDown(KeyCode.Mouse0);
        if (isPressPunchInput)
        {

        }
    }
    
    private void CheckMainMenuInput()
    {
        bool isPressMainMenuInput = Input.GetKeyDown(KeyCode.Escape);
        if (isPressMainMenuInput)
        {
            
        }
    }
}
