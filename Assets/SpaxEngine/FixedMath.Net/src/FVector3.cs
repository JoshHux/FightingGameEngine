using System;

namespace FixMath.NET
{
    [System.Serializable]
    public struct FVector3 : IEquatable<FVector3>
    {

        public Fix64 x;
        public Fix64 y;
        public Fix64 z;

        public static readonly FVector3 zero = new FVector3(0, 0, 0);

        public FVector3(Fix64 x, Fix64 y, Fix64 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Fix64 Dot(FVector3 a, FVector3 b)
        {
            Fix64 xPart = a.x * b.x;
            Fix64 yPart = a.y * b.y;
            Fix64 zPart = a.z * b.z;
            Fix64 ret = xPart + yPart + zPart;
            return ret;
        }

        public static FVector3 Cross(FVector3 a, FVector3 b)
        {
            Fix64 xPart = a.y * b.z - a.z * b.y;
            Fix64 yPart = a.z * b.x - a.x * b.z;
            Fix64 zPart = a.x * b.y - a.y * b.x;
            FVector3 ret = new FVector3(xPart, yPart, zPart);
            return ret;
        }

        public static FVector3 Min(FVector3 a, FVector3 b)
        {
            var xMin = Fix64.Min(a.x, b.x);
            var yMin = Fix64.Min(a.y, b.y);
            var zMin = Fix64.Min(a.z, b.z);
            var ret = new FVector3(xMin, yMin, zMin);
            return ret;
        }

        public static FVector3 Max(FVector3 a, FVector3 b)
        {

            var xMax = Fix64.Max(a.x, b.x);
            var yMax = Fix64.Max(a.y, b.y);
            var zMax = Fix64.Max(a.z, b.z);
            var ret = new FVector3(xMax, yMax, zMax);
            return ret;
        }

        public static FVector3 operator +(FVector3 a, FVector3 b)
        {
            return new FVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static FVector3 operator *(Fix64 a, FVector3 b)
        {
            return new FVector3(b.x * a, b.y * a, b.z * a);
        }

        public static FVector3 operator *(FVector3 a, Fix64 b)
        {
            return new FVector3(a.x * b, a.y * b, a.z * b);
        }

        public static FVector3 operator -(FVector3 a, FVector3 b)
        {
            return new FVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static FVector3 operator -(FVector3 a)
        {
            return new FVector3(-a.x, -a.y, -a.z);
        }


        public static bool operator ==(FVector3 a, FVector3 b)
        {
            bool matchX = a.x == b.x;
            bool matchY = a.y == b.y;
            bool matchZ = a.z == b.z;
            bool ret = matchX && matchY && matchZ;
            return ret;
        }

        public static bool operator !=(FVector3 a, FVector3 b)
        {
            bool matchX = a.x != b.x;
            bool matchY = a.y != b.y;
            bool matchZ = a.z != b.z;
            bool ret = matchX || matchY || matchZ;
            return ret;
        }


        public override bool Equals(object obj)
        {
            return (obj is FVector3) && (((FVector3)obj) == this);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() << 64 | y.GetHashCode() << 32 | z.GetHashCode();
        }


        public bool Equals(FVector3 other)
        {
            return other == this;
        }

    }
}