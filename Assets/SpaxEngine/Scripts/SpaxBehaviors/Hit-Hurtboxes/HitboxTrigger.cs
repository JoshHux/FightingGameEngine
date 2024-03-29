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

            //add our own activator event when we reached a frame and may activate this box trigger
            (this.Owner as CombatObject).OnHitFrameReached += CheckDataFromFrame;
        }

        void OnDestroy()
        {
            this.Trigger.Body.OnOverlap -= this.OnFlatOverlap;
            (this.Owner as CombatObject).OnHitFrameReached -= CheckDataFromFrame;

        }

        private void ActivateBox(in HitboxData newData)
        {
            this.m_data = newData;

            var newPos = this.m_data.Position;
            var newDim = this.m_data.Dimensions;

            this._activeTimer = new FrameTimer(this.m_data.Duration);

            //reset collision list
            //this._curColliding.Clear();
            //this.m_wasColliding.Clear();

            this.CommonActivateBox(newPos, newDim);
        }

        private void CheckDataFromFrame(object sender, in HitboxHolder data)
        {
            //if (data == null) { this.DeactivateBox(); return; }
            //UnityEngine.Debug.Log((data == null));
            //get the data for quick and easy access
            var boxData = data.GetHitbox(this.triggerIndex);
            this.SetData(boxData);
        }

        protected override void ApplyGameState(object sender, in GameplayState state)
        {
            //setting new hitboxTrigger information, deactivate to throw away any data we had previously
            this.DeactivateBox(this);

            //get the box here
            var hold = state.HitboxStates.GetValue(this.triggerIndex);
            this.SetData(hold.CurrentData);
            this._activeTimer = new FrameTimer(hold.TimerInfo.EndTime, hold.TimerInfo.TimeElapsed);

            //set WasColliding information
            int i = 0;
            int len = this.m_wasColliding.Count;

            //instance of manager
            var manager = Spax.SpaxManager.Instance;
            while (i < len)
            {
                //current state info we're looking at
                var info = hold.WasColliding.GetValue(i);

                //if the ID of the character is -1, then we don't care about any other object
                //  this is because of how we added elements to the list, we can just break here
                if (info.GetValue(0) == -1) { break; }

                //get the character by the ID
                var character = manager.GetLivingObjectByID(info.GetValue(0));
                //get the box we want to add
                var trigInd = info.GetValue(1);
                var absTrigInd = UnityEngine.Mathf.Abs(trigInd);

                BoxTrigger toAdd = (character as VulnerableObject).GetHurtbox(absTrigInd);

                if (trigInd < 0)
                {
                    toAdd = (character as CombatObject).GetHitbox(absTrigInd);
                }

                this.m_wasColliding.Add(toAdd);

                i++;
            }
        }

        public override void DeactivateBox(object sender)
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
            if (dontContinue)
            {
                //deactivate the box, just in case
                this.DeactivateBox(this);
                return;
            }

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
                    //if we're at the limit of 16 objects, drop the first one added
                    if (this._curColliding.Count >= 16) { this._curColliding.RemoveAt(0); }
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
                    var toAdd = new HitInfo(this.m_data, indicator, lerpPos, boxOwner, this.Owner as CombatObject);

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
                            //UnityEngine.Debug.Log(hurtbox.GetHurtboxData().Dimensions.x + ", " + hurtbox.GetHurtboxData().Dimensions.y + " | " + hurtbox.GetHurtboxData().Position.x + ", " + hurtbox.GetHurtboxData().Position.y);
                            //UnityEngine.Debug.Log("     " + hurtbox.Trigger.Body.Width + ", " + hurtbox.Trigger.Body.Height + " | " + hurtbox.Trigger.Body.Position.x + ", " + hurtbox.Trigger.Body.Position.y);
                            break;
                        default:
                            UnityEngine.Debug.LogError("Hitbox has detected invalid box class");
                            break;
                    }

                    //add the hitInfo to the list
                    ret.Add(toAdd);

                    //replace indicator value with the right value
                    toAdd.Indicator = indicator;

                    i++;
                }
                //the list of objects in curColliding are now the list of wasColliding
                this.m_wasColliding.AddRange(diffColliders);
                int diffLen = this.m_wasColliding.Count - 16;
                if (diffLen > 0 && this.m_wasColliding.Count > 0)
                {
                    this.m_wasColliding.RemoveRange(0, UnityEngine.Mathf.Abs(diffLen));
                }
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

        private void SetData(in HitboxData boxData)
        {
            //valid or invalid boxdata, checks if durationis nonzero
            bool isValid = boxData.IsValid();

            //if invalid data, don't
            if (!isValid)
            {
                return;
            }
            this.ActivateBox(boxData);
        }

        public HitboxData GetHitboxData() { return this.m_data; }
        public FrameTimer GetTimer() { return this._activeTimer; }
        public List<BoxTrigger> GetWasCol() { return this.m_wasColliding; }
    }
}