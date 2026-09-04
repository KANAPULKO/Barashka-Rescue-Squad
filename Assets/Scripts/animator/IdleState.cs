using UnityEngine;
using Ursaanimation.CubicFarmAnimals;

public class IdleState : BaseState
{
    private float _nextSwitchTime;
    private float _switchInterval = 10f;

    public override void Enterstate(AnimationController manager)
    {
        manager.animator.SetBool("Sit", false);
        manager.animator.SetBool("StandVariant", false);
    }

    public override void Exitstate(AnimationController manager)
    {
    }

    public override void Updatestate(AnimationController manager)
    {
    }
}