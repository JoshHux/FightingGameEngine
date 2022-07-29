using System.Collections.Generic;
using System.Linq;
using FixMath.NET;
using FlatPhysics.Contact;

namespace FlatPhysics
{
    public sealed class FlatWorld
    {

        public static readonly int MinIterations = 1;
        public static readonly int MaxIterations = 16;

        private int _staticInd;

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
            int ind = this.bodyList.Count;
            bool IsStatic = body.IsStatic;

            //if it does have a parent, insert the item at the end of the list
            if (hasParent)
            {
                //UnityEngine.Debug.Log("adding child bodys");
                this.bodyList.Insert(ind, body);
            }
            else if (IsStatic)
            {
                this._staticInd += 1;
                this.bodyList.Insert(0, body);
            }
            //if it does not have a parent, insert the item at the beginning of the list
            else
            {
                int insertInd = this._staticInd;
                if (ind == 0) { insertInd = -1; }
                this.bodyList.Insert(insertInd + 1, body);
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

            int bodyCount = this.bodyList.Count;
            int bodyCounti = this.bodyList.Count - 1;

            var broadPhaseList = new List<BroadPhasePairs>();

            for (int it = 0; it < iterations; it++)
            {
                // Movement step

                this.bodyList.ForEach(o => o.Step(time, this.gravity, iterations));

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

                // broad phase, find pairs of bodies what MIGHT collide

                for (int i = 0; i < bodyCounti; i++)
                {
                    FlatBody bodyA = this.bodyList[i];

                    bool skipA = (!bodyA.Awake);// || bodyA.IsStatic;
                                                //UnityEngine.Debug.Log(skipA + " A");
                    if (skipA) { continue; }

                    for (int j = i + 1; j < bodyCount; j++)
                    {
                        FlatBody bodyB = this.bodyList[j];

                        //we want to skip if both are static OR either is NOT awake
                        bool skipB = (!bodyB.Awake) || (bodyA.IsStatic && bodyB.IsStatic);
                        //UnityEngine.Debug.Log((!bodyB.Awake) + " || " + (bodyA.IsStatic && bodyB.IsStatic));

                        if (skipB) { continue; }

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

                        var toAdd = new BroadPhasePairs(bodyA, bodyB, bColA, aColB);

                        //AABB check
                        var AABBa = bodyA.GetAABB();
                        var AABBb = bodyB.GetAABB();
                        var aIsPushbox = bodyA.IsPushbox;
                        var bIsPushbox = bodyB.IsPushbox;
                        var bothAreActivePushboxes = (aIsPushbox && bIsPushbox) && (bodyA.ActivePushbox && bodyB.ActivePushbox);
                        var oneIsntPushbox = (!aIsPushbox) || (!bIsPushbox);

                        bool pushboxCheck = canCollide && (oneIsntPushbox || bothAreActivePushboxes);
                        bool checkAABB = pushboxCheck && Collisions.CheckAABB(AABBa, AABBb);
                        bool add = checkAABB && (broadPhaseList.Find(o => toAdd.EqualCheck(o)) == null);

                        //if ((aLayer == Filter.CollisionLayer.LAYER_6 && bLayer == Filter.CollisionLayer.LAYER_3) || (bLayer == Filter.CollisionLayer.LAYER_6 && aLayer == Filter.CollisionLayer.LAYER_3)) { UnityEngine.Debug.Log("colliding i hope"); }
                        //if ((aLayer == Filter.CollisionLayer.LAYER_9 || bLayer == Filter.CollisionLayer.LAYER_9) && (aLayer == Filter.CollisionLayer.LAYER_3 || bLayer == Filter.CollisionLayer.LAYER_3)) { UnityEngine.Debug.Log("environment detection " + canCollide); }
                        //UnityEngine.Debug.Log(checkAABB + " " + add);

                        //the && prevents Collide from running if the bodies can't collide in the first place
                        if (add)
                        {
                            broadPhaseList.Add(toAdd);
                        }
                    }
                }

                //cull duplicates
                //broadPhaseList = broadPhaseList.Distinct().ToList();


                //Narrow Phase
                int nLen = broadPhaseList.Count;

                for (int i = 0; i < nLen; i++)
                {
                    var hold = broadPhaseList[i];

                    //whether or not we can and do collide
                    FVector2 normal = new FVector2();
                    Fix64 depth = 0;

                    var bodyA = hold.BodyA;
                    var bodyB = hold.BodyB;

                    var bCola = hold.BColA;
                    var aColb = hold.AColB;

                    bool collide = this.Collide(bodyA, bodyB, out normal, out depth);

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

                            //bool pushBoxAndStatic = (bodyA.IsPushbox || bodyB.IsPushbox) && (bodyA.IsStatic || bodyB.IsStatic);

                            //pushbox unique collision stuff only needs to happen when both boxes are pushboxes
                            var bothPushBoxes = bodyB.IsPushbox && bodyA.IsPushbox;
                            //both have to be active in order to collide
                            //var bothActivePush = bodyB.ActivePushbox && bodyA.ActivePushbox;
                            //bool collidePushboxes = bothActivePush && bothPushBoxes;
                            //if they're both pushboxes, then we re-calculate the normal to only consider the x-axis
                            if (bothPushBoxes)
                            {
                                var aGO = bodyA.GameObject.GetComponent<FightingGameEngine.Gameplay.LivingObject>();
                                var bGO = bodyB.GameObject.GetComponent<FightingGameEngine.Gameplay.LivingObject>();

                                var aIsAirborne = aGO.IsAirborne();
                                var bIsAirborne = bGO.IsAirborne();
                                var aIsWalled = aGO.IsWalled();
                                var bIsWalled = bGO.IsWalled();
                                var aFacing = aGO.Status.CurrentFacingDirection;
                                var bFacing = bGO.Status.CurrentFacingDirection;

                                var bothAreDiff = (bIsAirborne ^ aIsAirborne);


                                //get the difference in x position 
                                var xOffset = bodyB.Position.x - bodyA.Position.x;

                                if (bothAreDiff > 0)
                                {
                                    if (bIsAirborne > 0)
                                    {
                                        //UnityEngine.Debug.Log("Body B is airborne, setting offset to " + (bFacing)); 
                                        if (aIsWalled > 0)
                                        {
                                            xOffset = -bodyA.Position.x;
                                        }
                                    }
                                    else if (aIsAirborne > 0)
                                    {
                                        //UnityEngine.Debug.Log("Body A is airborne, setting offset to " + (aFacing)); 
                                        if (bIsWalled > 0)
                                        {
                                            xOffset = bodyB.Position.x;
                                        }
                                    }
                                }

                                //if the boxes are directly on top of each other, set it to -1
                                if (xOffset == 0)
                                {
                                    UnityEngine.Debug.Log("setting offset to " + aFacing);
                                    xOffset = aFacing;
                                }
                                normal = new FVector2(xOffset, 0).normalized;

                                //UnityEngine.Debug.Log("both are pushboxes "+bothActivePush);
                                //UnityEngine.Debug.Log("reached" +
                                //"\nA true offset: (" + trueOffsetA.x + ", " + trueOffsetA.y + ")" +
                                //"\nB true offset: (" + trueOffsetB.x + ", " + trueOffsetB.y + ")" +
                                //"\noffset: (" + offset.x + ", " + offset.y + ")");
                                //UnityEngine.Debug.Log("reached" + "\noffset: (" + normal.x + ", " + normal.y + ")");
                            }
                            //if ((bodyA.Layer == Filter.CollisionLayer.LAYER_6 && bodyB.Layer == Filter.CollisionLayer.LAYER_3) || (bodyB.Layer == Filter.CollisionLayer.LAYER_6 && bodyA.Layer == Filter.CollisionLayer.LAYER_3)) { UnityEngine.Debug.Log("colliding i hope"); }


                            var offset = normal * depth;
                            //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                            var trueOffsetB = offset * bCola;
                            //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                            var trueOffsetA = -offset * aColb;
                            //if A is static, only move B
                            if (bodyA.IsStatic)
                            {
                                bodyB.Move(trueOffsetB);
                                //if (pushBoxAndStatic)
                                //{
                                //UnityEngine.Debug.Log("Body B is with static, setting offset to " + (trueOffsetB.x));
                                //    var go = bodyB.GameObject.GetComponent<FightingGameEngine.Gameplay.LivingObject>();
                                //}
                            }
                            //if B is static, only move A
                            else if (bodyB.IsStatic)
                            {
                                bodyA.Move(trueOffsetA);
                                //if (pushBoxAndStatic)
                                //{
                                //UnityEngine.Debug.Log("Body A is with static, setting offset to " + (trueOffsetA.x));
                                //     var go = bodyA.GameObject.GetComponent<FightingGameEngine.Gameplay.LivingObject>();
                                //}
                            }
                            //neither is static, move both
                            else
                            {
                                //cleaned up math op's a bit
                                //var offsetA = trueOffsetA / 2;
                                //var offsetB = trueOffsetB / 2;
                                var offsetA = trueOffsetA * FixedMath.C0p5;
                                var offsetB = trueOffsetB * FixedMath.C0p5;

                                var trOffA = new FVector2(offsetA.x, offsetA.y);
                                var trOffB = new FVector2(offsetB.x, offsetB.y);

                                //if both are pushboxes, apply to velocity
                                if (bothPushBoxes)// && depth > (FixedMath.C0p001 / 5))
                                {
                                    //UnityEngine.Debug.Log("both are pushboxes " + depth);



                                    bodyA.Impulse += (trOffA) * (2 / time);
                                    bodyB.Impulse += (trOffB) * (2 / time);

                                    if (trOffA.x * bodyB.LinearVelocity.x > 0) { bodyA.Impulse += new FVector2(bodyB.LinearVelocity.x, 0); }
                                    if (trOffB.x * bodyA.LinearVelocity.x > 0) { bodyB.Impulse += new FVector2(bodyA.LinearVelocity.x, 0); }

                                    //bodyA.Impulse += (trOffA) * (1 / time) + new FVector2(bodyB.LinearVelocity.x - bodyA.LinearVelocity.x, 0)* (1 / time);
                                    //bodyB.Impulse += (trOffB) * (1 / time) + new FVector2(bodyA.LinearVelocity.x - bodyB.LinearVelocity.x, 0)* (1 / time);


                                    //bodyA.Move(trOffA);
                                    //bodyB.Move(trOffB);
                                }
                                else
                                {
                                    bodyA.Move(trOffA);
                                    bodyB.Move(trOffB);
                                }
                            }
                            this.ResolveCollision(bodyA, bodyB, normal, depth, aColb, bCola, bothPushBoxes);
                        }

                        //if we are able to collide, call the respective callback
                        if (aColb > 0)
                        {
                            var c = new ContactData(FVector2.zero, bodyB.GameObject);
                            bodyA.OnColOverlap(c);
                        }
                        if (bCola > 0)
                        {
                            var c = new ContactData(FVector2.zero, bodyA.GameObject);
                            bodyB.OnColOverlap(c);
                        }

                    }
                }
            }
        }

        public void ResolveAgainstAllStatic(FlatBody body1, FlatBody body2)
        {
            var allStatic = this.bodyList.FindAll(o => o.IsStatic);
            var broadPhaseList = new List<BroadPhasePairs>();


            int bodyCount = allStatic.Count;

            for (int i = 0; i < bodyCount; i++)
            {

                FlatBody staticObj = allStatic[i];

                //check is body1 collides with the static objects

                var toAdd = new BroadPhasePairs(staticObj, body1, 1, 1);

                //AABB check
                var AABBa = body1.GetAABB();
                var AABBb = staticObj.GetAABB();

                bool checkAABB = Collisions.CheckAABB(AABBa, AABBb);
                bool add = checkAABB && (broadPhaseList.Find(o => toAdd.EqualCheck(o)) == null);

                //if ((aLayer == Filter.CollisionLayer.LAYER_6 && bLayer == Filter.CollisionLayer.LAYER_3) || (bLayer == Filter.CollisionLayer.LAYER_6 && aLayer == Filter.CollisionLayer.LAYER_3)) { UnityEngine.Debug.Log("colliding i hope"); }
                //if ((aLayer == Filter.CollisionLayer.LAYER_9 || bLayer == Filter.CollisionLayer.LAYER_9) && (aLayer == Filter.CollisionLayer.LAYER_3 || bLayer == Filter.CollisionLayer.LAYER_3)) { UnityEngine.Debug.Log("environment detection " + canCollide); }
                //UnityEngine.Debug.Log(checkAABB + " " + add);
                //UnityEngine.Debug.Log(body1.GameObject.name + " " + checkAABB + " " + add + " " + " " + staticObj.Position.x + " " + body1.Position.x);

                //the && prevents Collide from running if the bodies can't collide in the first place
                if (add)
                {
                    broadPhaseList.Add(toAdd);
                }

                //check is body1 collides with the static objects
                toAdd = new BroadPhasePairs(staticObj, body2, 1, 1);

                //AABB check
                AABBa = staticObj.GetAABB();
                AABBb = body2.GetAABB();

                checkAABB = Collisions.CheckAABB(AABBa, AABBb);
                add = checkAABB && (broadPhaseList.Find(o => toAdd.EqualCheck(o)) == null);

                //if ((aLayer == Filter.CollisionLayer.LAYER_6 && bLayer == Filter.CollisionLayer.LAYER_3) || (bLayer == Filter.CollisionLayer.LAYER_6 && aLayer == Filter.CollisionLayer.LAYER_3)) { UnityEngine.Debug.Log("colliding i hope"); }
                //if ((aLayer == Filter.CollisionLayer.LAYER_9 || bLayer == Filter.CollisionLayer.LAYER_9) && (aLayer == Filter.CollisionLayer.LAYER_3 || bLayer == Filter.CollisionLayer.LAYER_3)) { UnityEngine.Debug.Log("environment detection " + canCollide); }
                //UnityEngine.Debug.Log(body2.GameObject.name + " " + checkAABB + " " + add + " " + " " + staticObj.Position.x + " " + body2.Position.x);

                //the && prevents Collide from running if the bodies can't collide in the first place
                if (add)
                {
                    broadPhaseList.Add(toAdd);
                }

            }

            //cull duplicates
            //broadPhaseList = broadPhaseList.Distinct().ToList();


            //Narrow Phase
            int nLen = broadPhaseList.Count;

            //total offset of two bodies
            var bodyOffset = new FVector2(0, 0);

            for (int i = 0; i < nLen; i++)
            {
                var hold = broadPhaseList[i];

                //whether or not we can and do collide
                FVector2 normal = new FVector2();
                Fix64 depth = 0;

                var bodyA = hold.BodyA;
                var bodyB = hold.BodyB;

                var bCola = hold.BColA;
                var aColb = hold.AColB;

                bool collide = this.Collide(bodyA, bodyB, out normal, out depth);
                //UnityEngine.Debug.Log(bodyB.GameObject.name + " found a collision " + collide + " " + bodyA.Position.x + " " + bodyB.Position.x);

                //if we do collide
                if (collide)
                {


                    var offset = normal * depth;
                    //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                    var trueOffsetB = offset * bCola;
                    //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                    var trueOffsetA = -offset * aColb;
                    //if A is static, only move B
                    //if (bodyA.IsStatic)
                    //{
                    bodyB.Move(trueOffsetB);
                    //if (pushBoxAndStatic)
                    //{
                    //UnityEngine.Debug.Log("Body B is with static, setting offset to " + (trueOffsetB.x));
                    //    var go = bodyB.GameObject.GetComponent<FightingGameEngine.Gameplay.LivingObject>();
                    //}
                    //bodyA.Move(trueOffsetB);

                    //bodyB is always the non-static object in this case
                    if (bodyB == body1)
                    {
                        body2.Move(trueOffsetB);
                    }
                    else
                    {
                        body1.Move(trueOffsetB);

                    }

                    //}


                    this.ResolveCollision(bodyA, bodyB, normal, depth, aColb, bCola, false);
                }

            }
        }

        public void ResolveCollision(FlatBody bodyA, FlatBody bodyB, FVector2 normal, Fix64 depth, int aColB, int bColA, bool bothActivePushboxes)
        {
            FVector2 relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            //if (bodyA.GameObject.name == "TestPlayer" && bodyA.LinearVelocity.magnitude > 0) UnityEngine.Debug.Log("- bodyA, velocity :: (" + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y + ")");
            //if (bodyB.GameObject.name == "TestPlayer" && bodyB.LinearVelocity.magnitude > 0) UnityEngine.Debug.Log("- bodyB, velocity :: (" + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y + ")");

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

            if (bothActivePushboxes)
            {
                //var mag = relativeVelocity.magnitude;
                relativeVelocity.y = 0;

                //relativeVelocity = relativeVelocity.normalized * mag;

                bodyA.Impulse -= -relativeVelocity * bodyA.InvMass * aColB;
                bodyB.Impulse += -relativeVelocity * bodyB.InvMass * bColA;

                impulse.y = 0;
                //bodyA.Impulse -= impulse * bodyA.InvMass * aColB;
                //bodyB.Impulse += impulse * bodyB.InvMass * bColA;

                //if (bodyA.GameObject.name == "TestPlayer" && bodyA.LinearVelocity.magnitude > 0) UnityEngine.Debug.Log("- bodyA, velocity :: (" + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y + ")");
                //if (bodyB.GameObject.name == "TestPlayer" && bodyB.LinearVelocity.magnitude > 0) UnityEngine.Debug.Log("- bodyB, velocity :: (" + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y + ")");
            }
            else
            {
                bodyA.LinearVelocity -= impulse * bodyA.InvMass * aColB;
                bodyB.LinearVelocity += impulse * bodyB.InvMass * bColA;

                //bodyA.Impulse -= impulse * bodyA.InvMass * aColB;
                //bodyB.Impulse += impulse * bodyB.InvMass * bColA;

            }
        }



        public bool Collide(FlatBody bodyA, FlatBody bodyB, out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA == ShapeType.Box)
            {
                if (shapeTypeB == ShapeType.Box)
                {
                    return Collisions.IntersectPolygons(
                        bodyA.Position, bodyA.GetTransformedVertices(),
                        bodyB.Position, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB == ShapeType.Circle)
                {
                    bool result = Collisions.IntersectCirclePolygon(
                        bodyB.Position, bodyB.Radius,
                        bodyA.Position, bodyA.GetTransformedVertices(),
                        out normal, out depth);

                    normal = -normal;
                    return result;
                }
            }
            else if (shapeTypeA == ShapeType.Circle)
            {
                if (shapeTypeB == ShapeType.Box)
                {
                    return Collisions.IntersectCirclePolygon(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB == ShapeType.Circle)
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
    public sealed class BroadPhasePairs
    {
        private FlatBody _a;
        private FlatBody _b;

        private int _aColb;
        private int _bCola;

        public FlatBody BodyA { get { return this._a; } }
        public FlatBody BodyB { get { return this._b; } }
        public int AColB { get { return this._aColb; } }
        public int BColA { get { return this._bCola; } }


        public BroadPhasePairs(FlatBody A, FlatBody B, int ab, int ba)
        {
            this._a = A;
            this._b = B;
            this._aColb = ab;
            this._bCola = ba;
        }

        public bool EqualCheck(BroadPhasePairs other)
        {
            bool ret1 = this._a == other.BodyA;
            bool ret2 = ret1 && (this._b == other.BodyB);
            bool ret3 = this._b == other.BodyA;
            bool ret4 = ret3 && (this._a == other.BodyB);

            bool ret = ret2 || ret4;

            return ret;
        }
    }
}
