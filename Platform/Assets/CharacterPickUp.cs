using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPickUp : CharacterAbility
{
    public GameObject CurrentGoOnHand;

    protected override void Initialization()
    {
        base.Initialization();
    }
    protected override void HandleInput()
    {
        if (_inputManager.DashButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
        {
          
        }
    }

    protected override void InternalHandleInput()
    {
        base.InternalHandleInput();
    }

    public override void ProcessAbility()
    {
        base.ProcessAbility();
    }
}
