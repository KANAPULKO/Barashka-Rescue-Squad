using UnityEngine;
using Ursaanimation.CubicFarmAnimals;

public class IdleVariant : BaseState
{
    public override void Enterstate(AnimationController manager)
    {
        manager.animator.SetBool("Sit", false);
        manager.animator.SetBool("StandVariant", true);
    }
    public override void Exitstate(AnimationController manager)
    {

    }
    public override void Updatestate(AnimationController manager)
    {
    }
}
