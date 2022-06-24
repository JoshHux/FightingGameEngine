using FixMath.NET;

namespace FlatPhysics
{
    public readonly struct FlatAABB
    {
        public readonly FVector2 Min;
        public readonly FVector2 Max;

        public FlatAABB(FVector2 min, FVector2 max)
        {
            this.Min = min;
            this.Max = max;
        }

        public FlatAABB(Fix64 minX, Fix64 minY, Fix64 maxX, Fix64 maxY)
        {
            this.Min = new FVector2(minX, minY);
            this.Max = new FVector2(maxX, maxY);
        }
    }
}
