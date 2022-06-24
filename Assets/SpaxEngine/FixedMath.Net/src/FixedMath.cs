
namespace FixMath.NET
{
    public static class FixedMath
    {
        public static long RawPiOver180 = 0x477D1A8;
        public static long Raw180OverPi = 0x394BB84000;
        public static long RawThreeFourths = 0xC0000000;
        public static long RawHalf = 0x80000000;
        public static long RawFourth = 0x40000000;
        public static long RawTenth = 0x199999A0;
        public static long RawHundreth = 0x28F5C28;
        public static long RawThousanth = 0x418937;
        public static Fix64 Rad2Deg = Fix64.FromRaw(Raw180OverPi);
        public static Fix64 Deg2Rad = Fix64.FromRaw(RawPiOver180);
        public static Fix64 C0p1 = Fix64.FromRaw(RawTenth);
        public static Fix64 C0p01 = Fix64.FromRaw(RawHundreth);
        public static Fix64 C0p001 = Fix64.FromRaw(RawThousanth);
        public static Fix64 C0p75 = Fix64.FromRaw(RawThreeFourths);
        public static Fix64 C0p5 = Fix64.FromRaw(RawHalf);
        public static Fix64 C0p25 = Fix64.FromRaw(RawFourth);
    }
}