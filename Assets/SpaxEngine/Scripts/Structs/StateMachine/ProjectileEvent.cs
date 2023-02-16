using FixMath.NET;

namespace FightingGameEngine.Data
{
    //info for spawning a projectile 
    [System.Serializable]
    public struct ProjectileEvent
    {
        public int ProjectileInd;
        public FVector2 RelativePos;
        public Fix64 SpawnRotation;

    }
}