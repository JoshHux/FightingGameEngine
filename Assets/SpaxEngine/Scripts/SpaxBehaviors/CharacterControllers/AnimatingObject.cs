using System.Linq;
using UnityEngine;
using FixMath.NET;
using FlatPhysics.Unity;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Gameplay
{
    public class AnimatingObject : RendererBehavior
    {
        [SerializeField] private soCharacterStatus _status;
        [SerializeField, ReadOnly] private Animator _animator;

        protected override void OnStart()
        {
            base.OnStart();

            this._animator = ObjectFinder.FindChildWithTag(this.gameObject, "RenderingContainer").GetComponentInChildren<Animator>();
        }

        protected override void PreRenderUpdate() { }
        protected override void RenderUpdate()
        {
            //get the animation frame at the current state timer count
            int timeInState = this._status.StateTimer.TimeElapsed;
            AnimationFrameData frame = this._status.CurrentState.GetAnimationAt(timeInState);

            //checking if there is data to process
            if (frame != null)
            {
                this.ProcessAnimationData(frame);
            }
        }
        protected override void PostRenderUpdate() { }

        //call to process a given animationFrameData
        private void ProcessAnimationData(AnimationFrameData afd)
        {
            //if we have a non-empty animation name
            if (afd.AnimationName.Length > 0)
            {
                //do we ignore this animation name if it's the same as the current one?
                bool ignoreSameName = afd.SkipIfSameName;

                //get currently playing animation's info
                var animStateinfo = this._animator.GetCurrentAnimatorStateInfo(0);
                //are the two states the same?
                var sameStateName = ignoreSameName && animStateinfo.IsName(afd.AnimationName);

                //do we want to skip changing the animation?
                var skipAnimChange = sameStateName;

                if (!skipAnimChange)
                {
                    //we want to change the animation

                    //fixed delta time
                    var delta = Time.fixedDeltaTime;

                    //start time in real time
                    var startTime = delta * afd.StartFrame;

                    //assign the new animation state
                    this._animator.PlayInFixedTime(afd.AnimationName, 0, startTime);

                }

            }


        }

    }
}