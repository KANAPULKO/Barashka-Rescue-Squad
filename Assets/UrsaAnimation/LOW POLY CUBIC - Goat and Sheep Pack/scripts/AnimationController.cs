using UnityEngine;

namespace Ursaanimation.CubicFarmAnimals
{
    public class AnimationController : MonoBehaviour
    {
        [SerializeField] public Animator animator;
        BaseState currentState;
        public IdleState IdleState = new IdleState();
        public SitState SitState = new SitState();
        public IdleVariant IdleVariant = new IdleVariant();

        private float _nextSwitchTime;
        private float _switchInterval = 10f;
        private bool _isTransitioning = false;

        public void SwitchState(BaseState newState)
        {
            if (currentState != null && !_isTransitioning)
            {
                currentState.Exitstate(this);
            }
            currentState = newState;
            currentState.Enterstate(this);
        }

        public void Start()
        {
            SwitchState(IdleState);
            _nextSwitchTime = Time.time + _switchInterval;
        }

        public void Update()
        {
            // Проверяем, можно ли переключать состояние (только по таймеру и не во время анимации)
            if (Time.time >= _nextSwitchTime && !_isTransitioning)
            {
                CheckAnimalState();
                _nextSwitchTime = Time.time + _switchInterval;
            }

            currentState.Updatestate(this);
        }

        void CheckAnimalState()
        {
            if (currentState == IdleState)
            {
                if (Random.Range(0, 2) == 0)
                    SwitchState(IdleVariant);
                else
                    SwitchState(SitState);
            }
            else
            {
                SwitchState(IdleState);
            }
        }

        

        public void EndTransition()
        {
            _isTransitioning = false;
        }

        public void OnAnimationComplete()
        {
            EndTransition();
        }
    }
}