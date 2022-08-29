using FixMath.NET;


namespace FightingGameEngine.Data
{
    [System.Serializable]
    public struct PhysicsState
    {
        public FVector2 CurrentPosition;
        public FVector2 CurrentVelocity;
        public FVector2 CalcVelocity;

        public PhysicsState(FVector2 curPos, FVector2 curVel, FVector2 calcVel)
        {
            this.CurrentPosition = curPos;
            this.CurrentVelocity = curVel;
            this.CalcVelocity = calcVel;
        }
    }
}
