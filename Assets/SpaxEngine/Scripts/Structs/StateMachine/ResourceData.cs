using FixMath.NET;
using UnityEngine;
namespace FightingGameEngine.Data
{
    //represents one instance of an input
    [System.Serializable]
    public struct ResourceData
    {
        public int Health;
        //32767 is the max value for the next 8 resources
        public short Meter;
        public short Burst;
        public short Pressure;
        public short Airjumps;
        public short Airdashes;
        public short Resource1;
        public short Resource2;
        public short Resource3;


        public ResourceData(int h, short m, short r4, short r5, short r6, short r7, short r1, short r2, short r3)
        {
            this.Health = h;
            this.Meter = m;
            this.Airjumps = r4;
            this.Airdashes = r5;
            this.Burst = r6;
            this.Pressure = r7;
            this.Resource1 = r1;
            this.Resource2 = r2;
            this.Resource3 = r3;
        }

        //check if the other ResourceData has at least the data in the one
        public bool Check(ResourceData other)
        {
            var hCheck = this.Health <= other.Health;
            var mCheck = hCheck && (this.Meter <= other.Meter);
            var r1Check = mCheck && (this.Resource1 <= other.Resource1);
            var r2Check = r1Check && (this.Resource2 <= other.Resource2);
            var r3Check = r2Check && (this.Resource3 <= other.Resource3);
            var r4Check = r3Check && (this.Airjumps <= other.Airjumps);
            var r5Check = r4Check && (this.Airdashes <= other.Airdashes);
            var r6Check = r5Check && (this.Burst <= other.Burst);
            var r7Check = r6Check && (this.Pressure <= other.Pressure);

            bool ret = r7Check;

            return ret;
        }

        //set all variables to another ResourceData's variables if it exceeds the value
        public static ResourceData Min(ResourceData a, ResourceData b)
        {
            short r1 = (short)Mathf.Min(a.Health, b.Health);
            short r2 = (short)Mathf.Min(a.Meter, b.Meter);
            short r6 = (short)Mathf.Min(a.Airjumps, b.Airjumps);
            short r7 = (short)Mathf.Min(a.Airdashes, b.Airdashes);
            short r8 = (short)Mathf.Min(a.Burst, b.Burst);
            short r9 = (short)Mathf.Min(a.Pressure, b.Pressure);
            short r3 = (short)Mathf.Min(a.Resource1, b.Resource1);
            short r4 = (short)Mathf.Min(a.Resource2, b.Resource2);
            short r5 = (short)Mathf.Min(a.Resource3, b.Resource3);

            return new ResourceData(r1, r2, r6, r7, r8, r9, r3, r4, r5);
        }


        //set all variables to another ResourceData's variables if it exceeds the value
        public static ResourceData Max(ResourceData a, ResourceData b)
        {
            short r1 = (short)Mathf.Max(a.Health, b.Health);
            short r2 = (short)Mathf.Max(a.Meter, b.Meter);
            short r3 = (short)Mathf.Max(a.Resource1, b.Resource1);
            short r4 = (short)Mathf.Max(a.Resource2, b.Resource2);
            short r5 = (short)Mathf.Max(a.Resource3, b.Resource3);
            short r6 = (short)Mathf.Max(a.Airjumps, b.Airjumps);
            short r7 = (short)Mathf.Max(a.Airdashes, b.Airdashes);
            short r8 = (short)Mathf.Max(a.Burst, b.Burst);
            short r9 = (short)Mathf.Max(a.Pressure, b.Pressure);

            return new ResourceData(r1, r2, r6, r7, r8, r9, r3, r4, r5);
        }


        //return only values that are NOT negative in newR
        public ResourceData FilterNeg(ResourceData newR)
        {
            //whether the first val is negative or not
            int negR1 = (((ushort)newR.Health) >> 15);
            int negR2 = (((ushort)newR.Meter) >> 15);
            int negR3 = (((ushort)newR.Resource1) >> 15);
            int negR4 = (((ushort)newR.Resource2) >> 15);
            int negR5 = (((ushort)newR.Resource3) >> 15);
            int negR6 = (((ushort)newR.Airjumps) >> 15);
            int negR7 = (((ushort)newR.Airdashes) >> 15);
            int negR8 = (((ushort)newR.Burst) >> 15);
            int negR9 = (((ushort)newR.Pressure) >> 15);


            short r1 = (short)(this.Health * (negR1) + newR.Health * (negR1 ^ 1));
            short r2 = (short)(this.Meter * (negR2) + newR.Meter * (negR2 ^ 1));
            short r3 = (short)(this.Resource1 * (negR3) + newR.Resource1 * (negR3 ^ 1));
            short r4 = (short)(this.Resource2 * (negR4) + newR.Resource2 * (negR4 ^ 1));
            short r5 = (short)(this.Resource3 * (negR5) + newR.Resource3 * (negR5 ^ 1));
            short r6 = (short)(this.Airjumps * (negR6) + newR.Airjumps * (negR6 ^ 1));
            short r7 = (short)(this.Airdashes * (negR7) + newR.Airdashes * (negR7 ^ 1));
            short r8 = (short)(this.Burst * (negR8) + newR.Burst * (negR8 ^ 1));
            short r9 = (short)(this.Pressure * (negR9) + newR.Pressure * (negR9 ^ 1));

            return new ResourceData(r1, r2, r6, r7, r8, r9, r3, r4, r5);
        }


        public static ResourceData operator -(ResourceData a, ResourceData b)
        {
            var newH = a.Health - b.Health;
            var newM = (short)(a.Meter - b.Meter);
            var newR1 = (short)(a.Resource1 - b.Resource1);
            var newR2 = (short)(a.Resource2 - b.Resource2);
            var newR3 = (short)(a.Resource3 - b.Resource3);
            var newR4 = (short)(a.Airjumps - b.Airjumps);
            var newR5 = (short)(a.Airdashes - b.Airdashes);
            var newR6 = (short)(a.Burst - b.Burst);
            var newR7 = (short)(a.Pressure - b.Pressure);

            var ret = new ResourceData(newH, newM, newR4, newR5, newR6, newR7, newR1, newR2, newR3);

            return ret;
        }

        public static ResourceData operator +(ResourceData a, ResourceData b)
        {
            var newH = a.Health + b.Health;
            var newM = (short)(a.Meter + b.Meter);
            var newR1 = (short)(a.Resource1 + b.Resource1);
            var newR2 = (short)(a.Resource2 + b.Resource2);
            var newR3 = (short)(a.Resource3 + b.Resource3);
            var newR4 = (short)(a.Airjumps + b.Airjumps);
            var newR5 = (short)(a.Airdashes + b.Airdashes);
            var newR6 = (short)(a.Burst + b.Burst);
            var newR7 = (short)(a.Pressure + b.Pressure);

            var ret = new ResourceData(newH, newM, newR4, newR5, newR6, newR7, newR1, newR2, newR3);

            return ret;
        }

        public static ResourceData operator *(ResourceData a, int b)
        {
            var newH = a.Health * b;
            var newM = (short)(a.Meter * b);
            var newR1 = (short)(a.Resource1 * b);
            var newR2 = (short)(a.Resource2 * b);
            var newR3 = (short)(a.Resource3 * b);
            var newR4 = (short)(a.Airjumps * b);
            var newR5 = (short)(a.Airdashes * b);
            var newR6 = (short)(a.Burst * b);
            var newR7 = (short)(a.Pressure * b);

            var ret = new ResourceData(newH, newM, newR4, newR5, newR6, newR7, newR1, newR2, newR3);

            return ret;

        }

        public static ResourceData operator *(ResourceData a, Fix64 b)
        {
            var newH = (int)(a.Health * b);
            var newM = (short)(a.Meter * b);
            var newR1 = (short)(a.Resource1 * b);
            var newR2 = (short)(a.Resource2 * b);
            var newR3 = (short)(a.Resource3 * b);
            var newR4 = (short)(a.Airjumps * b);
            var newR5 = (short)(a.Airdashes * b);
            var newR6 = (short)(a.Burst * b);
            var newR7 = (short)(a.Pressure * b);

            var ret = new ResourceData(newH, newM, newR4, newR5, newR6, newR7, newR1, newR2, newR3);

            return ret;

        }
    }
}
