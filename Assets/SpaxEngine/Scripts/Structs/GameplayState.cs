using FixMath.NET;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Data
{
    public struct GameplayState
    {
        public int CurrentStateInd;
        public PhysicsState PhysicsData;
        //the conditions we have, based on our current state
        public StateConditions CurrentStateConditions;
        //the conditions we process
        public StateConditions TotalStateConditions;
        public TransitionFlags TransitionFlags;
        public CancelConditions CancelFlags;
        public ResourceData CurrentResources;
        public Fix64 CurrentGravity;
        public Fix64 CurrentProration;
        public int CurrentComboCount;

        //public access to bounce info
        public int GroundBounces;
        public Fix64 GroundBounceScaling;
        public int WallBounces;
        public Fix64 WallBounceScaling;
        public InputItem CurrentControllerState;
    }
}