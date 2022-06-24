using System;

namespace FixMath.NET
{
    [System.Serializable]
    public struct FVector4 : IEquatable<FVector4>
    {

        public readonly Fix64 x;
        public readonly Fix64 y;
        public readonly Fix64 z;
        public readonly Fix64 w;

        public FVector4(Fix64 x, Fix64 y, Fix64 z, Fix64 w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static FVector4 operator +(FVector4 a, FVector4 b)
        {
            return new FVector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }


        public static bool operator ==(FVector4 a, FVector4 b)
        {
            bool matchX = a.x == b.x;
            bool matchY = a.y == b.y;
            bool matchZ = a.z == b.z;
            bool matchW = a.w == b.w;
            bool ret = matchX && matchY && matchZ && matchW;
            return ret;
        }

        public static bool operator !=(FVector4 a, FVector4 b)
        {
            bool matchX = a.x != b.x;
            bool matchY = a.y != b.y;
            bool matchZ = a.z != b.z;
            bool matchW = a.w != b.w;
            bool ret = matchX || matchY || matchZ || matchW;
            return ret;
        }

        public override bool Equals(object obj)
        {
            return (obj is FVector4) && (((FVector4)obj) == this);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() << 96 | y.GetHashCode() << 64 | z.GetHashCode() << 32 | w.GetHashCode();
        }

        public bool Equals(FVector4 other)
        {
            return other == this;
        }
    }
}