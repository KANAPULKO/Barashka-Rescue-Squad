using UnityEngine;
using Ursaanimation.CubicFarmAnimals;

public class SitState : BaseState
{
    public override void Enterstate(AnimationController manager)
    {
        manager.animator.SetBool("Sit", true);
        manager.animator.SetBool("StandVariant", false);
    }
    public override void Exitstate(AnimationController manager)
    {

    }
    public override void Updatestate(AnimationController manager)
    {

    }
}