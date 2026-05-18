using System.Linq;
using UnityEngine;
using FixMath.NET;
using FlatPhysics.Unity;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;
using System.Threading;
using Spax;

namespace FightingGameEngine.Gameplay
{
    public class AnimatingObject : RendererBehavior
    {
        [SerializeField] private soCharacterStatus _status;
        [SerializeField] private soCharacterData _data;
        [SerializeField, ReadOnly] private Animator _animator;
        [SerializeField, ReadOnly] private AudioSource _audioSource;
        [SerializeField, ReadOnly] private float _animSpeed;
        [SerializeField, ReadOnly] private int _lastProcessedFrame;
        [SerializeField, ReadOnly] private soStateData _lastProcessedState;


        public soCharacterStatus Status { set { this._status = value; } }

        public Animator PlayerAnimator
        {
            get
            {

                if (this._animator == null) { this._animator = ObjectFinder.FindChildWithTag(this.gameObject, "RenderingContainer").GetComponentInChildren<Animator>(); }
                return this._animator;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            this._audioSource = ObjectFinder.FindChildWithTag(this.gameObject, "RenderingContainer").GetComponentInChildren<AudioSource>();

            this._animator = ObjectFinder.FindChildWithTag(this.gameObject, "RenderingContainer").GetComponentInChildren<Animator>();
            //Debug.Log(this._animator);
            this._animSpeed = 1f;

            this._lastProcessedFrame = -1;
            this._lastProcessedState = null;
        }

        protected override void PreRenderUpdate()
        {
        }

        protected override void RenderUpdate()
        {
            //if we're in hitstop, don't continue the animation
            bool inHitstop = this._status.InHitstop;
            //get the animation frame at the current state timer count
            int timeInState = this._status.StateTimer.TimeElapsed;

            if (inHitstop)
            {
                this._animator.speed = 0;
                int isStunState = (int)EnumHelper.HasEnumInt((uint)this._status.TotalStateConditions, (uint)StateConditions.STUN_STATE);
                int isHitstopNonzero = (int)EnumHelper.isNotZero((uint)this._status.StopTimer.TimeElapsed) * isStunState;
                timeInState = Mathf.Max(timeInState, isHitstopNonzero);
            }
            else
            {
                this._animator.speed = this._animSpeed;

                AnimationFrameData frame = this._status.CurrentState.GetAnimationAt(timeInState);
                //if (this._status.CurrentState.name == "Jab") { Debug.Log("state timer in renderupdate is - " + this._status.StateTimer.TimeElapsed + "/" + this._status.StateTimer.EndTime); }

                //checking if there is data to process
                if (frame != null)
                {
                    //Debug.Log("found frame - " + timeInState);
                    this.ProcessAnimationData(frame);
                }
            }

            var rendererTransform = this._animator.transform;
            var rendererScale = rendererTransform.localScale;
            var facingDir = this._status.CurrentFacingDirection;

            var newScale = new Vector3(facingDir, rendererScale.y, rendererScale.z);
            this.transform.localScale = newScale;

            if (this._status.RendererInfo.VFXID >= 0)
            {
                //convert player position to Vector3
                Vector3 playerPos = new Vector3((float)this._status.CurrentPosition.x, (float)this._status.CurrentPosition.y, 0);
                //shortcut
                RendererInfo ri = this._status.RendererInfo;
                //print(this._status);
                //this is gonna run into issues really quick, we need a better way to handle vfx
                Instantiate(this._data.VFXValues.VFXList[ri.VFXID], ri.VFXPos, Quaternion.identity);
                this._status.RendererInfo.VFXID = -1;
            }


            if (EnumHelper.HasEnum((uint)this._status.RendererInfo.RelevantEvents, (uint)TransitionFlags.GOT_HIT)) { this.GotHitStuff(); } else { this._stillInStun = false; }

        }
        protected override void PostRenderUpdate()
        {
            print("post render");
            this._status.RendererInfo = new RendererInfo(-1, Vector3.zero);
        }

        //call to process a given animationFrameData
        private void ProcessAnimationData(AnimationFrameData afd)
        {
            if (this._lastProcessedState == this._status.CurrentState && this._lastProcessedFrame == afd.AtFrame) { return; }

            //if we have a non-empty animation name
            if (afd.AnimationName.Length > 0)
            {
                //do we ignore this animation name if it's the same as the current one?
                bool ignoreSameName = afd.SkipIfSameName;
                //default animation speed
                this._animSpeed = 1f;

                //get currently playing animation's info
                var animStateinfo = this._animator.GetCurrentAnimatorStateInfo(0);
                //are the two states the same?
                var sameStateName = ignoreSameName && animStateinfo.IsName(afd.AnimationName);

                //do we want to skip changing the animation?
                var skipAnimChange = sameStateName;

                if (!skipAnimChange)
                {
                    //Debug.Log(this.name + " transitioning animation to " + afd.AnimationName);
                    //we want to change the animation

                    //fixed delta time
                    var delta = Time.fixedDeltaTime;

                    //start time in real time
                    var startTime = delta * afd.StartFrame;

                    //assign the new animation state
                    this._animator.PlayInFixedTime(afd.AnimationName, 0, startTime);
                    this._animSpeed = (float)afd.AnimationSpeed;
                    //this._animator.speed = (float)afd.AnimationSpeed;



                }

            }

            if (afd.VFX >= 0 && this._data.VFXValues.VFXList.Count > 0)
            {
                Vector3 spawnPos = new Vector3((float)afd.VFXPosition.x, (float)afd.VFXPosition.y, (float)afd.VFXPosition.z);
                Vector3 playerPos = new Vector3((float)this._status.CurrentPosition.x, (float)this._status.CurrentPosition.y, 0);
                Quaternion spawnRot = Quaternion.Euler((float)afd.VFXRotation.x, (float)afd.VFXRotation.y, (float)afd.VFXRotation.z);
                Instantiate(this._data.VFXValues.VFXList[afd.VFX], playerPos + spawnPos, spawnRot);
            }

            if (afd.ScreenShake > 0 && afd.ShakeDuration > 0)
            {
                CameraBehavior.Instance.ShakeCam((float)afd.ScreenShake, (float)afd.ShakeDuration);
            }

            if (afd.SoundFX.Length > 0)
            {
                var soundClip = this._data.SoundList.Find(o => o.name == afd.SoundFX);
                this._audioSource.clip = soundClip;
                this._audioSource.Play();
            }

            this._lastProcessedFrame = afd.AtFrame;
            this._lastProcessedState = this._status.CurrentState;
        }

        private bool _stillInStun = false;
        //this is a placeholder before I figure out a better way to process events on getting hit
        private void GotHitStuff()
        {

            if (this._stillInStun) { return; }

            if (EnumHelper.HasEnum((uint)this._status.RendererInfo.RelevantEvents, (uint)TransitionFlags.BLOCKED_HIT, true))
            {
                var soundClip = this._data.SoundList.Find(o => o.name == "Parry");
                this._audioSource.clip = soundClip;
                this._audioSource.Play();
            }
            else
            {
                var soundClip = this._data.SoundList.Find(o => o.name == "Shove1");

                if (this._status.CurrentHP < 1) { soundClip = this._data.SoundList.Find(o => o.name == "Big Hit"); }

                this._audioSource.clip = soundClip;
                this._audioSource.Play();
                CameraBehavior.Instance.ShakeCam(0.1f, 0.1f);
            }

            if (SpaxManager.Instance.IsTraining) { this._status.CurrentHP = 1; }

            this._stillInStun = true;
            this._status.RendererInfo.RelevantEvents &= ~TransitionFlags.GOT_HIT;
        }

    }
}