using FixMath.NET;
using FightingGameEngine.Enum;
using FightingGameEngine.Gameplay;
using MessagePack;
using PleaseResync;

namespace FightingGameEngine.Data
{

    [System.Serializable]
    [MessagePackObject]
    public struct GameplayState 
    {
        [Key(0)]
        public StateID CurrentStateID;
        [Key(1)]
        public PhysicsState PhysicsData;
        //the conditions we have, based on our current state
        [Key(2)]
        public StateConditions CurrentStateConditions;
        //the conditions we process
        [Key(3)]
        public StateConditions PersistSttCond;
        [Key(4)]
        public TimerInfo StateTimer;
        [Key(5)]
        public TimerInfo StopTimer;
        [Key(6)]
        public TimerInfo FlashTimer;
        [Key(7)]
        public TimerInfo PersistTimer;
        [Key(8)]
        public TransitionFlags TransitionFlags;
        [Key(9)]
        public CancelConditions CancelFlags;
        [Key(10)]
        public ResourceData CurrentResources;
        [Key(11)]
        public Fix64 CurrentGravity;
        [Key(12)]
        public Fix64 GravityScaling;
        [Key(13)]
        public Fix64 CurrentProration;
        [Key(14)]
        public Fix64 StoredProration;
        [Key(15)]
        public int CurrentComboCount;

        //public access to bounce info
        [Key(16)]
        public int GroundBounces;
        [Key(17)]
        public Fix64 GroundBounceScaling;
        [Key(18)]
        public int WallBounces;
        [Key(19)]
        public Fix64 WallBounceScaling;
        [Key(20)]
        public InputSnapshot CurrentControllerState;
        [Key(21)]
        public int FacingDir;

        [Key(22)]
        public Arr8<HitboxState> HitboxStates;
        [Key(23)]
        public Arr8<HurtboxData> HurtboxStates;
        [Key(24)]
        public Arr8<int> TransitionInfo;
        [Key(25)]
        public Arr8<int> ProjectileInfo;

        public GameplayState(in CharStateInfo info)
        {
            //get the info from the parameter
            soCharacterStatus status = info.GetStatus();
            HitboxTrigger[] hitboxes = info.GetHitboxes();
            HurtboxTrigger[] hurtboxes = info.GetHurtboxes();

            this.CurrentStateID = status.CurrentState.StateID;
            this.PhysicsData = new PhysicsState(status.CurrentPosition, status.CurrentVelocity, status.CalcVelocity);
            this.CurrentStateConditions = status.CurrentStateConditions;
            this.PersistSttCond = status.ConditionTimer.StateConditions;
            this.StateTimer = new TimerInfo(status.StateTimer);
            this.StopTimer = new TimerInfo(status.StopTimer);
            this.FlashTimer = new TimerInfo(status.SuperFlashTimer);
            this.PersistTimer = new TimerInfo(status.ConditionTimer);
            this.CancelFlags = status.CancelFlags;
            this.TransitionFlags = status.TransitionFlags;
            this.CurrentResources = status.CurrentResources;
            this.CurrentGravity = status.CurrentGravity;
            this.GravityScaling = status.GravityScaling;
            this.CurrentProration = status.CurrentDamageScaling;
            this.StoredProration = status.StoredDamageScaling;
            this.CurrentComboCount = status.CurrentComboCount;
            this.GroundBounces = status.GroundBounces;
            this.GroundBounceScaling = status.GroundBounceScaling;
            this.WallBounces = status.WallBounces;
            this.WallBounceScaling = status.WallBounceScaling;
            this.CurrentControllerState = status.CurrentControllerState;
            this.FacingDir = status.CurrentFacingDirection;
            this.TransitionInfo = status.TransitionInfo;
            this.ProjectileInfo = status.CurProjectiles;

            //set the hitbox states
            this.HitboxStates = new Arr8<HitboxState>();
            //HitboxState has builtin protection in can hitboxes is null
            this.HitboxStates.SetValue(0, new HitboxState(hitboxes[0]));
            this.HitboxStates.SetValue(1, new HitboxState(hitboxes[1]));
            this.HitboxStates.SetValue(2, new HitboxState(hitboxes[2]));
            this.HitboxStates.SetValue(3, new HitboxState(hitboxes[3]));
            this.HitboxStates.SetValue(4, new HitboxState(hitboxes[4]));
            this.HitboxStates.SetValue(5, new HitboxState(hitboxes[5]));
            this.HitboxStates.SetValue(6, new HitboxState(hitboxes[6]));
            this.HitboxStates.SetValue(7, new HitboxState(hitboxes[7]));

            //set the hurtbox states
            this.HurtboxStates = new Arr8<HurtboxData>();
            //if we get an array of empty objects
            if (hurtboxes[0] != null)
            {
                this.HurtboxStates.SetValue(0, hurtboxes[0].GetHurtboxData());
                this.HurtboxStates.SetValue(1, hurtboxes[1].GetHurtboxData());
                this.HurtboxStates.SetValue(2, hurtboxes[2].GetHurtboxData());
                this.HurtboxStates.SetValue(3, hurtboxes[3].GetHurtboxData());
                this.HurtboxStates.SetValue(4, hurtboxes[4].GetHurtboxData());
                this.HurtboxStates.SetValue(5, hurtboxes[5].GetHurtboxData());
                this.HurtboxStates.SetValue(6, hurtboxes[6].GetHurtboxData());
                this.HurtboxStates.SetValue(7, hurtboxes[7].GetHurtboxData());
            }
            else
            {
                this.HurtboxStates.SetValue(0, new HurtboxData());
                this.HurtboxStates.SetValue(1, new HurtboxData());
                this.HurtboxStates.SetValue(2, new HurtboxData());
                this.HurtboxStates.SetValue(3, new HurtboxData());
                this.HurtboxStates.SetValue(4, new HurtboxData());
                this.HurtboxStates.SetValue(5, new HurtboxData());
                this.HurtboxStates.SetValue(6, new HurtboxData());
                this.HurtboxStates.SetValue(7, new HurtboxData());
            }
        }
    }
}