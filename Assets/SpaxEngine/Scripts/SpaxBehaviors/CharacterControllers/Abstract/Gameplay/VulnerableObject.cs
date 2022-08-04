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
        }

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

                //is this a non-whiff?
                bool notWhiff = hold.Indicator > 0;
                if (notWhiff)
                {
                    //did we block the hit?
                    int blocked = (int)EnumHelper.isNotZero((uint)(hold.Indicator & HitIndicator.BLOCKED));
                    //unblocked hit
                    int rawHit = blocked ^ 1;
                    //were we grabbed?
                    bool isGrabbed = EnumHelper.HasEnum((uint)hold.Indicator, (uint)HitIndicator.GRABBED);

                    //Debug.Log("processing hit");
                    //set hitstop
                    int potenHitstop = hold.HitboxData.Hitstop * rawHit + hold.HitboxData.BlockStop * blocked;
                    this.SetStopTimer(potenHitstop);

                    //set universal target state
                    univTargetState = hold.HitboxData.UniversalStateCause;
                    totalStun = hold.HitboxData.Hitstun * rawHit + hold.HitboxData.BlockStun * blocked;


                    //add transition flag to let the status know we got hit
                    this.status.TransitionFlags = this.status.TransitionFlags | TransitionFlags.GOT_HIT | (TransitionFlags)((int)TransitionFlags.BLOCKED_HIT * blocked);

                    //knockback/postion offset value (for grabs)
                    var groundedPhysVal = hold.HitboxData.GroundedKnockback;
                    groundedPhysVal.x = groundedPhysVal.x * hold.OtherOwner.Facing;
                    var airbornePhysVal = hold.HitboxData.AirborneKnockback;
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
                        this.status.CalcVelocity = (groundedPhysVal * grounded) + (airbornePhysVal * airborne);
                    }
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


        //list of hitboxes to process, reset every frame
        private List<HitInfo> m_hurtList;
        //called by the hurtbox to add the hitbox to process in HurtBoxQueryUpdate
        public HitIndicator AddHitboxToQuery(HitInfo boxData)
        {
            HitIndicator ret = HitIndicator.WHIFF;
            /*--- our state condition info ---*/

            //current state conditions we process
            var curSttCond = this.status.TotalStateConditions;

            //what we are invulnerable against
            var strikeInvuln = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.INVULNERABLE_STRIKE));
            var grabInvuln = (int)EnumHelper.isNotZero((uint)(curSttCond & StateConditions.INVULNERABLE_GRAB));

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
            var isStrikeGrab = isGrab & isStrike;

            /*--- COMPARING THE HITBOX INFO VS OUR STATE CONDITIONS ---*/

            //strike box
            var weBlocked = EnumHelper.HasEnumInt(guardEnum, (uint)strikeType);
            var weGotHit = weBlocked ^ 1;


            //provide data to the ret
            ret = (HitIndicator)((weBlocked * (int)HitIndicator.BLOCKED) | (weGotHit * (int)HitIndicator.HIT));


            //TODO: try to make this branchless
            //invuln check
            if (((isStrike > 0) || (isStrikeGrab > 0)) && (strikeInvuln > 0))
            {
                ret = HitIndicator.WHIFF;

            }
            else if (isGrab > 0)
            {
                if ((isStrikeGrab == 0) && (grabInvuln > 0))
                {
                    ret = HitIndicator.WHIFF;
                }
                else if (EnumHelper.HasEnum((uint)grabType, (uint)airOrGround, true))
                {
                    //Debug.Log(grabType + " " + airOrGround);
                    ret |= HitIndicator.GRABBED;
                }
                else
                {
                    ret = HitIndicator.WHIFF;
                }
            }
            //Debug.Log(ret);



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