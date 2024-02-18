using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterAttack : CharacterAbility
{
    protected bool _charHztlMvmtFlipInitialSetting;
    protected bool _charHztlMvmtFlipInitialSettingSet = false;

    protected IEnumerator _attackCoroutine;
    Rigidbody2D _rigidbody2;
    protected const string _attackAnimationParameterName = "Attack";
    protected int _attackAnimationParameter;
    protected override void Initialization()
    {
        base.Initialization();
        if (_characterHorizontalMovement != null)
        {
            _charHztlMvmtFlipInitialSetting = _characterHorizontalMovement.FlipCharacterToFaceDirection;
        }
        _rigidbody2 = GetComponent<Rigidbody2D>();
    }
    protected override void HandleInput()
    {
        if (_inputManager.AttackButton.State.CurrentState == MMInput.ButtonStates.ButtonDown&& _movement.CurrentState != CharacterStates.MovementStates.Dashing
            && _movement.CurrentState != CharacterStates.MovementStates.DashAttacking)
        {
            Attack();
        }
    }
   
    public void Attack()
    {
        _movement.ChangeState(CharacterStates.MovementStates.Attacking);
      
        // we trigger a character event
        MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.HandleWeapon);
        if (_controller.State.IsGrounded)
        {
           _controller.SetHorizontalForce(0);
        }
       
        _controller.DefaultParameters.Gravity = -20;
        StartCoroutine(AttackCor());

    }
    IEnumerator AttackCor()
    {
        // if the character is not in a position where it can move freely, we do nothing.
        if (!AbilityAuthorized
             || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
        {
            yield break;
        }
       // _characterHorizontalMovement.ReadInput = false;
       // _characterHorizontalMovement.MovementForbidden = true;

        if (_animator != null)
        {
            MMAnimatorExtensions.SetAnimatorTrigger(_animator, _attackAnimationParameter,
                _character._animatorParameters, _character.PerformAnimatorSanityChecks);
        }

        float rollStartedAt = Time.time;

        // we keep rolling until we've reached our target distance or until we get interrupted
        while ((Time.time - rollStartedAt < 0.4f)
                && !_controller.State.TouchingLevelBounds
                && _movement.CurrentState == CharacterStates.MovementStates.Attacking)
        {
          
            yield return null;
        }
        StopAttack();
    }
    public virtual void StopAttack()
    {
        // _controller.GravityActive(true);
        _controller.DefaultParameters.Gravity = -30;
        //_characterHorizontalMovement.ReadInput = true;
        //_characterHorizontalMovement.MovementForbidden = false;
        // we play our exit sound
        StopStartFeedbacks();
        PlayAbilityStopFeedbacks();
        MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.HandleWeapon, MMCharacterEvent.Moments.End);

        // once the boost is complete, if we were rolling, we make it stop and start the roll cooldown
        if (_movement.CurrentState == CharacterStates.MovementStates.Attacking)
        {
            if (_controller.State.IsGrounded)
            {
                _movement.ChangeState(CharacterStates.MovementStates.Idle);
            }
            else
            {
                _movement.RestorePreviousState();
            }
        }
    }
    protected override void InitializeAnimatorParameters()
    {
        RegisterAnimatorParameter(_attackAnimationParameterName, AnimatorControllerParameterType.Bool, out _attackAnimationParameter);
    }
    public override void UpdateAnimator()
    {
        MMAnimatorExtensions.UpdateAnimatorBool
           (_animator, _attackAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Attacking), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
    }
   

    public override void ProcessAbility()
    {
        base.ProcessAbility();
    }
}
