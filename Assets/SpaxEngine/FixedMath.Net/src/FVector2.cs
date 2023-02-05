/*
 *  VolatilePhysics - A 2D Physics Library for Networked Games
 *  Copyright (c) 2015-2016 - Alexander Shoulson - http://ashoulson.com
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty. In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *  
 *  1. The origin of this software must not be misrepresented; you must not
 *     claim that you wrote the original software. If you use this software
 *     in a product, an acknowledgment in the product documentation would be
 *     appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *     misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
*/
using System;
//using Newtonsoft.Json;
namespace FixMath.NET
{
    [Serializable]
    public struct FVector2 : IEquatable<FVector2>
    {
        public static FVector2 zero { get { return new FVector2(Fix64.Zero, Fix64.Zero); } }
        public static FVector2 one { get { return new FVector2(Fix64.One, Fix64.One); } }

        public static Fix64 Dot(FVector2 a, FVector2 b)
        {
            return (a.x * b.x) + (a.y * b.y);
        }

        //public readonly Fix64 x;
        //public readonly Fix64 y;

        public Fix64 x;
        public Fix64 y;
        //[JsonIgnore]
        public Fix64 sqrMagnitude
        {
            get
            {
                return (this.x * this.x) + (this.y * this.y);
            }
        }

        //[JsonIgnore]
        public Fix64 magnitude
        {
            get
            {
                return Fix64.Sqrt(this.sqrMagnitude);
            }
        }

        //[JsonIgnore]
        public FVector2 normalized
        {
            get
            {
                Fix64 magnitude = this.magnitude;
                if(magnitude==0){new FVector2(0, 0);}
                return new FVector2(this.x / magnitude, this.y / magnitude);
            }
        }
        public FVector2 tangent
        {
            get
            {
                
                return new FVector2(-this.y , this.x);
            }
        }

        public FVector2(Fix64 x, Fix64 y)
        {
            this.x = x;
            this.y = y;
        }

        public void Normalize()
        {
            this = new FVector2(this.normalized.x, this.normalized.y);
        }

        public static Fix64 Distance(FVector2 pos1, FVector2 pos2)
        {
            FVector2 difference = pos2 - pos1;

            Fix64 ret = difference.magnitude;

            return ret;
        }

        public static FVector2 Min(FVector2 a, FVector2 b)
        {
            var xMin = Fix64.Min(a.x, b.x);
            var yMin = Fix64.Min(a.y, b.y);
            var ret = new FVector2(xMin, yMin);
            return ret;
        }

        public static FVector2 Max(FVector2 a, FVector2 b)
        {

            var xMax = Fix64.Max(a.x, b.x);
            var yMax = Fix64.Max(a.y, b.y);
            var ret = new FVector2(xMax, yMax);
            return ret;
        }

        internal static FVector2 Transform(FVector2 v, FlatPhysics.FlatTransform transform)
        {
            return new FVector2(
                transform.Cos * v.x - transform.Sin * v.y + transform.PositionX,
                transform.Sin * v.x + transform.Cos * v.y + transform.PositionY);
        }

        public static FVector2 operator *(FVector2 a, FVector2 b)
        {
            return new FVector2(a.x * b.x, a.y * b.y);
        }

        public static FVector2 operator *(FVector2 a, Fix64 b)
        {
            return new FVector2(a.x * b, a.y * b);
        }

        public static FVector2 operator *(Fix64 a, FVector2 b)
        {
            return new FVector2(b.x * a, b.y * a);
        }

        public static FVector2 operator *(int a, FVector2 b)
        {
            return new FVector2(b.x * a, b.y * a);
        }

        public static FVector2 operator /(FVector2 a, int b)
        {
            return new FVector2(a.x / b, a.y / b);
        }

        public static FVector2 operator /(FVector2 a, Fix64 b)
        {
            return new FVector2(a.x / b, a.y / b);
        }

        public static FVector2 operator +(FVector2 a, FVector2 b)
        {
            return new FVector2(a.x + b.x, a.y + b.y);
        }

        public static FVector2 operator -(FVector2 a, FVector2 b)
        {
            return new FVector2(a.x - b.x, a.y - b.y);
        }

        public static FVector2 operator -(FVector2 a)
        {
            return new FVector2(-a.x, -a.y);
        }

        public static bool operator ==(FVector2 a, FVector2 b)
        {
            bool matchX = a.x == b.x;
            bool matchY = a.y == b.y;
            bool ret = matchX && matchY;
            return ret;
        }

        public static bool operator !=(FVector2 a, FVector2 b)
        {
            bool matchX = a.x != b.x;
            bool matchY = a.y != b.y;
            bool ret = matchX || matchY;
            return ret;
        }


        public override bool Equals(object obj)
        {
            return (obj is FVector2) && (((FVector2)obj) == this);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() << 32 | y.GetHashCode();
        }

        public bool Equals(FVector2 other)
        {
            return other == this;
        }
    }
}
