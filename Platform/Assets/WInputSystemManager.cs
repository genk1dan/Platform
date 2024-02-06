using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
public class WInputSystemManager : InputManager
{
    public PlatformInput InputActions;

    public MMInput.IMButton AttackButton { get; protected set; }


    protected bool _inputActionsEnabled = true;
    protected bool _initialized = false;

    protected override void Start()
    {
        if (!_initialized)
        {
            Initialization();
        }
    }
    protected override void InitializeButtons()
    {
        base.InitializeButtons();
        ButtonList.Add(AttackButton = new MMInput.IMButton(PlayerID, "Attack", AttackButtonDown, AttackButtonPressed, AttackButtonUp));
    }
    /// <summary>
    /// On init we register to all our actions
    /// </summary>
    protected override void Initialization()
    {
        base.Initialization();

        _inputActionsEnabled = true;

        InputActions = new PlatformInput();

        InputActions.PlayerControls.PrimaryMovement.performed += context => _primaryMovement = context.ReadValue<Vector2>();
        InputActions.PlayerControls.SecondaryMovement.performed += context => _secondaryMovement = context.ReadValue<Vector2>();

        InputActions.PlayerControls.Jump.performed += context => { BindButton(context, JumpButton); };
       
        InputActions.PlayerControls.Dash.performed += context => { BindButton(context, DashButton); };
        InputActions.PlayerControls.Attack.performed += context => { BindButton(context, AttackButton); };
        InputActions.PlayerControls.Throw.performed += context => { BindButton(context, ThrowButton); };
      
        _initialized = true;
    }
    public virtual void AttackButtonDown() { AttackButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
    public virtual void AttackButtonPressed() { AttackButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
    public virtual void AttackButtonUp() { AttackButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }
    /// <summary>
    /// Changes the state of our button based on the input value
    /// </summary>
    /// <param name="context"></param>
    /// <param name="imButton"></param>
    protected virtual void BindButton(InputAction.CallbackContext context, MMInput.IMButton imButton)
    {
        var control = context.control;

        if (control is ButtonControl button)
        {
            if (button.wasPressedThisFrame)
            {
                imButton.TriggerButtonDown();
            }
            if (button.wasReleasedThisFrame)
            {
                imButton.TriggerButtonUp();
            }
        }
    }

    protected override void Update()
    {
        if (IsMobile && _inputActionsEnabled)
        {
            _inputActionsEnabled = false;
            InputActions.Disable();
        }

        if (!IsMobile && (InputDetectionActive != _inputActionsEnabled))
        {
            if (InputDetectionActive)
            {
                _inputActionsEnabled = true;
                InputActions.Enable();
                ForceRefresh();
            }
            else
            {
                _inputActionsEnabled = false;
                InputActions.Disable();
            }
        }
    }

    protected virtual void ForceRefresh()
    {
        _primaryMovement = InputActions.PlayerControls.PrimaryMovement.ReadValue<Vector2>();
        _secondaryMovement = InputActions.PlayerControls.SecondaryMovement.ReadValue<Vector2>();
    }

    /// <summary>
    /// On enable we enable our input actions
    /// </summary>
    protected virtual void OnEnable()
    {
        if (!_initialized)
        {
            Initialization();
        }
        InputActions.Enable();
    }

    /// <summary>
    /// On disable we disable our input actions
    /// </summary>
    protected virtual void OnDisable()
    {
        InputActions.Disable();
    }
}

