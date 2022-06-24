using FixMath.NET;

namespace FlatPhysics
{
    internal readonly struct FlatTransform
    {
        public readonly Fix64 PositionX;
        public readonly Fix64 PositionY;
        public readonly Fix64 Sin;
        public readonly Fix64 Cos;

        public readonly static FlatTransform Zero = new FlatTransform(0, 0, 0);

        public FlatTransform(FVector2 position, Fix64 angle)
        {
            this.PositionX = position.x;
            this.PositionY = position.y;
            this.Sin = Fix64.Sin(angle);
            this.Cos = Fix64.Cos(angle);
        }

        public FlatTransform(Fix64 x, Fix64 y, Fix64 angle)
        {
            this.PositionX = x;
            this.PositionY = y;
            this.Sin = Fix64.Sin(angle);
            this.Cos = Fix64.Cos(angle);
        }


    }
}
