using System;
using FixMath.NET;

namespace FlatPhysics
{
    public static class Collisions
    {
        public static bool CheckAABB(FlatAABB a, FlatAABB b)
        {
            var d1 = a.Min - b.Max;
            var d2 = b.Min - a.Max;

            return d1.x <= 0 && d1.y <= 0 && d2.x <= 0 && d2.y <= 0;
        }

        public static bool IntersectCirclePolygon(FVector2 circleCenter, Fix64 circleRadius,
                                                    FVector2 polygonCenter, FVector2[] vertices,
                                                    out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = Fix64.MaxValue;

            FVector2 axis = FVector2.zero;
            Fix64 axisDepth = 0;
            Fix64 minA, maxA, minB, maxB;

            for (int i = 0; i < vertices.Length; i++)
            {
                FVector2 va = vertices[i];
                FVector2 vb = vertices[(i + 1) % vertices.Length];

                FVector2 edge = vb - va;
                axis = new FVector2(-edge.y, edge.x);
                //axis = axis.Normalize();
                axis.Normalize();

                Collisions.ProjectVertices(vertices, axis, out minA, out maxA);
                Collisions.ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                axisDepth = Fix64.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            int cpIndex = Collisions.FindClosestPointOnPolygon(circleCenter, vertices);
            FVector2 cp = vertices[cpIndex];

            axis = cp - circleCenter;
            //axis = FlatMath.Normalize(axis);
            axis.Normalize();

            Collisions.ProjectVertices(vertices, axis, out minA, out maxA);
            Collisions.ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

            if (minA >= maxB || minB >= maxA)
            {
                return false;
            }

            axisDepth = Fix64.Min(maxB - minA, maxA - minB);

            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }

            FVector2 direction = polygonCenter - circleCenter;

            if (FVector2.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }

            return true;
        }


        public static bool IntersectCirclePolygon(FVector2 circleCenter, Fix64 circleRadius,
            FVector2[] vertices,
            out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = Fix64.MaxValue;

            FVector2 axis = FVector2.zero;
            Fix64 axisDepth = 0;
            Fix64 minA, maxA, minB, maxB;

            for (int i = 0; i < vertices.Length; i++)
            {
                FVector2 va = vertices[i];
                FVector2 vb = vertices[(i + 1) % vertices.Length];

                FVector2 edge = vb - va;
                axis = new FVector2(-edge.y, edge.x);
                //axis = FlatMath.Normalize(axis);
                axis.Normalize();

                Collisions.ProjectVertices(vertices, axis, out minA, out maxA);
                Collisions.ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                axisDepth = Fix64.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            int cpIndex = Collisions.FindClosestPointOnPolygon(circleCenter, vertices);
            FVector2 cp = vertices[cpIndex];

            axis = cp - circleCenter;
            //axis = FlatMath.Normalize(axis);
            axis.Normalize();

            Collisions.ProjectVertices(vertices, axis, out minA, out maxA);
            Collisions.ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

            if (minA >= maxB || minB >= maxA)
            {
                return false;
            }

            axisDepth = Fix64.Min(maxB - minA, maxA - minB);

            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }

            FVector2 polygonCenter = Collisions.FindArithmeticMean(vertices);

            FVector2 direction = polygonCenter - circleCenter;

            if (FVector2.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }

            return true;
        }

        private static int FindClosestPointOnPolygon(FVector2 circleCenter, FVector2[] vertices)
        {
            int result = -1;
            Fix64 minDistance = Fix64.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                FVector2 v = vertices[i];
                Fix64 distance = FVector2.Distance(v, circleCenter);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    result = i;
                }
            }

            return result;
        }

        private static void ProjectCircle(FVector2 center, Fix64 radius, FVector2 axis, out Fix64 min, out Fix64 max)
        {
            //FVector2 direction = FlatMath.Normalize(axis);
            FVector2 direction = axis.normalized;
            FVector2 directionAndRadius = direction * radius;

            FVector2 p1 = center + directionAndRadius;
            FVector2 p2 = center - directionAndRadius;

            min = FVector2.Dot(p1, axis);
            max = FVector2.Dot(p2, axis);

            if (min > max)
            {
                // swap the min and max values.
                Fix64 t = min;
                min = max;
                max = t;
            }
        }

        public static bool IntersectPolygons(FVector2 centerA, FVector2[] verticesA, FVector2 centerB, FVector2[] verticesB, out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = Fix64.MaxValue;

            for (int i = 0; i < verticesA.Length; i++)
            {
                FVector2 va = verticesA[i];
                FVector2 vb = verticesA[(i + 1) % verticesA.Length];

                FVector2 edge = vb - va;
                FVector2 axis = new FVector2(-edge.y, edge.x);
                //axis = FlatMath.Normalize(axis);
                axis.Normalize();

                Collisions.ProjectVertices(verticesA, axis, out Fix64 minA, out Fix64 maxA);
                Collisions.ProjectVertices(verticesB, axis, out Fix64 minB, out Fix64 maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                Fix64 axisDepth = Fix64.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                FVector2 va = verticesB[i];
                FVector2 vb = verticesB[(i + 1) % verticesB.Length];

                FVector2 edge = vb - va;
                FVector2 axis = new FVector2(-edge.y, edge.x);
                //axis = FlatMath.Normalize(axis);
                axis.Normalize();

                Collisions.ProjectVertices(verticesA, axis, out Fix64 minA, out Fix64 maxA);
                Collisions.ProjectVertices(verticesB, axis, out Fix64 minB, out Fix64 maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                Fix64 axisDepth = Fix64.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            FVector2 direction = centerB - centerA;

            if (FVector2.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }

            return true;
        }

        public static bool IntersectPolygons(FVector2[] verticesA, FVector2[] verticesB, out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = Fix64.MaxValue;

            for (int i = 0; i < verticesA.Length; i++)
            {
                FVector2 va = verticesA[i];
                FVector2 vb = verticesA[(i + 1) % verticesA.Length];

                FVector2 edge = vb - va;
                FVector2 axis = new FVector2(-edge.y, edge.x);
                //                //axis = FlatMath.Normalize(axis);
                axis.Normalize();

                Collisions.ProjectVertices(verticesA, axis, out Fix64 minA, out Fix64 maxA);
                Collisions.ProjectVertices(verticesB, axis, out Fix64 minB, out Fix64 maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                Fix64 axisDepth = Fix64.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                FVector2 va = verticesB[i];
                FVector2 vb = verticesB[(i + 1) % verticesB.Length];

                FVector2 edge = vb - va;
                FVector2 axis = new FVector2(-edge.y, edge.x);
                //axis = FlatMath.Normalize(axis);
                axis.Normalize();

                Collisions.ProjectVertices(verticesA, axis, out Fix64 minA, out Fix64 maxA);
                Collisions.ProjectVertices(verticesB, axis, out Fix64 minB, out Fix64 maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                Fix64 axisDepth = Fix64.Min(maxB - minA, maxA - minB);

                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            FVector2 centerA = Collisions.FindArithmeticMean(verticesA);
            FVector2 centerB = Collisions.FindArithmeticMean(verticesB);

            FVector2 direction = centerB - centerA;

            if (FVector2.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }

            return true;
        }

        private static FVector2 FindArithmeticMean(FVector2[] vertices)
        {
            Fix64 sumX = 0;
            Fix64 sumY = 0;

            for (int i = 0; i < vertices.Length; i++)
            {
                FVector2 v = vertices[i];
                sumX += v.x;
                sumY += v.y;
            }

            return new FVector2(sumX / (Fix64)vertices.Length, sumY / (Fix64)vertices.Length);
        }

        private static void ProjectVertices(FVector2[] vertices, FVector2 axis, out Fix64 min, out Fix64 max)
        {
            min = Fix64.MaxValue;
            max = Fix64.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                FVector2 v = vertices[i];
                Fix64 proj = FVector2.Dot(v, axis);

                if (proj < min) { min = proj; }
                if (proj > max) { max = proj; }
            }
        }

        public static bool IntersectCircles(
            FVector2 centerA, Fix64 radiusA,
            FVector2 centerB, Fix64 radiusB,
            out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = 0;

            Fix64 distance = FVector2.Distance(centerA, centerB);
            Fix64 radii = radiusA + radiusB;

            if (distance >= radii)
            {
                return false;
            }

            //normal = FVector2.Normalize(centerB - centerA);
            normal = centerB - centerA;
            normal.Normalize();
            depth = radii - distance;

            return true;
        }

    }
}
