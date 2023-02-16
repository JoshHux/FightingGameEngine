using FixMath.NET;
using FlatPhysics.Unity;
using FightingGameEngine.Data;
using Spax;

namespace FightingGameEngine.Gameplay
{
    public abstract class BoxTrigger : SpaxBehavior
    {
        //vulnerable object because this is either a hit or hurtbox and the VulnerableObject is the super to CombatObject
        private VulnerableObject _owner;
        private FBox _trigger;
        //int that tells us what "side" we're on
        [ReadOnly, UnityEngine.SerializeField] protected int allegiance;
        //index in either the hurt or hitbox list that tells up what hit/hurtbox to look for
        [ReadOnly, UnityEngine.SerializeField] protected int triggerIndex;

        public int Allegiance { get { return this.allegiance; } }
        public VulnerableObject Owner { get { return this._owner; } }
        public FBox Trigger { get { return this._trigger; } }

        protected override void OnStart()
        {
            base.OnStart();

            this._trigger = this.GetComponent<FBox>();
            this._owner = this.transform.parent.GetComponentInParent<VulnerableObject>();

            //set rb to sleep
            this._trigger.Body.Awake = false;
            //add our own activator event when we reached a frame and may activate this box trigger
            this._owner.OnFrameReached += CheckDataFromFrame;
            this._owner.OnGameStateSet += ApplyGameState;
        }



        void OnDestroy()
        {
            this._owner.OnFrameReached -= CheckDataFromFrame;
            this._owner.OnGameStateSet -= ApplyGameState;

        }



        protected void CommonActivateBox(FVector2 pos, FVector2 dim)
        {
            this._trigger.Awake = true;

            var ownerFacing = this._owner.Facing;
            //make new position based on the owner's facing direction
            var newPos = new FVector2(pos.x * ownerFacing, pos.y);

            this._trigger.LocalPosition = newPos;
            this._trigger.SetDimensions(dim);
        }


        protected void CommonDeactivateBox()
        {
            this._trigger.Awake = false;
        }
        //should only be called once by each hit and hurtbox
        public void SetTriggerIndex(int newTi)
        {
            this.triggerIndex = newTi;
        }

        //should only be called once by each hit and hurtbox
        public void SetTriggerAllegiance(int newA)
        {
            this.allegiance = newA;
        }

        public int GetTriggerIndex() { return this.triggerIndex; }

        public virtual bool IsActive() { return this._trigger.Awake; }

        protected abstract void CheckDataFromFrame(object sender, in FrameData data);
        protected abstract void ApplyGameState(object sender, in GameplayState state);
        public abstract void DeactivateBox();
    }
}