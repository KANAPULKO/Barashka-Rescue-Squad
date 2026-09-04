using UnityEngine;
using Ursaanimation.CubicFarmAnimals;

public abstract class BaseState
{
    public abstract void Updatestate(AnimationController manager);
    public abstract void Enterstate(AnimationController manager);
    public abstract void Exitstate(AnimationController manager);

}
