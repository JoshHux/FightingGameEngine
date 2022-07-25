using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;

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
                    bool isGrabbed = EnumHelper.HasEnum((uint)hold.Indicator, (uint)HitIndicator.GRABBED);
                    //Debug.Log("processing hit");
                    //set hitstop
                    int potenHitstop = hold.HitboxData.Hitstop;
                    this.SetStopTimer(potenHitstop);

                    //set universal target state
                    univTargetState = hold.HitboxData.UniversalStateCause;
                    totalStun = hold.HitboxData.Hitstun;

                    //add transition flag to let the status know we got hit
                    this.status.TransitionFlags = this.status.TransitionFlags | TransitionFlags.GOT_HIT;

                    //knockback/postion offset value (for grabs)
                    var groundedPhysVal = hold.HitboxData.GroundedKnockback;
                    groundedPhysVal.x = groundedPhysVal.x * hold.OtherOwner.Facing;
                    var airbornePhysVal = hold.HitboxData.AirborneKnockback;
                    airbornePhysVal.x = airbornePhysVal.x * hold.OtherOwner.Facing;


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
                            var newFacing = FixMath.NET.Fix64.Sign(diffPos);
                            this.status.CurrentFacingDirection = newFacing;
                        }
                        //Debug.Log("universal transition state - " + univTargetState);
                    }
                    else { }
                }
                i++;
            }

            if (univTargetState > -1)
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
            HitIndicator ret = HitIndicator.HIT;

            //TODO: have a check against the stateconditions for whether we are grabbed or not
            bool isGrabBox = EnumHelper.HasEnum((uint)boxData.HitboxData.Type, (uint)HitboxType.GRAB);
            if (isGrabBox) { ret |= HitIndicator.GRABBED; }

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