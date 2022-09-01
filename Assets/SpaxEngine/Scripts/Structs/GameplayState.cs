using FixMath.NET;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Data
{

    [System.Serializable]
    public struct GameplayState
    {
        public StateID CurrentStateID;
        public PhysicsState PhysicsData;
        //the conditions we have, based on our current state
        public StateConditions CurrentStateConditions;
        //the conditions we process
        public StateConditions PersistSttCond;
        public TimerInfo StateTimer;
        public TimerInfo StopTimer;
        public TimerInfo FlashTimer;
        public TimerInfo PersistTimer;
        public TransitionFlags TransitionFlags;
        public CancelConditions CancelFlags;
        public ResourceData CurrentResources;
        public Fix64 CurrentGravity;
        public Fix64 CurrentProration;
        public Fix64 StoredProration;
        public int CurrentComboCount;

        //public access to bounce info
        public int GroundBounces;
        public Fix64 GroundBounceScaling;
        public int WallBounces;
        public Fix64 WallBounceScaling;
        public InputItem CurrentControllerState;
        public int FacingDir;

        public GameplayState(in soCharacterStatus status)
        {
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
            this.CurrentProration = status.CurrentDamageScaling;
            this.StoredProration = status.StoredDamageScaling;
            this.CurrentComboCount = status.CurrentComboCount;
            this.GroundBounces = status.GroundBounces;
            this.GroundBounceScaling = status.GroundBounceScaling;
            this.WallBounces = status.WallBounces;
            this.WallBounceScaling = status.WallBounceScaling;
            this.CurrentControllerState = status.CurrentControllerState;
            this.FacingDir = status.CurrentFacingDirection;
        }
    }
}