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
        protected int allegiance;
        //index in either the hurt or hitbox list that tells up what hit/hurtbox to look for
        protected int triggerIndex;

        public bool IsActive { get { return this._trigger.Awake; } }
        public int Allegiance { get { return this.allegiance; } }
        public VulnerableObject Owner { get { return this._owner; } }
        public FBox Trigger { get { return this._trigger; } }

        protected override void OnStart()
        {
            base.OnStart();

            this._trigger = this.GetComponent<FBox>();
            this._owner = this.transform.parent.GetComponentInParent<VulnerableObject>();

            //add our own activator event when we reached a frame and may activate this box trigger
            this._owner.OnFrameReached += CheckDataFromFrame;
        }



        void OnDestroy()
        {
            this._owner.OnFrameReached -= CheckDataFromFrame;

        }



        protected void CommonActivateBox(FVector2 pos, FVector2 dim)
        {
            this._trigger.Awake = true;
            this._trigger.LocalPosition = pos;
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


        protected abstract void CheckDataFromFrame(object sender, FrameData data);
        public abstract void DeactivateBox();
    }
}