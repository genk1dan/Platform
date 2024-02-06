using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPickUp : CharacterAbility
{
    public GameObject CurrentGoOnHand;
    [SerializeField] private Collider2D target;
    protected bool _charHztlMvmtFlipInitialSetting;
    protected bool _charHztlMvmtFlipInitialSettingSet = false;
    protected const string _pickUpAnimationParameterName = "PickUp";
    protected int _pickUpAnimationParameter;

    [SerializeField] Transform hand;

    protected override void InitializeAnimatorParameters()
    {
        RegisterAnimatorParameter(_pickUpAnimationParameterName, AnimatorControllerParameterType.Bool, out _pickUpAnimationParameter);
    }
    public override void UpdateAnimator()
    {
        MMAnimatorExtensions.UpdateAnimatorBool
           (_animator, _pickUpAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Attacking), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
    }


    protected override void Initialization()
    {
        base.Initialization();
        if (_characterHorizontalMovement != null)
        {
            _charHztlMvmtFlipInitialSetting = _characterHorizontalMovement.FlipCharacterToFaceDirection;
        }
    }
    protected override void HandleInput()
    {
        if (_inputManager.ThrowButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
        {
            TryThrow();
        }
    }
    public void TryThrow()
    {
        if(CurrentGoOnHand != null)
        {
            TryThrowOut();
        }
        else
        {
            TryPickUp();
        }
    }

    public void TryThrowOut()
    {
        //float dir = 1;

        //if (_character.IsFacingRight == false)
        //{
        //    dir = -1;
        //}
        CurrentGoOnHand.transform.SetParent(null);

        CurrentGoOnHand.GetComponent<AIWalk>().enabled = false;

        var temp = CurrentGoOnHand.GetComponent<CharacterHorizontalMovement>();
        temp.MovementForbidden = false;
        CurrentGoOnHand.GetComponent<CharacterOnHand>().StartDash(_character.IsFacingRight);
        CurrentGoOnHand = null;
        target = null;



    }

    public void TryPickUp()
    {
        Vector2 dir = Vector2.right;
        if (_character.IsFacingRight == false)
        {
            dir = Vector2.left;
        }
        target = Physics2D.CircleCast(transform.position, 0.8f, dir, 1f, LayerMask.GetMask("Enemies")).collider;
        if(target != null)
        {
            CurrentGoOnHand = target.gameObject;
            CurrentGoOnHand.GetComponent<AIWalk>().enabled = false;
            CurrentGoOnHand.GetComponent<CharacterHorizontalMovement>().MovementForbidden = true;
            CurrentGoOnHand.transform.SetParent(hand, true);
            CurrentGoOnHand.transform.localPosition = Vector2.zero;
        }
        else
        {
            CurrentGoOnHand = null;
        }
    }

   
    public override void ProcessAbility()
    {
        base.ProcessAbility();
    }
}
