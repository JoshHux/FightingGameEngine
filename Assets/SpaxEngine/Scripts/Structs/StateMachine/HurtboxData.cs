using FixMath.NET;
using MessagePack;
namespace FightingGameEngine.Data
{
    [System.Serializable]
    [MessagePackObject]

    public struct HurtboxData
    {
        [Key(0)] public FVector2 Position;
        [Key(1)] public FVector2 Dimensions;

    }
}