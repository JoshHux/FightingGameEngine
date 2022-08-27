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
        public short Resource1;
        public short Resource2;
        public short Resource3;
        public short Resource4;
        public short Resource5;
        public short Resource6;
        public short Resource7;

        public ResourceData(int h, short m, short r1, short r2, short r3, short r4, short r5, short r6, short r7)
        {
            this.Health = h;
            this.Meter = m;
            this.Resource1 = r1;
            this.Resource2 = r2;
            this.Resource3 = r3;
            this.Resource4 = r4;
            this.Resource5 = r5;
            this.Resource6 = r6;
            this.Resource7 = r7;
        }

        //check if the other ResourceData has at least the data in the one
        public bool Check(ResourceData other)
        {
            var hCheck = this.Health <= other.Health;
            var mCheck = hCheck && (this.Meter <= other.Meter);
            var r1Check = mCheck && (this.Resource1 <= other.Resource1);
            var r2Check = r1Check && (this.Resource2 <= other.Resource2);
            var r3Check = r2Check && (this.Resource3 <= other.Resource3);
            var r4Check = r3Check && (this.Resource4 <= other.Resource4);
            var r5Check = r4Check && (this.Resource5 <= other.Resource5);
            var r6Check = r5Check && (this.Resource6 <= other.Resource6);
            var r7Check = r6Check && (this.Resource7 <= other.Resource7);

            bool ret = r7Check;

            return ret;
        }

        //set all variables to another ResourceData's variables if it exceeds the value
        public ResourceData SetMin(ResourceData other)
        {
            if (this.Health > other.Health)
            {
                this.Health = other.Health;
            }
            if (this.Meter > other.Meter)
            {
                this.Meter = other.Meter;
            }
            if (this.Resource1 > other.Resource1)
            {
                //Debug.Log("setting max resource to Resource1");
                //Debug.Log("before :: " + this.Resource1 + " vs " + other.Resource1);
                this.Resource1 = other.Resource1;
                //Debug.Log("after :: " + this.Resource1 + " vs " + other.Resource1);
            }
            if (this.Resource2 > other.Resource2)
            {
                this.Resource2 = other.Resource2;
            }
            if (this.Resource3 > other.Resource3)
            {
                this.Resource3 = other.Resource3;
            }
            if (this.Resource4 > other.Resource4)
            {
                this.Resource4 = other.Resource4;
            }
            if (this.Resource5 > other.Resource5)
            {
                this.Resource5 = other.Resource5;
            }
            if (this.Resource6 > other.Resource6)
            {
                this.Resource6 = other.Resource6;
            }
            if (this.Resource7 > other.Resource7)
            {
                this.Resource7 = other.Resource7;
            }

            return new ResourceData(this.Health, this.Meter, this.Resource1, this.Resource2, this.Resource3, this.Resource4, this.Resource5, this.Resource6, this.Resource7);
        }

        public static ResourceData operator -(ResourceData a, ResourceData b)
        {
            var newH = a.Health - b.Health;
            var newM = (short)(a.Meter - b.Meter);
            var newR1 = (short)(a.Resource1 - b.Resource1);
            var newR2 = (short)(a.Resource2 - b.Resource2);
            var newR3 = (short)(a.Resource3 - b.Resource3);
            var newR4 = (short)(a.Resource4 - b.Resource4);
            var newR5 = (short)(a.Resource5 - b.Resource5);
            var newR6 = (short)(a.Resource6 - b.Resource6);
            var newR7 = (short)(a.Resource7 - b.Resource7);

            var ret = new ResourceData(newH, newM, newR1, newR2, newR3, newR4, newR5, newR6, newR7);

            return ret;
        }

        public static ResourceData operator +(ResourceData a, ResourceData b)
        {
            var newH = a.Health + b.Health;
            var newM = (short)(a.Meter + b.Meter);
            var newR1 = (short)(a.Resource1 + b.Resource1);
            var newR2 = (short)(a.Resource2 + b.Resource2);
            var newR3 = (short)(a.Resource3 + b.Resource3);
            var newR4 = (short)(a.Resource4 + b.Resource4);
            var newR5 = (short)(a.Resource5 + b.Resource5);
            var newR6 = (short)(a.Resource6 + b.Resource6);
            var newR7 = (short)(a.Resource7 + b.Resource7);

            var ret = new ResourceData(newH, newM, newR1, newR2, newR3, newR4, newR5, newR6, newR7);

            return ret;
        }

        public static ResourceData operator *(ResourceData a, int b)
        {
            var newH = a.Health * b;
            var newM = (short)(a.Meter * b);
            var newR1 = (short)(a.Resource1 * b);
            var newR2 = (short)(a.Resource2 * b);
            var newR3 = (short)(a.Resource3 * b);
            var newR4 = (short)(a.Resource4 * b);
            var newR5 = (short)(a.Resource5 * b);
            var newR6 = (short)(a.Resource6 * b);
            var newR7 = (short)(a.Resource7 * b);

            var ret = new ResourceData(newH, newM, newR1, newR2, newR3, newR4, newR5, newR6, newR7);

            return ret;

        }

        public static ResourceData operator *(ResourceData a, Fix64 b)
        {
            var newH = (int)(a.Health * b);
            var newM = (short)(a.Meter * b);
            var newR1 = (short)(a.Resource1 * b);
            var newR2 = (short)(a.Resource2 * b);
            var newR3 = (short)(a.Resource3 * b);
            var newR4 = (short)(a.Resource4 * b);
            var newR5 = (short)(a.Resource5 * b);
            var newR6 = (short)(a.Resource6 * b);
            var newR7 = (short)(a.Resource7 * b);

            var ret = new ResourceData(newH, newM, newR1, newR2, newR3, newR4, newR5, newR6, newR7);

            return ret;

        }
    }
}
