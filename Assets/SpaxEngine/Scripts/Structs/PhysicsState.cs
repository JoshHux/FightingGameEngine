using FixMath.NET;
using MessagePack;


namespace FightingGameEngine.Data
{
    [System.Serializable]
    [MessagePackObject]

    public struct PhysicsState
    {
        [Key(0)]
        public FVector2 CurrentPosition;
        [Key(1)]
        public FVector2 CurrentVelocity;
        [Key(2)]
        public FVector2 CalcVelocity;

        public PhysicsState(FVector2 curPos, FVector2 curVel, FVector2 calcVel)
        {
            this.CurrentPosition = curPos;
            this.CurrentVelocity = curVel;
            this.CalcVelocity = calcVel;
        }
    }
}
