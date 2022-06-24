using System.Collections.Generic;
using FixMath.NET;
using FlatPhysics.Contact;

namespace FlatPhysics
{
    public sealed class FlatWorld
    {

        public static readonly int MinIterations = 1;
        public static readonly int MaxIterations = 128;

        private FVector2 gravity;
        private List<FlatBody> bodyList;

        public int BodyCount
        {
            get { return this.bodyList.Count; }
        }

        public FlatWorld()
        {
            this.gravity = new FVector2(0, 0);
            this.bodyList = new List<FlatBody>();
        }

        public void AddBody(FlatBody body)
        {
            //whether the body we want to add has a parent in or not
            //this makes sure that children are only update after their parent
            //only works if there's only one level of parenting
            bool hasParent = body.HasParent();

            //if it does have a parent, insert the item at the end of the list
            if (hasParent)
            {
                //UnityEngine.Debug.Log("adding child bodys");
                int ind = this.bodyList.Count;
                this.bodyList.Insert(ind, body);
            }
            //if it does not have a parent, insert the item at the beginning of the list
            else
            {
                this.bodyList.Insert(0, body);
            }
        }

        public bool RemoveBody(FlatBody body)
        {
            return this.bodyList.Remove(body);
        }

        public bool GetBody(int index, out FlatBody body)
        {
            body = null;

            if (index < 0 || index >= this.bodyList.Count)
            {
                return false;
            }

            body = this.bodyList[index];
            return true;
        }

        public void Step(Fix64 time, int iterations)
        {
            iterations = UnityEngine.Mathf.Clamp(iterations, FlatWorld.MinIterations, FlatWorld.MaxIterations);

            for (int it = 0; it < iterations; it++)
            {
                // Movement step
                for (int i = 0; i < this.bodyList.Count; i++)
                {
                    this.bodyList[i].Step(time, this.gravity, iterations);
                }
                //TODO: Remove this loop, maybe update childeren from the parent in the Step, childeren should not have linear velocity afterall
                // Parent update step
                //we update the positions again to resolve the local positions
                /*for (int i = 0; i < this.bodyList.Count; i++)
                {
                    var hold = this.bodyList[i];
                    var check = hold.HasParent();
                    if (check)
                        hold.StepParent();
                }*/

                // collision step
                for (int i = 0; i < this.bodyList.Count - 1; i++)
                {
                    FlatBody bodyA = this.bodyList[i];

                    for (int j = i + 1; j < this.bodyList.Count; j++)
                    {
                        FlatBody bodyB = this.bodyList[j];

                        bool bothAwake = bodyA.Awake && bodyB.Awake;

                        bool bothStatic = bodyA.IsStatic && bodyB.IsStatic;
                        //we want to skip if both are static OR either is NOT awake
                        bool skip = bothStatic && (!bothAwake);

                        if (skip)
                        {
                            continue;
                        }

                        //the following 2 variables can only be 0 or 1
                        //this is be cause Layer is guarenteed to be nonzero, so norisk of dividing by zero
                        //if (CollidesWith & Layer) is nonzero, then the value is equal to Layer, since Layer is guarenteed to be only a power of 2
                        // so (CollidesWith & Layer)/Layer is 1, otherwise, it is 0

                        //get the collision layer data
                        var aLayer = bodyA.Layer;
                        var bLayer = bodyB.Layer;
                        var aCollidesWith = bodyA.CollidesWith;
                        var bCollidesWith = bodyB.CollidesWith;


                        //bodyA can collide with bodyB
                        var abLayer = (int)(aCollidesWith & bLayer);
                        int aColB = abLayer / (int)bLayer;
                        //bodyB can collide with bodyA
                        var baLayer = (int)(bCollidesWith & aLayer);
                        int bColA = baLayer / (int)aLayer;

                        //bool version to check nonzero condition of above
                        bool abCol = aColB > 0;
                        bool baCol = bColA > 0;

                        //either body can collide with the other
                        bool canCollide = abCol || baCol;

                        //whether or not we can and do collide
                        FVector2 normal = new FVector2();
                        Fix64 depth = 0;

                        //AABB check
                        var AABBa = bodyA.GetAABB();
                        var AABBb = bodyB.GetAABB();
                        bool checkAABB = canCollide && Collisions.CheckAABB(AABBa, AABBb);

                        //the && prevents Collide from running if the bodies can't collide in the first place
                        bool collide = checkAABB && this.Collide(bodyA, bodyB, out normal, out depth);

                        //if we do collide
                        if (collide)
                        {
                            //hopefully, moving the boolean operations outside of the if statement directly helps a bit with performance
                            var eitherIsTrigger = bodyA.IsTrigger || bodyB.IsTrigger;
                            var neitherIsTrigger = !eitherIsTrigger;
                            //neither body is a trigger, resolve collision
                            if (neitherIsTrigger)
                            {

                                //TODO: Make the pushboxes check if each are cornered or not, likely use a boolean or int (-1,1) to determine what direction to push
                                //current solution is a little duct-tape-y

                                //pushbox unique collision stuff only needs to happen when both boxes are pushboxes
                                var bothPushBoxes = bodyB.IsPushbox && bodyA.IsPushbox;
                                //if they're both pushboxes, then we re-calculate the normal to only consider the x-axis
                                if (bothPushBoxes)
                                {
                                    //get the difference in x position 
                                    var xOffset = bodyB.Position.x - bodyA.Position.x;
                                    //if the boxes are directly on top of each other, set it to -1
                                    if (xOffset == 0)
                                    {
                                        xOffset = -1;
                                    }
                                    normal = new FVector2(xOffset, 0).normalized;

                                    //UnityEngine.Debug.Log("reached" +
                                    //"\nA true offset: (" + trueOffsetA.x + ", " + trueOffsetA.y + ")" +
                                    //"\nB true offset: (" + trueOffsetB.x + ", " + trueOffsetB.y + ")" +
                                    //"\noffset: (" + offset.x + ", " + offset.y + ")");
                                    //UnityEngine.Debug.Log("reached" + "\noffset: (" + normal.x + ", " + normal.y + ")");
                                }

                                var offset = normal * depth;
                                //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                                var trueOffsetB = offset * bColA;
                                //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                                var trueOffsetA = -offset * aColB;
                                //if A is static, only move B
                                if (bodyA.IsStatic)
                                {
                                    bodyB.Move(trueOffsetB);
                                }
                                //if B is static, only move A
                                else if (bodyB.IsStatic)
                                {
                                    bodyA.Move(trueOffsetA);
                                }
                                //neither is static, move both
                                else
                                {
                                    //cleaned up math op's a bit
                                    //var offsetA = trueOffsetA / 2;
                                    //var offsetB = trueOffsetB / 2;
                                    var offsetA = trueOffsetA * FixedMath.C0p5;
                                    var offsetB = trueOffsetB * FixedMath.C0p5;

                                    bodyA.Move(offsetA);
                                    bodyB.Move(offsetB);
                                }

                                this.ResolveCollision(bodyA, bodyB, normal, depth, aColB, bColA);
                            }

                            //if we are anle to collide, call the respective callback
                            if (abCol)
                            {
                                var c = new ContactData(FVector2.zero, bodyB.GameObject);
                                bodyA.OnColOverlap(c);
                            }
                            if (baCol)
                            {
                                var c = new ContactData(FVector2.zero, bodyA.GameObject);
                                bodyB.OnColOverlap(c);
                            }

                        }
                    }
                }
            }
        }

        public void ResolveCollision(FlatBody bodyA, FlatBody bodyB, FVector2 normal, Fix64 depth, int aColB, int bColA)
        {
            FVector2 relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (FVector2.Dot(relativeVelocity, normal) > 0)
            {
                return;
            }

            Fix64 e = Fix64.Min(bodyA.Restitution, bodyB.Restitution);

            Fix64 j = -(1 + e) * FVector2.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass + bodyB.InvMass;

            FVector2 impulse = j * normal;

            //            bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            //            bodyB.LinearVelocity += impulse * bodyB.InvMass;

            bodyA.LinearVelocity -= impulse * bodyA.InvMass * aColB;
            bodyB.LinearVelocity += impulse * bodyB.InvMass * bColA;
        }

        public bool Collide(FlatBody bodyA, FlatBody bodyB, out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectPolygons(
                        bodyA.Position, bodyA.GetTransformedVertices(),
                        bodyB.Position, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    bool result = Collisions.IntersectCirclePolygon(
                        bodyB.Position, bodyB.Radius,
                        bodyA.Position, bodyA.GetTransformedVertices(),
                        out normal, out depth);

                    normal = -normal;
                    return result;
                }
            }
            else if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectCirclePolygon(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    return Collisions.IntersectCircles(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.Radius,
                        out normal, out depth);
                }
            }

            return false;
        }
    }
}
