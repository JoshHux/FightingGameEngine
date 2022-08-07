using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class FrameData
    {
        //the minimum atframe value should be 1, if 0, then it's invalid
        [SerializeField] private int _atFrame;
        [SerializeField] private int _superFlashDuration;
        [SerializeField] private bool _setVelocity;
        [SerializeField] private FVector2 _appliedVelocity;
        //whether or not we change the gravity of the character
        [SerializeField] private bool _setGravity;
        //  basically ignored if setGravity is false
        [SerializeField] private Fix64 _appliedGravity;
        [SerializeField] private FrameEventData _timerEvents;
        [SerializeField] private StateConditions _toggleCond;
        [SerializeField] private CancelConditions _toggleCancels;
        //what and how do resources change at this frame
        [SerializeField] private ResourceData _resourceChange;
        [SerializeField] private HitboxHolder _hitboxes;
        [SerializeField] private HurtboxHolder _hurtboxes;
        [SerializeField] private ProjectileEvent[] _projectiles;

        //the minimum atframe value should be 1, if 0, then it's invalid
        public int AtFrame { get { return this._atFrame; } }
        public int SuperFlashDuration { get { return this._superFlashDuration; } }

        public bool SetVelocity { get { return this._setVelocity; } }
        public FVector2 AppliedVelocity { get { return this._appliedVelocity; } }
        public bool SetGravity { get { return this._setGravity; } }
        public Fix64 AppliedGravity { get { return this._appliedGravity; } }

        public StateConditions ToggleConditions { get { return this._toggleCond; } }
        public CancelConditions ToggleCancels { get { return this._toggleCancels; } }

        //what and how do resources change at this frame
        public ResourceData ResourceChange { get { return this._resourceChange; } }
        public FrameEventData TimerEvent { get { return this._timerEvents; } }
        public ProjectileEvent[] Projectiles { get { return this._projectiles; } }


        //returns whether or not this frame has a projectiles to spawn
        public bool HasProjectile()
        {
            var ret = this._projectiles.Length > 0;
            return ret;
        }

        //returns the hitboxdata at index i, returns the first hitbox data if out of bounds
        //for hitboxes to quickly get the necessary data quickly
        public HitboxData GetHitbox(int i)
        {
            var ret = this._hitboxes.GetHitbox(i);

            //return the value set to ret
            return ret;
        }

        //returns the hurtboxdata at index i, returns the first hurtbox data if out of bounds
        //for hurtboxes to quickly get the necessary data quickly
        public HurtboxData GetHurtbox(int i)
        {
            //default value is the first hurtbox
            var ret = this._hurtboxes.GetHurtbox(i);


            //return the value set to ret
            return ret;
        }
    }
}