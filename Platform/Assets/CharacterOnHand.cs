using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterOnHand : CharacterAbility
{
  
    [Tooltip("the distance this dash should cover")]
    public float DashDistance = 3f;
    /// the force of the dash
    [Tooltip("the force of the dash")]
    public float DashForce = 40f;
    /// if this is true, forces will be reset on dash exit (killing inertia)
    [Tooltip("if this is true, forces will be reset on dash exit (killing inertia)")]
    public bool ResetForcesOnExit = false;
    /// if this is true, position will be forced on exit to match an exact distance
    [Tooltip("if this is true, position will be forced on exit to match an exact distance")]
    public bool ForceExactDistance = false;
    /// if this is true, the character's controller will detach from moving platforms when dash starts
    [Tooltip("if this is true, the character's controller will detach from moving platforms when dash starts")]
    public bool DetachFromMovingPlatformsOnDash = false;

    [Header("Direction")]

    
    [Tooltip("the direction threshold over which to compare direction when authorizing the dash. You'll likely want to keep it fairly close to zero")]
    public float DashDirectionMinThreshold = 0.1f;

    public enum SuccessiveDashResetMethods { Grounded, Time }

    
    /// the method used to reset the number of dashes left, only if dashes are not infinite
    [Tooltip("the method used to reset the number of dashes left, only if dashes are not infinite")]
    [MMCondition("LimitedDashes", true)]
    public SuccessiveDashResetMethods SuccessiveDashResetMethod = SuccessiveDashResetMethods.Grounded;
    /// when in time reset mode, the duration, in seconds, after which the amount of dashes left gets reset, only if dashes are not infinite
    [Tooltip("when in time reset mode, the duration, in seconds, after which the amount of dashes left gets reset, only if dashes are not infinite")]
    [MMEnumCondition("SuccessiveDashResetMethod", (int)SuccessiveDashResetMethods.Time)]
    public float SuccessiveDashResetDuration = 2f;

    

    protected float _cooldownTimeStamp = 0;
    protected float _startTime;
    protected Vector2 _initialPosition;
    protected Vector2 _dashDirection;
    protected float _distanceTraveled = 0f;
    protected bool _shouldKeepDashing = true;
    protected float _slopeAngleSave = 0f;
    protected bool _dashEndedNaturally = true;
    protected IEnumerator _dashCoroutine;
    protected CharacterDive _characterDive;
    protected float _lastDashAt = 0f;
    protected float _averageDistancePerFrame;
    protected int _startFrame;
    protected Bounds _bounds;

    // animation parameters
   // protected const string _dashingAnimationParameterName = "Dashing";
   // protected int _dashingAnimationParameter;

    /// <summary>
    /// Initializes our aim instance
    /// </summary>
    protected override void Initialization()
    {
        base.Initialization();
      
        _characterDive = _character?.FindAbility<CharacterDive>();
       
    }


    /// <summary>
    /// At the start of each cycle, we check if we're pressing the dash button. If we
    /// </summary>
  

 

    /// <summary>
    /// Causes the character to dash or dive (depending on the vertical movement at the start of the dash)
    /// </summary>
    public virtual void StartDash(bool facingRight)
    {
        _dashDirection = facingRight ? Vector2.right : Vector2.left;
        InitiateDash();
    }

   

   
    /// <summary>
    /// initializes all parameters prior to a dash and triggers the pre dash feedbacks
    /// </summary>
    public virtual void InitiateDash()
    {
        if (DetachFromMovingPlatformsOnDash)
        {
            _controller.DetachFromMovingPlatform();
        }

        // we set its dashing state to true
        _movement.ChangeState(CharacterStates.MovementStates.StrikeToFly);

        // we start our sounds
        PlayAbilityStartFeedbacks();
        //MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Dash, MMCharacterEvent.Moments.Start);

        // we initialize our various counters and checks
        _startTime = Time.time;
        _startFrame = Time.frameCount;
        _dashEndedNaturally = false;
        _initialPosition = _characterTransform.position;
        _distanceTraveled = 0;
        _shouldKeepDashing = true;
      
        _lastDashAt = Time.time;
     

        // we prevent our character from going through slopes
        _slopeAngleSave = _controller.Parameters.MaximumSlopeAngle;
        _controller.Parameters.MaximumSlopeAngle = 0;
        _controller.SlowFall(0f);

      
     
        // we launch the boost corountine with the right parameters
        _dashCoroutine = Dash();
        StartCoroutine(_dashCoroutine);
    }

    /// <summary>
    /// Computes the dash direction based on the selected options
    /// </summary>
  

    /// <summary>
    /// Prevents the character from dashing into the ground when already grounded and if AutoCorrectTrajectory is checked
    /// </summary>
   

    /// <summary>
    /// Checks whether or not a character flip is required, and flips the character if needed
    /// </summary>
   

    /// <summary>
    /// Coroutine used to move the player in a direction over time
    /// </summary>
    protected virtual IEnumerator Dash()
    {
        // if the character is not in a position where it can move freely, we do nothing.
        if (!AbilityAuthorized
             || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
        {
            yield break;
        }

        // we keep dashing until we've reached our target distance or until we get interrupted
        while (_distanceTraveled < DashDistance
               && _shouldKeepDashing
               && TestForLevelBounds()
               && TestForExactDistance()
               && _movement.CurrentState == CharacterStates.MovementStates.StrikeToFly)
        {
            _distanceTraveled = Vector3.Distance(_initialPosition, _characterTransform.position);

            // if we collide with something on our left or right (wall, slope), we stop dashing, otherwise we apply horizontal force
            if ((_controller.State.IsCollidingLeft && _dashDirection.x < -DashDirectionMinThreshold)
                 || (_controller.State.IsCollidingRight && _dashDirection.x > DashDirectionMinThreshold)
                 || (_controller.State.IsCollidingAbove && _dashDirection.y > DashDirectionMinThreshold)
                 || (_controller.State.IsCollidingBelow && _dashDirection.y < -DashDirectionMinThreshold))
            {
                _shouldKeepDashing = false;
                _controller.SetForce(Vector2.zero);
            }
            else
            {
                _controller.GravityActive(false);
                _controller.SetForce(_dashDirection * DashForce);
            }
            yield return null;
        }

        StopDash();
    }

    /// <summary>
    /// If the character is hitting level bounds, we check if they're "in front" of us or "behind" us, and whether it should prevent the dash or not
    /// </summary>
    /// <returns></returns>
    protected virtual bool TestForLevelBounds()
    {
        if (!_controller.State.TouchingLevelBounds)
        {
            return true;
        }
        else
        {
            _bounds = LevelManager.Instance.LevelBounds;
            return (_character.IsFacingRight) ? (_character.transform.position.x < _bounds.center.x) : (_character.transform.position.x > _bounds.center.x);
        }
    }

    /// <summary>
    /// Checks (if needed) if we've exceeded our distance, and positions the character at the exact final position
    /// </summary>
    /// <returns></returns>
    protected virtual bool TestForExactDistance()
    {
        if (!ForceExactDistance)
        {
            return true;
        }

        int framesSinceStart = Time.frameCount - _startFrame;
        _averageDistancePerFrame = _distanceTraveled / framesSinceStart;

        if (DashDistance - _distanceTraveled < _averageDistancePerFrame)
        {
            _characterTransform.position = _initialPosition + (_dashDirection * DashDistance);
            return false;
        }


        return true;
    }

    /// <summary>
    /// Stops the dash coroutine and resets all necessary parts of the character
    /// </summary>
    public virtual void StopDash()
    {
        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);
        }

        // once our dash is complete, we reset our various states
        _controller.DefaultParameters.MaximumSlopeAngle = _slopeAngleSave;
        _controller.Parameters.MaximumSlopeAngle = _slopeAngleSave;
        _controller.GravityActive(true);
        _dashEndedNaturally = true;

        // we reset our forces
        if (ResetForcesOnExit)
        {
            _controller.SetForce(Vector2.zero);
        }

      
        // we play our exit sound
        StopStartFeedbacks();
        MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Dash, MMCharacterEvent.Moments.End);
        PlayAbilityStopFeedbacks();

        // once the boost is complete, if we were dashing, we make it stop and start the dash cooldown
        if (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
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

    /// <summary>
    /// Adds required animator parameters to the animator parameters list if they exist
    /// </summary>
    //protected override void InitializeAnimatorParameters()
    //{
    //    RegisterAnimatorParameter(_dashingAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashingAnimationParameter);
    //}

    ///// <summary>
    ///// At the end of the cycle, we update our animator's Dashing state 
    ///// </summary>
    //public override void UpdateAnimator()
    //{
    //    MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Dashing), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
    //}

    /// <summary>
    /// On reset ability, we cancel all the changes made
    /// </summary>
    public override void ResetAbility()
    {
        base.ResetAbility();
        if (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
        {
            StopDash();
        }

        //if (_animator != null)
        //{
        //    MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
        //}
    }
}

