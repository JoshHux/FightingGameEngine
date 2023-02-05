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
        // taken from https://github.com/pgkelley4/line-segments-intersect/blob/master/js/line-segments-intersect.js
        // which was adapted from http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
        // returns the point where they intersect (if they intersect)
        // returns Math.POSITIVE_INFINITY if they don't intersect
        private Fix64 getRayIntersectionFractionOfFirstRay(FVector2 originA, FVector2 endA, FVector2 originB, FVector2 endB)
        {
            var r = endA - originA;
            var s = endB - originB;

            var numerator = FVector2.Dot(originB - originA, r);
            var denominator = FVector2.Dot(r, s);

            if (numerator == 0 && denominator == 0)
            {
                // the lines are co-linear
                // check if they overlap
                // todo: calculate intersection point
                //UnityEngine.Debug.Log("colinear");
                return Fix64.MaxValue;
            }
            if (denominator == 0)
            {
                // lines are parallel
                //UnityEngine.Debug.Log("parallel");
                return Fix64.MaxValue;
            }

            var u = numerator / denominator;
            var t = FVector2.Dot((originB - originA), s) / denominator;
            if ((t >= 0) && (t <= 1) && (u >= 0) && (u <= 1))
            {
                return t;
            }
            //UnityEngine.Debug.Log("misc | u :: " + u + " | t :: " + t);
            return Fix64.MaxValue;
        }

        public Fix64 getRayIntersectionFraction(FVector2 origin, FVector2 direction)
        {
            var end = origin + direction;

            // for each of the AABB's four edges
            // calculate the minimum fraction of "direction"
            // in order to find where the ray FIRST intersects
            // the AABB (if it ever does)
            var minT = getRayIntersectionFractionOfFirstRay(origin, end, new FVector2(this.Min.x, this.Min.y), new FVector2(this.Min.x, this.Max.y));
            Fix64 x;
            x = getRayIntersectionFractionOfFirstRay(origin, end, new FVector2(this.Min.x, this.Max.y), new FVector2(this.Max.x, this.Max.y));
            if (x < minT)
                minT = x;
            x = getRayIntersectionFractionOfFirstRay(origin, end, new FVector2(this.Max.x, this.Max.y), new FVector2(this.Max.x, this.Min.y));
            if (x < minT)
                minT = x;
            x = getRayIntersectionFractionOfFirstRay(origin, end, new FVector2(this.Max.x, this.Min.y), new FVector2(this.Min.x, this.Min.y));
            if (x < minT)
                minT = x;

            //added checks
            //checks the opposite direction because collisions on the left side get messed up without it  
            x = getRayIntersectionFractionOfFirstRay(origin, end, new FVector2(this.Min.x, this.Max.y), new FVector2(this.Min.x, this.Min.y));
            if (x < minT)
                minT = x;
            x = getRayIntersectionFractionOfFirstRay(origin, end, new FVector2(this.Max.x, this.Max.y), new FVector2(this.Min.x, this.Max.y));
            if (x < minT)
                minT = x;
            x = getRayIntersectionFractionOfFirstRay(origin, end, new FVector2(this.Max.x, this.Min.y), new FVector2(this.Max.x, this.Max.y));
            if (x < minT)
                minT = x;
            x = getRayIntersectionFractionOfFirstRay(origin, end, new FVector2(this.Min.x, this.Min.y), new FVector2(this.Max.x, this.Min.y));
            if (x < minT)
                minT = x;


            // ok, now we should have found the fractional component along the ray where we collided
            return minT;
        }
    }
}
