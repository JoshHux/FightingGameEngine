namespace FixMath.NET
{
    [System.Serializable]
    public struct FMatrix4x4
    {
        public Fix64 m00, m01, m02, m03,
                        m10, m11, m12, m13,
                        m20, m21, m22, m23,
                        m30, m31, m32, m33;

        public static readonly FMatrix4x4 Identity = new FMatrix4x4(new FVector4(Fix64.One, Fix64.Zero, Fix64.Zero, Fix64.Zero),
                                                                        new FVector4(Fix64.Zero, Fix64.One, Fix64.Zero, Fix64.Zero),
                                                                        new FVector4(Fix64.Zero, Fix64.Zero, Fix64.One, Fix64.Zero),
                                                                        new FVector4(Fix64.Zero, Fix64.Zero, Fix64.Zero, Fix64.One));

        public FMatrix4x4(FVector4 r0, FVector4 r1, FVector4 r2, FVector4 r3)
        {
            m00 = r0.x; m01 = r0.y; m02 = r0.z; m03 = r0.w;
            m10 = r1.x; m11 = r1.y; m12 = r1.z; m13 = r1.w;
            m20 = r2.x; m21 = r0.y; m22 = r2.z; m23 = r2.w;
            m30 = r3.x; m31 = r0.y; m32 = r3.z; m33 = r2.w;
        }

        public static bool operator ==(FMatrix4x4 a, FMatrix4x4 b)
        {
            bool matchX = (a.m00 == b.m00) && (a.m10 == b.m10) && (a.m20 == b.m20) && (a.m30 == b.m30);
            bool matchY = (a.m01 == b.m01) && (a.m11 == b.m11) && (a.m21 == b.m21) && (a.m31 == b.m31);
            bool matchZ = (a.m02 == b.m02) && (a.m12 == b.m12) && (a.m22 == b.m22) && (a.m32 == b.m32);
            bool matchW = (a.m03 == b.m03) && (a.m13 == b.m13) && (a.m23 == b.m23) && (a.m33 == b.m33);
            bool ret = matchX && matchY && matchZ && matchW;
            return ret;
        }

        public static bool operator !=(FMatrix4x4 a, FMatrix4x4 b)
        {
            bool matchX = (a.m00 != b.m00) || (a.m10 != b.m10) || (a.m20 != b.m20) || (a.m30 != b.m30);
            bool matchY = (a.m01 != b.m01) || (a.m11 != b.m11) || (a.m21 != b.m21) || (a.m31 != b.m31);
            bool matchZ = (a.m02 != b.m02) || (a.m12 != b.m12) || (a.m22 != b.m22) || (a.m32 != b.m32);
            bool matchW = (a.m03 != b.m03) || (a.m13 != b.m13) || (a.m23 != b.m23) || (a.m33 != b.m33);
            bool ret = matchX || matchY || matchZ || matchW;
            return ret;
        }

        public override bool Equals(object obj)
        {
            return (obj is FMatrix4x4) && (((FMatrix4x4)obj) == this);
        }
        public override int GetHashCode()
        {
            return m00.GetHashCode();
        }
    }
}