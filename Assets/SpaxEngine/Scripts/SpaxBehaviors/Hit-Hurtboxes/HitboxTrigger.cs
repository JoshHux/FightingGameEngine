using System.Collections.Generic;
using System.Linq;
using FlatPhysics.Contact;
using FlatPhysics.Unity;
using FightingGameEngine.Data;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Gameplay
{
    public class HitboxTrigger : BoxTrigger
    {
        [ReadOnly, UnityEngine.SerializeField] private FrameTimer _activeTimer;
        private HitboxData m_data;
        [ReadOnly, UnityEngine.SerializeField] private List<BoxTrigger> m_wasColliding;
        [ReadOnly, UnityEngine.SerializeField] private List<BoxTrigger> _curColliding;

        void Awake()
        {
            this._activeTimer = new FrameTimer();
            this.m_wasColliding = new List<BoxTrigger>();
            this._curColliding = new List<BoxTrigger>();
        }
        protected override void OnStart()
        {
            base.OnStart();

            this.Trigger.Body.OnOverlap += this.OnFlatOverlap;
        }

        void OnDestroy()
        {
            this.Trigger.Body.OnOverlap -= this.OnFlatOverlap;

        }

        private void ActivateBox(in HitboxData newData)
        {
            this.m_data = newData;

            var newPos = this.m_data.Position;
            var newDim = this.m_data.Dimensions;

            this._activeTimer = new FrameTimer(this.m_data.Duration);

            //reset collision list
            this._curColliding.Clear();
            this.m_wasColliding.Clear();

            this.CommonActivateBox(newPos, newDim);
        }

        protected override void CheckDataFromFrame(object sender, in FrameData data)
        {
            if (data == null) { this.DeactivateBox(); return; }
            //UnityEngine.Debug.Log((data == null));
            //get the data for quick and easy access
            var boxdata = data.GetHitbox(this.triggerIndex);
            //valid or invalid boxdata, checks if durationis nonzero
            bool isValid = boxdata.IsValid();

            //if invalid data, don't
            if (!isValid)
            {
                return;
            }
            this.ActivateBox(boxdata);
        }

        public override void DeactivateBox()
        {
            this.CommonDeactivateBox();
            //clear the lists of bodies, so that we can start out our next activation with a clean slate
            //clear the list of bodies we were colliding with
            this.m_wasColliding.Clear();
            //clear the list of bodies we are colliding with
            this._curColliding.Clear();
        }


        //delegate to add the object this object is overlapping with
        //if we are not overlapping with an object, nothing is added to curColliding
        private void OnFlatOverlap(in ContactData c)
        {
            //only continue if we are active
            bool dontContinue = !this.IsActive();
            if (dontContinue) { return; }

            //the frigidbody we might add to the currently colliding list

            //get the FRigidbody
            var frb = c.other;

            //if (frb == null) { return; }

            //get the box we want to check against
            var possibleAddition = frb.GetComponent<BoxTrigger>();

            //UnityEngine.Debug.Log(frb.name);

            //the other box's owner
            var otherOwner = possibleAddition.Owner;

            //is the box we are trying to hit active?
            var otherIsActive = possibleAddition.IsActive();

            //check to make sure that the allegiance is mismatched
            var checkAllegiance = possibleAddition.Allegiance != this.allegiance;

            //we are not alligned to this box and the other box is active
            if (checkAllegiance && otherIsActive)
            {
                //check if we are NOT colliding with that owner in curColliding
                //the box's owner is not an owner of something we are colliding with
                var listBox = this._curColliding.Find(obj => obj.Owner == otherOwner);
                var notInCur = listBox == null;

                //only continue if the owner already has a box in the list
                //if the owner already has a box in the curColliding list, is that box a hitbox?
                var listBoxIsHitbox = !notInCur && (listBox is HitboxTrigger);

                //owner has a hitbox in the list, if we want to add a hurtbox, remove the hitbox
                var removeListBox = listBoxIsHitbox && (possibleAddition is HurtboxTrigger);


                //we want to remove listBox from curColliding
                if (removeListBox)
                {
                    this._curColliding.Remove(listBox);
                }


                //we want to possibleAddition ONLY if we either
                //  1) removed a hitbox
                //  2) owner doesn't have a box in the list yet
                bool okayAdd = notInCur || removeListBox;


                //if object was in neither list
                if (okayAdd)
                {
                    //add new object to curColliding
                    this._curColliding.Add(possibleAddition);
                    //UnityEngine.Debug.Log("okay to add");
                }

            }
        }

        //call to get the list of BoxTrigger to process in QueryHitbox
        private List<BoxTrigger> GetDiff()
        {
            //due to how we programmed how objects get added to curColliding
            //  we can assume that every object in curColliding and wasColliding
            //  has a unique owner (respective to each list)

            //list to return
            var ret = new List<BoxTrigger>();

            //the goal is to get the BoxTrigger objects in curColliding that have owners that DON'T
            //  appear as an owner in wasColliding

            //loop to search through the elements of curColliding
            int i = 0;
            int len = this._curColliding.Count;
            while (i < len)
            {
                //get the box in curList we're looking at
                var curListBox = this._curColliding[i];
                //get the owner of that box
                var curBoxOwner = curListBox.Owner;

                //does that owner have a box in wasColliding
                var hasInWas = this.m_wasColliding.Exists(obj => obj.Owner == curBoxOwner);

                //if the owner DOES NOT have a box in wasColliding
                if (!hasInWas)
                {
                    //first time interacting with that owner, add it to the list to return
                    ret.Add(curListBox);
                }

                i++;
            }


            return ret;
        }

        //call to query the hitbox contacts returns hit info
        //ONLY CALL IF BOX IS ACTIVE
        public HitInfo[] QueryHitbox()
        {
            bool active = this.IsActive();

            //init object to return
            var ret = new List<HitInfo>();
            if (active)
            {

                //the new boxes we are colliding with
                //these are the PORTION of boxes WE ARE CURRENTLY COLLIDING WITH
                //THAT have owners that we HAVEN'T interacted with
                var diffColliders = this.GetDiff();



                int i = 0;
                int len = diffColliders.Count;
                while (i < len)
                {
                    //the box we want tp [rpcess]
                    var box = diffColliders[i];
                    //owner of the box
                    var boxOwner = box.Owner;


                    //the lerp position for vfx
                    var ourPos = this.transform.position;
                    var theirPos = boxOwner.transform.position;
                    var lerpPos = (ourPos * 0.5f) + (theirPos * 0.5f);

                    //indicator for the HitInfo to add to the list
                    //will be given a non-zero value below
                    HitIndicator indicator = 0;

                    //the hitInfo object to add to the return list
                    //replace indicator and object parameter later, put like this for now for stuff in the hit object
                    var toAdd = new HitInfo(this.m_data, indicator, lerpPos, this.Owner);

                    //switch case based on box type
                    switch (box)
                    {
                        //if we're querying a hitbox
                        case HitboxTrigger hitbox:
                            //remove the box from curColliding
                            //this is to prevent a scenario where a clash would occur and then immediately
                            //afterwards,a hurtbox would activate and SHOULD trigger a hit, IE clashing DP's
                            //guarentees that wasColliding ONLY consists of HurtboxTrigger objects
                            //this._curColliding.Remove(box);
                            break;
                        //if we're querying a hurtbox
                        case HurtboxTrigger hurtbox:
                            //TODO: conduct experiments in Xrd regarding to what happens to opponent who stops blocking a still active hitbox
                            //  if the character gets hit, remove box from curColliding if hitIndicator has the BLOCKED flag, makes it so that we can hit them on the next frame
                            indicator = hurtbox.HurtThisBox(toAdd);
                            break;
                        default:
                            UnityEngine.Debug.LogError("Hitbox has detected invalid box class");
                            break;
                    }

                    //add the hitInfo to the list
                    ret.Add(toAdd);

                    //replace indicator value with the right value
                    toAdd.Indicator = indicator;
                    //replace with correct vulnerableObject
                    toAdd.OtherOwner = boxOwner;

                    i++;
                }

                //the list of objects in curColliding are now the list of wasColliding
                this.m_wasColliding = new List<BoxTrigger>(this._curColliding);
                //clear the list of boxes we are currently colliding with
                this._curColliding.Clear();
            }

            //tick active timer
            this._activeTimer.TickTimer();

            return ret.ToArray();

        }

        public override bool IsActive()
        {
            var ret = !this._activeTimer.IsDone();

            return ret;
        }


    }
}