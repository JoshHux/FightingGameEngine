using System.Collections.Generic;
using System.Linq;
using FixMath.NET;
using FlatPhysics.Contact;
using System;
using UnityEngine;

namespace FlatPhysics
{
    public sealed class FlatWorld
    {

        public static readonly int MinIterations = 1;
        public static readonly int MaxIterations = 8;

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
            this._staticInd = 0;
        }

        public void AddBody(FlatBody body)
        {
            //whether the body we want to add has a parent in or not
            //this makes sure that children are only update after their parent
            //only works if there's only one level of parenting
            bool hasParent = body.HasParent();
            int ind = this.bodyList.Count;
            bool IsStatic = body.IsStatic;

            if (bodyList.Find(o => o.BodyID == body.BodyID) != null) { return; }

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


            int i = this.bodyList.Count;
            do
            {
                //assign the id based on the totalnumber of bodies so far
                body.BodyID = i;
                i++;
            }
            while (this.bodyList.Find(o => o.BodyID == i) != null);

        }


        public FlatBody FindBody(FlatPhysics.Unity.FRigidbody go)
        {
            //if(this.bodyList.Find(o => o.GameObject == null)!=null){Debug.Log("null go objects found");}
            return this.bodyList.Find(o => o.BodyID == go.BodyID);
        }

        public List<int> GetBodyIds()
        {
            var ret = new List<int>();
            foreach (var b in this.bodyList)
            {
                ret.Add(b.BodyID);
            }

            ret.Sort();
            return ret;
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

        public void Step(Fix64 time)
        {
            //iterations = UnityEngine.Mathf.Clamp(iterations, FlatWorld.MinIterations, FlatWorld.MaxIterations);

            int bodyCount = this.bodyList.Count;
            int bodyCounti = this.bodyList.Count - 1;

            var broadPhaseList = new List<BroadPhasePairs>();


            //for (int it = 0; it < iterations; it++)
            //{
            #region Physic calculations

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
            //if two pushboxes collide, we want to query their collision again after static objects have collided just in case
            var dynamicPairs = new List<BroadPhasePairs>();
            int staticCol = 0;

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

                    //only bodies with different or null parents will collide with each other
                    bool parentCheck = pushboxCheck && (bodyA.Parent == null || bodyB.Parent == null || (bodyA.Parent != bodyB.Parent));
                    bool checkAABB = parentCheck && Collisions.CheckAABB(AABBa, AABBb);
                    bool add = checkAABB;// && (broadPhaseList.Find(o => toAdd.EqualCheck(o)) == null);

                    //if ((aLayer == Filter.CollisionLayer.LAYER_6 && bLayer == Filter.CollisionLayer.LAYER_3) || (bLayer == Filter.CollisionLayer.LAYER_6 && aLayer == Filter.CollisionLayer.LAYER_3)) { UnityEngine.Debug.Log("colliding i hope"); }
                    //if ((aLayer == Filter.CollisionLayer.LAYER_9 || bLayer == Filter.CollisionLayer.LAYER_9) && (aLayer == Filter.CollisionLayer.LAYER_3 || bLayer == Filter.CollisionLayer.LAYER_3)) { UnityEngine.Debug.Log("environment detection " + canCollide); }
                    //UnityEngine.Debug.Log(checkAABB + " " + add);

                    //the && prevents Collide from running if the bodies can't collide in the first place
                    if (add)
                    {
                        if (bodyA.IsPushbox && bodyB.IsPushbox)
                        {
                            broadPhaseList.Insert(0, toAdd);
                            dynamicPairs.Add(toAdd);
                        }
                        else
                        {
                            broadPhaseList.Add(toAdd);
                        }

                        if (bodyA.IsStatic || bodyB.IsStatic)
                        {
                            staticCol += 1;
                        }
                    }
                }
            }

            if (dynamicPairs.Count > 0 && broadPhaseList.Count - staticCol > 1) { broadPhaseList.InsertRange(staticCol + 1, dynamicPairs); }
            else
            if (dynamicPairs.Count > 0 && broadPhaseList.Count - staticCol > 0) { broadPhaseList.AddRange(dynamicPairs); }
            //Narrow Phase
            int nLen = broadPhaseList.Count;
            //var colList = "";

            for (int i = 0; i < broadPhaseList.Count; i++)
            {
                var hold = broadPhaseList[i];

                //whether or not we can and do collide
                FVector2 normal = new FVector2();
                Fix64 depth = 0;

                var bodyA = hold.BodyA;
                var bodyB = hold.BodyB;
                //debug
                //colList += bodyA.GameObject.name + ", " + bodyB.GameObject.name + "\n";

                var bCola = hold.BColA;
                var aColb = hold.AColB;
                bool collide = this.Collide(bodyA, bodyB, out normal, out depth);


                //pushbox unique collision stuff only needs to happen when both boxes are pushboxes
                var bothPushBoxes = bodyB.IsPushbox && bodyA.IsPushbox;

                //if (i == staticCol + 1 && bothPushBoxes)
                //{ Debug.Log("checking additional col " + i + ", " + staticCol + " | " + bodyA.GameObject.name + " - " + bodyB.GameObject.name); }
                //if we do collide
                if (collide)
                {
                    if (bodyA.IsTrigger && bodyB.IsTrigger)
                    {
                        depth = 0;
                        normal = FVector2.zero;
                        //Debug.Log("discrete | " + bodyA.GameObject.name + " - " + bodyB.GameObject.name);
                    }
                    var offset = normal * depth;
                    //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                    var trueOffsetB = offset * bCola;
                    //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                    var trueOffsetA = -offset * aColb;

                    //Debug.Log("avel - " + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y);
                    //Debug.Log("bvel - " + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y);
                    //Debug.Log("apos - " + bodyA.Position.x + ", " + bodyA.Position.y);
                    //Debug.Log("bpos - " + bodyB.Position.x + ", " + bodyB.Position.y);
                    //Debug.Log("normal :: " + normal.x + ", " + normal.y);

                    //if (bothPushBoxes)
                    //{//}

                    //{


                    int aInCorner = 1;
                    int bInCorner = 1;
                    if (dynamicPairs.Count > 0 && i == staticCol + 1 && bothPushBoxes)
                    {
                        //continue;
                        aInCorner = Fix64.Sign(Fix64.Abs(bodyA.LinearVelocity.x));
                        bInCorner = Fix64.Sign(Fix64.Abs(bodyB.LinearVelocity.x));
                        //Debug.Log("checking additional col " + i + ", " + staticCol + " | " + bodyA.GameObject.name + " - " + bodyB.GameObject.name);

                        if (aInCorner != 0 && bInCorner != 0) { continue; }
                    }

                    bodyB.Move(trueOffsetB * bInCorner);
                    bodyA.Move(trueOffsetA * aInCorner);


                    this.ResolveCollision(bodyA, bodyB, normal, depth, aColb, bCola, false, aInCorner, bInCorner);


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
                }//we didn't collide
                else
                {
                    //swept collision
                    var colTime = this.SweptCollide(bodyA, bodyB, out normal, out depth);
                    if (colTime < Fix64.MaxValue)
                    {
                        //Debug.Log("max: " + bodyB.GetAABB().Max.x + ", " + bodyB.GetAABB().Max.y);
                        //Debug.Log("min: " + bodyB.GetAABB().Min.x + ", " + bodyB.GetAABB().Min.y);

                        //if (colTime > 0)
                        //if (bothPushBoxes)
                        //{

                        if (bodyA.IsTrigger && bodyB.IsTrigger)
                        {
                            continue;
                            //Debug.Log("swept: " + colTime + " | " + bodyA.GameObject.name + " - " + bodyB.GameObject.name);
                        }
                        //Debug.Log("avel - " + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y);
                        //Debug.Log("bvel - " + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y);
                        //Debug.Log("apos - " + bodyA.Position.x + ", " + bodyA.Position.y);
                        //Debug.Log("bpos - " + bodyB.Position.x + ", " + bodyB.Position.y);
                        //Debug.Log("normal :: " + normal.x + ", " + normal.y);
                        //}
                        var offset = normal * depth;
                        //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                        var trueOffsetB = offset * bCola;
                        //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                        var trueOffsetA = -offset * aColb;
                        //{

                        int aInCorner = 1;
                        int bInCorner = 1;
                        if (dynamicPairs.Count > 0 && i == staticCol + 1 && bothPushBoxes)
                        {
                            aInCorner = Fix64.Sign(Fix64.Abs(bodyA.LinearVelocity.x));
                            bInCorner = Fix64.Sign(Fix64.Abs(bodyB.LinearVelocity.x));
                            //Debug.Log("checking additional col " + i + ", " + staticCol + " | " + bodyA.GameObject.name + " - " + bodyB.GameObject.name);

                            if (aInCorner != 0 && bInCorner != 0) { continue; }
                        }
                        bodyB.Move(bodyB.LinearVelocity * time * colTime);
                        bodyA.Move(bodyA.LinearVelocity * time * colTime);
                        this.ResolveCollision(bodyA, bodyB, normal, depth, aColb, bCola, false, aInCorner, bInCorner);


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

            /*
            //insearts dynamic pairs aafter statics
            int dlen = dynamicPairs.Count();
            for (int i = 0; i < dlen; i++)
            {
                break;
                var hold = dynamicPairs[i];

                //whether or not we can and do collide
                FVector2 normal = new FVector2();
                Fix64 depth = 0;

                var bodyA = hold.BodyA;
                var bodyB = hold.BodyB;
                //debug
                //colList += bodyA.GameObject.name + ", " + bodyB.GameObject.name + "\n";

                var bCola = hold.BColA;
                var aColb = hold.AColB;
                bool collide = this.Collide(bodyA, bodyB, out normal, out depth);


                //pushbox unique collision stuff only needs to happen when both boxes are pushboxes
                var bothPushBoxes = bodyB.IsPushbox && bodyA.IsPushbox;

                //if we do collide
                if (collide)
                {
                    var offset = normal * depth;
                    //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                    var trueOffsetB = offset * bCola;
                    //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                    var trueOffsetA = -offset * aColb;

                    //Debug.Log("discrete | " + bodyA.GameObject.name + " - " + bodyB.GameObject.name);
                    //Debug.Log("avel - " + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y);
                    //Debug.Log("bvel - " + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y);
                    //Debug.Log("apos - " + bodyA.Position.x + ", " + bodyA.Position.y);
                    //Debug.Log("bpos - " + bodyB.Position.x + ", " + bodyB.Position.y);
                    //Debug.Log("normal :: " + normal.x + ", " + normal.y);

                    //if (bothPushBoxes)
                    //{//}

                    //{


                    int aInCorner = Fix64.Sign(Fix64.Abs(bodyA.LinearVelocity.x));
                    int bInCorner = Fix64.Sign(Fix64.Abs(bodyB.LinearVelocity.x));

                    if (aInCorner != 0 && bInCorner != 0) { continue; }

                    bodyB.Move(trueOffsetB * bInCorner);
                    bodyA.Move(trueOffsetA * aInCorner);


                    this.ResolveCollision(bodyA, bodyB, normal, depth, aColb, bCola, false, aInCorner, bInCorner);


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
                }//we didn't collide
                else
                {
                    //swept collision
                    var colTime = this.SweptCollide(bodyA, bodyB, out normal, out depth);
                    if (colTime < Fix64.MaxValue)
                    {
                        //Debug.Log("max: " + bodyB.GetAABB().Max.x + ", " + bodyB.GetAABB().Max.y);
                        //Debug.Log("min: " + bodyB.GetAABB().Min.x + ", " + bodyB.GetAABB().Min.y);

                        //if (colTime > 0)
                        //if (bothPushBoxes)
                        //{
                        //Debug.Log("swept: " + colTime + " | " + bodyA.GameObject.name + " - " + bodyB.GameObject.name);
                        //Debug.Log("avel - " + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y);
                        //Debug.Log("bvel - " + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y);
                        //Debug.Log("apos - " + bodyA.Position.x + ", " + bodyA.Position.y);
                        //Debug.Log("bpos - " + bodyB.Position.x + ", " + bodyB.Position.y);
                        //Debug.Log("normal :: " + normal.x + ", " + normal.y);
                        //}
                        var offset = normal * depth;
                        //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                        var trueOffsetB = offset * bCola;
                        //will equal offset if bodyB can collide with bodyA, otherwise, it equals 0
                        var trueOffsetA = -offset * aColb;
                        //{

                        int aInCorner = Fix64.Sign(Fix64.Abs(bodyA.LinearVelocity.x));
                        int bInCorner = Fix64.Sign(Fix64.Abs(bodyB.LinearVelocity.x));

                        if (aInCorner != 0 && bInCorner != 0) { continue; }

                        bodyB.Move(bodyB.LinearVelocity * time * colTime);
                        bodyA.Move(bodyA.LinearVelocity * time * colTime);

                        this.ResolveCollision(bodyA, bodyB, normal, depth, aColb, bCola, false, aInCorner, bInCorner);


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
            */

            //Debug.Log("====================================================");

            // Movement step

            this.bodyList.ForEach(o => o.Step(time, this.gravity, 1));

            //ZzzLog.Instance.SetTxt(colList);
            broadPhaseList.Clear();
            #endregion
            //}

            //Debug.Log("------------end of physics step-------------------");
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
                    bodyA.Move(trueOffsetA);
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

        public void ResolveCollision(FlatBody bodyA, FlatBody bodyB, FVector2 normal, Fix64 depth, int aColB, int bColA, bool bothActivePushboxes, int aInCorner = 1, int bInCorner = 1)
        {
            FVector2 relativeVelocity = bodyB.LinearVelocity * bInCorner - bodyA.LinearVelocity * aInCorner;

            //if (bodyA.GameObject.name == "TestPlayer" && bodyA.LinearVelocity.magnitude > 0) UnityEngine.Debug.Log("- bodyA, velocity :: (" + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y + ")");
            //if (bodyB.GameObject.name == "TestPlayer" && bodyB.LinearVelocity.magnitude > 0) UnityEngine.Debug.Log("- bodyB, velocity :: (" + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y + ")");

            if (FVector2.Dot(relativeVelocity, normal) > 0)
            {
                return;
            }

            Fix64 e = 0;//Fix64.Min(1, 1);

            Fix64 j = -(1 + e) * FVector2.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass * aInCorner + bodyB.InvMass * bInCorner;

            FVector2 impulse = j * normal;

            //            bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            //            bodyB.LinearVelocity += impulse * bodyB.InvMass;

            //Debug.Log("a- " + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y);
            //Debug.Log("b- " + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y);
            bodyA.LinearVelocity -= impulse * bodyA.InvMass * aColB * aInCorner;
            bodyB.LinearVelocity += impulse * bodyB.InvMass * bColA * bInCorner;
            //Debug.Log(impulse.x + ", " + impulse.y);
            //Debug.Log("a- " + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y);
            //Debug.Log("b- " + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y);
            //bodyB.Position += new FVector2((depth), impulse.y) * bodyB.InvMass * bColA;// * ((Fix64)1 / (Fix64)60);
            //bodyA.Position -= new FVector2((depth), impulse.y) * bodyA.InvMass * aColB;// * ((Fix64)1 / (Fix64)60);

            //bodyA.Impulse -= impulse * bodyA.InvMass * aColB;
            //bodyB.Impulse += impulse * bodyB.InvMass * bColA;


            //bodyA.LinearVelocity -= impulse * bodyA.InvMass * aColB;
            //bodyB.LinearVelocity += impulse * bodyB.InvMass * bColA;
        }

        public Fix64 SweptCollide(FlatBody bodyA, FlatBody bodyB, out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = 0;

            var abox = bodyA.GetBox();
            var bbox = bodyB.GetBox();

            var dt = (Fix64.One / (Fix64)60);
            var botLeft = bbox.Min - abox.Max;
            var adim = new FVector2(bodyA.Width, bodyA.Height);
            var bdim = new FVector2(bodyB.Width, bodyB.Height);
            var fullSize = bdim + adim;
            var md = new FlatAABB(botLeft, botLeft + fullSize);

            // calculate the relative motion between the two boxes
            var relativeMotion = (bodyA.LinearVelocity - bodyB.LinearVelocity) * dt;
            var relativePosition = bodyA.Position - bodyB.Position;

            if (md.Min.x <= 0 && md.Max.x >= 0 && md.Min.y <= 0 && md.Max.y >= 0)
            {
                //calculate normal based on relative orientation to each other
                //either objects are directly beside or above each other, one box cannot do both
                if ((md.Min.x == 0 || md.Max.x == 0) && ((relativeMotion.x * relativePosition.x) < 0))
                {
                    normal = new FVector2(-Fix64.Sign(bodyA.Position.x - bodyB.Position.x), 0);
                    //Debug.Log("min or max x is zero - " + normal.x + ", " + normal.y);
                    //Debug.Log("avel - " + bodyA.LinearVelocity.x + ", " + bodyA.LinearVelocity.y);
                    //Debug.Log("bvel - " + bodyB.LinearVelocity.x + ", " + bodyB.LinearVelocity.y);
                }
                else if ((md.Min.y == 0 || md.Max.y == 0) && ((relativeMotion.y * relativePosition.y) < 0))
                {
                    normal = new FVector2(0, -Fix64.Sign(bodyA.Position.y - bodyB.Position.y));
                    //Debug.Log("min or max y is zero - " + normal.x + ", " + normal.y);
                }

                return Fix64.Zero;
            }


            // ray-cast the relativeMotion vector against the Minkowski AABB
            var h = md.getRayIntersectionFraction(FVector2.zero, relativeMotion);
            //Debug.Log("relative motion :: " + relativeMotion.x + ", " + relativeMotion.y);
            //Debug.Log("md :: " + md.Min.x + ", " + md.Min.y + " ; " + md.Max.x + ", " + md.Max.y);
            //Debug.Log(h);

            // check to see if a collision will happen this frame
            // getRayIntersectionFraction returns Math.POSITIVE_INFINITY if there is no intersection
            if (h < Fix64.MaxValue)
            {
                // yup, there WILL be a collision this frame
                // move the boxes appropriately
                //bodyA.Position += bodyA.LinearVelocity * dt * h;
                //bodyB.Position += bodyB.LinearVelocity * dt * h;

                // zero the normal component of the velocity
                // (project the velocity onto the tangent of the relative velocities
                //  and only keep the projected component, tossing the normal component)
                normal = relativeMotion.normalized;

                if (Fix64.Abs(normal.x) > Fix64.Abs(normal.y))
                {
                    normal = new FVector2(Fix64.Sign(normal.x), 0);

                }
                else
                {

                    normal = new FVector2(0, Fix64.Sign(normal.y));
                }
                ZzzLog.Instance.SetTxt(normal.x + ", " + normal.y);
                //Debug.Log(normal.x + ", " + normal.y);
                //bodyA.LinearVelocity = FVector2.Dot(bodyA.LinearVelocity, tangent) * tangent;
                //bodyB.LinearVelocity = FVector2.Dot(bodyB.LinearVelocity, tangent) * tangent;
                return h;
            }
            return Fix64.Zero;
        }



        public bool Collide(FlatBody bodyA, FlatBody bodyB, out FVector2 normal, out Fix64 depth)
        {
            normal = FVector2.zero;
            depth = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;
            return Collisions.IntersectRectangles(
                                    bodyA,
                                    bodyB,
                                    out normal, out depth);

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
