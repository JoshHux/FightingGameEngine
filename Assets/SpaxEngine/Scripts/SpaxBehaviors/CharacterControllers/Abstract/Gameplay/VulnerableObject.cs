using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;
using FixMath.NET;

namespace FightingGameEngine.Gameplay
{
    public delegate void BoxActivator(object sender, FrameData frameData);

    public abstract class VulnerableObject : LivingObject
    {
        private HurtboxTrigger[] _hurtboxes;

        //delgate that is called to activate hit/hurtboxes, we pass the hitbox triggers the frame data to know whether or not to activate
        public BoxActivator OnFrameReached;

        public int Facing { get { return this.status.CurrentFacingDirection; } }

        protected override void OnStart()
        {
            base.OnStart();

            //find the parent of all hurtboxes
            GameObject hurtHolder = ObjectFinder.FindChildWithTag(this.gameObject, "HurtboxContainer");
            //stor all of the hurtboxes
            this._hurtboxes = hurtHolder.GetComponentsInChildren<HurtboxTrigger>();
            //initialize all hurtboxes
            int len = _hurtboxes.Length;
            for (int i = 0; i < len; i++)
            {
                HurtboxTrigger box = this._hurtboxes[i];
                //set the trigger index for each box
                box.SetTriggerIndex(i);
                box.SetTriggerAllegiance(this.status.Allegiance);
                //we don't need to hook the delegate, since the boxes do it themselves on start
            }

            //init the list of hitinfo objects to process when hit
            this.m_hurtList = new List<HitInfo>();
            //init indicator of whether or not we landed a strike
            this.landedStrikeThisFrame = false;
        }

        //ONLY HERE TO MAKE SURE THAT STRIKE-TRADES END THE STRIKE'S FAVOR
        protected bool landedStrikeThisFrame = false;

        protected override void HurtboxQueryUpdate()
        {
            int i = 0;
            int len = this.m_hurtList.Count;

            //iindex of universal target state we want to go to
            int univTargetState = -1;
            int totalStun = 0;

            while (i < len)
            {
                var hold = this.m_hurtList[i];
                //applied knockback
                var kb = new FVector2();

                //were we grabbed?
                bool isGrabbed = EnumHelper.HasEnum((uint)hold.Indicator, (uint)HitIndicator.GRABBED);

                //is this a non-whiff?
                bool notWhiff = hold.Indicator > 0;
                bool tradedWithGrab = (isGrabbed && landedStrikeThisFrame);
                if (notWhiff && !tradedWithGrab)
                {
                    //quick access to the hitbox data
                    var boxData = hold.HitboxData;

                    //did we block the hit?
                    int blocked = (int)EnumHelper.isNotZero((uint)(hold.Indicator & HitIndicator.BLOCKED));
                    //unblocked hit
                    int rawHit = blocked ^ 1;

                    //Debug.Log("processing hit");

                    //set universal target state
                    univTargetState = boxData.UniversalStateCause;
                    totalStun = boxData.Hitstun * rawHit + boxData.BlockStun * blocked;


                    //add transition flag to let the status know we got hit
                    this.status.TransitionFlags = this.status.TransitionFlags | TransitionFlags.GOT_HIT | (TransitionFlags)((int)TransitionFlags.BLOCKED_HIT * blocked);

                    //knockback/postion offset value (for grabs)
                    var groundedPhysVal = boxData.GroundedKnockback;
                    groundedPhysVal.x = groundedPhysVal.x * hold.OtherOwner.Facing;
                    var airbornePhysVal = boxData.AirborneKnockback;
                    airbornePhysVal.x = airbornePhysVal.x * hold.OtherOwner.Facing;

                    //remove the accumulated velocity so far
                    this.status.CurrentVelocity = new FVector2(0, 0);


                    if (isGrabbed)
                    {
                        //position of grabber
                        var grabberPosition = hold.OtherOwner.FlatPosition;

                        var newPosition = grabberPosition + groundedPhysVal;

                        this.SetPosition(newPosition);

                        //set proper facing direction
                        //what is the difference between our x position and their x position?
                        var diffPos = hold.OtherOwner.FlatPosition.x - this.status.CurrentPosition.x;

                        //if our facing direction and the difference in position are different, then we should turn
                        bool shouldTurn = (diffPos * this.status.CurrentFacingDirection) < 0;

                        if (shouldTurn)
                        {
                            var newFacing = Fix64.Sign(diffPos);
                            this.status.CurrentFacingDirection = newFacing;
                        }
                        //Debug.Log("universal transition state - " + univTargetState);
                        Spax.SpaxManager.Instance.ResolveRepositioning(hold.OtherOwner.Body, this.Body);
                    }
                    else
                    {
                        int airborne = this.IsAirborne();
                        int grounded = airborne ^ 1;
                        //Velocity in hitstop will be restored based on CurrentVelocity
                        kb = (groundedPhysVal * grounded) + (airbornePhysVal * airborne);
                        //Debug.Log("grounded - " + grounded + " | airborne - " + airborne + " | knockback - " + ((groundedPhysVal * grounded) + (airbornePhysVal * airborne)).x + " , " + ((groundedPhysVal * grounded) + (airbornePhysVal * airborne)).y);

                        //set bounce data
                        this.status.GroundBounces = boxData.GroundBounces;
                        this.status.GroundBounceScaling = boxData.GroundBounceMultiplier;
                        this.status.WallBounces = boxData.WallBounces;
                        this.status.WallBounceScaling = boxData.WallBounceMultiplier;
                    }

                    //set hitstop
                    int potenHitstop = hold.HitboxData.Hitstop * rawHit + hold.HitboxData.BlockStop * blocked;
                    this.SetStopTimer(potenHitstop);
                    //set the current velocity to kb so that we only override velocity when hit
                    this.status.CurrentVelocity = kb;
                }
                i++;
            }

            //DON'T transition if we're going into hit/blockstun with 0 totalstun
            if (univTargetState > -1 && !(univTargetState == 0 && totalStun <= 0))
            {
                this.TryTransitionUniversalState(univTargetState);
                //set stun duration
                int sttDur = (this.status.CurrentState.Duration == 0) ? totalStun : this.status.CurrentState.Duration;
                this.Status.StateTimer = new FrameTimer(sttDur);
            }

            this.m_hurtList.Clear();
            this.landedStrikeThisFrame = false;
        }

        //call to process the state conditions of our current state
        protected override void ProcessStateConditions(StateConditions stateConditions)
        {
            //grounded or not
            int grounded = this.IsAirborne() ^ 1;
            //walled or not
            int walled = this.IsWalled();

            //bounce data
            //  we multiply each ground bounce count by the respective environment check to make it so that we only apply bounce if each count is nonzero
            //wall bounce count
            int groundBounces = this.status.GroundBounces * grounded;
            //ground bounce count
            int wallBounces = this.status.WallBounces * walled;

            //get the current physics velocity for bouncing
            //new velocity that will be reassigned to the current velocity
            var newVel = this.status.CurrentVelocity;

            base.ProcessStateConditions(stateConditions);

            //TODO: see if we can make this branchless
            //  MAYBE: check to make sure we only wall bounce when airborne

            //we only bounce with x or y velocity since we ony want the character to either pop up or away from the wall
            //  we don't check the direction of the velocity but generally, we will only really collide with the wall or ground 
            //  in the same direction of their velocity
            //      IF BOUNCES ARE NOT WORKING PROPERLY: CHANGE THE CHECK OF VELOCITY TO EXPLICITLY LAUNCH AWAY FROM THE WALL
            if (groundBounces > 0)
            {
                var gBounceScaling = this.status.GroundBounceScaling;

                //multiply by the negative scaling so that we bounce back up
                newVel.y = newVel.y * gBounceScaling * -1;

                //deincrement respective bounce count
                this.status.GroundBounces -= 1;
            }
            if (wallBounces > 0)
            {
                var wBounceScaling = this.status.WallBounceScaling;

                //multiply by the negative scaling so that we bounce back up
                newVel.x = newVel.x * wBounceScaling * -1;
                //deincrement respective bounce count
                this.status.WallBounces -= 1;
            }

            //reassign new velocity
            this.status.CurrentVelocity = newVel;

        }
        //call to process the frame data
        protected override void ProcessFrameData(FrameData frame)
        {
            //call base version
            base.ProcessFrameData(frame);

            /*----- ACTIVATING HURTBOXES -----*/
            //call the delegate and pass the frame in, the boxes will take care of the rest
            //the way that we set this up, we don't need to call this from CombatObject because the HitboxTrigger objects will also have their hooks in this delegate
            this.OnFrameReached.Invoke(this, frame);
        }

        //call to process transition event enums
        protected override void ProcessTransitionEvent(TransitionEvents te)
        {
            base.ProcessTransitionEvent(te);

            /*----- PROCESSING TECHING EVENT -----*/
            bool grabTech = EnumHelper.HasEnum((uint)te, (uint)TransitionEvents.GRAB_TECH);

            if (grabTech)
            {
                int otherID = (this.status.PlayerID == 0) ? 1 : 0;
                var grabber = Spax.SpaxManager.Instance.GetLivingObjectByID(otherID);

                grabber.TryTransitionUniversalState(1);
            }
        }


        //list of hitboxes to process, reset every frame
        private List<HitInfo> m_hurtList;
        //called by the hurtbox to add the hitbox to process in HurtBoxQueryUpdate
        public HitIndicator AddHitboxToQuery(HitInfo boxData)
        {
            HitIndicator ret = HitIndicator.WHIFF;
            /*--- our state condition info ---*/

            //current state conditions we process
            var curSttCond = this.status.TotalStateConditions;
            //are we in a stunstate?
            var inStun = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.STUN_STATE));

            //what we are invulnerable against
            var strikeInvuln = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.INVULNERABLE_STRIKE));
            var grabInvuln = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.INVULNERABLE_GRAB));
            var projInvuln = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.INVULNERABLE_PROJECTILE));
            var onTheGround = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.ON_THE_GROUND));

            //what we have guard point against
            var midGuard = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.GUARD_POINT_MID));
            var lowGuard = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.GUARD_POINT_LOW));
            var highGuard = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.GUARD_POINT_HIGH));
            var guardEnum = (uint)((uint)curSttCond >> 23);

            //are we grounded or airborne?
            var airOrGround = (uint)(this.status.TotalStateConditions & (StateConditions.GROUNDED | StateConditions.AIRBORNE));

            /*--- attacker hitbox properties ---*/

            //the total hitbox characteristics
            var hitboxType = boxData.HitboxData.Type;

            //strike properties of the hitbox
            var strikeType = hitboxType & HitboxType.STRIKE;
            //grab properties of the hitbox
            uint grabType = ((uint)(hitboxType & HitboxType.GRAB) >> 3);

            //hitbox type check
            var isStrike = (int)EnumHelper.isNotZero((uint)(strikeType));
            var isGrab = (int)EnumHelper.isNotZero((uint)(grabType));
            var isProj = (int)EnumHelper.isNotZero((uint)(hitboxType & HitboxType.PROJECTILE));
            var isNotOtg = ((int)EnumHelper.isNotZero((uint)(hitboxType & HitboxType.OFF_THE_GROUND)) ^ 1);
            var isStrikeGrab = isGrab & isStrike;

            /*--- COMPARING THE HITBOX INFO VS OUR STATE CONDITIONS ---*/

            //strike box
            var weBlocked = EnumHelper.HasEnumInt(guardEnum, (uint)strikeType);
            var weGotHit = weBlocked ^ 1;


            //provide data to the ret
            ret = (HitIndicator)((weBlocked * (int)HitIndicator.BLOCKED) | (weGotHit * (int)HitIndicator.HIT));


            //TODO: try to make this branchless
            //invuln check
            //  each multiplier vairable can ONLY be zero IF they are that hitbox type AND we have the corresponding invuln condition 
            //  we want the stuff afte 1^ to be 0 for a hit, 1 for a whiff

            //projectile invuln multiplier
            //  only considers a whiff if this is a projectile first
            var projMult = 1 ^ (isProj * projInvuln);
            var otgMult = 1 ^ (isNotOtg * onTheGround);

            ret = (HitIndicator)((int)ret * projMult * otgMult);

            //strike invuln multiplier
            //  mult is 1 if it isn't a strike box
            var strikeIndicator = (isStrike | isStrikeGrab);
            var strikeMult = 1 ^ ((strikeIndicator * strikeInvuln) | (strikeIndicator ^ 1));

            ret = (HitIndicator)((int)ret * strikeMult);

            //grab invuln multiplier
            var grabLocMatch = (int)EnumHelper.isNotZero((uint)(grabType & airOrGround));
            //  ((isStrikeGrab^isGrab)*isStrikeGrab) overrides isGrab to be 0 if the hit is a strike-grab
            //      it's 1 if we have a regular grab
            var rawGrab = ((isStrikeGrab ^ isGrab) * (isStrikeGrab ^ 1));
            var grabMult = 1 ^ ((rawGrab & (grabInvuln | inStun | (grabLocMatch ^ 1))) | (isStrike * (1 ^ isStrikeGrab)));

            ret |= (HitIndicator)((int)HitIndicator.GRABBED * grabMult);


            /*if (isProj > 0 && projInvuln > 0)
                        {
                            ret = HitIndicator.WHIFF;
                        }
                        else
            if (((isStrike > 0) || (isStrikeGrab > 0)) && (strikeInvuln > 0))
            {
                ret = HitIndicator.WHIFF;
            }
            else
            if (isGrab > 0)
            {
                //IF it's a strike-grab AND we don't have strike invuln
                //  THEN we get grabbed
                if ((isStrikeGrab > 0) && (strikeInvuln == 0))
                {
                    ret |= HitIndicator.GRABBED;
                }
                else if (grabInvuln > 0)
                {
                    //Debug.Log("INVULN WHIFF");
                    ret = HitIndicator.WHIFF;
                }
                else if (EnumHelper.HasEnum((uint)grabType, (uint)airOrGround, true) && !EnumHelper.HasEnum((uint)this.status.TotalStateConditions, (uint)StateConditions.STUN_STATE))
                {
                    //Debug.Log(grabType + " " + airOrGround);
                    ret |= HitIndicator.GRABBED;
                }
                else
                {
                    //Debug.Log("ELSE WHIFF");
                    ret = HitIndicator.WHIFF;
                }
            }*/
            //Debug.Log(grabType + " " + airOrGround);
            //Debug.Log(grabMult + " = " + rawGrab + " & " + (grabInvuln | inStun | (grabLocMatch ^ 1)));



            //make copy of object just in case of some shallow memory access shenanigans
            //last parameter is the object that hit us (to be used later)
            var toAdd = new HitInfo(boxData.HitboxData, ret, boxData.ContactLoc, boxData.OtherOwner);



            //make sure we only are hit by the hitbox WITH THE HIGHEST priority if hit by multiple hitboxes from the same object
            var existsTarget = this.m_hurtList.Find(o => o.OtherOwner == toAdd.OtherOwner);
            if (existsTarget == null)
            {
                //Debug.Log("add to list");

                //add to list of hitboxes to process
                this.m_hurtList.Add(toAdd);

            }
            else
            {
                //Debug.Log("checking the hit priority");
                bool newHasHigherPriority = existsTarget.HitboxData.Priority > toAdd.HitboxData.Priority;

                if (newHasHigherPriority)
                {
                    this.m_hurtList.Remove(existsTarget);
                    //add to list of hitboxes to process
                    this.m_hurtList.Add(toAdd);
                }
            }

            return ret;
        }


    }
}