using UnityEngine;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class TransitionData
    {
        [SerializeField] private soStateData _targetState;

        [SerializeField] private InputItem[] _requiredInputs;
        [SerializeField] private TransitionEvents _transitionEvents;
        [SerializeField] private TransitionFlags _requiredTransitionFlags;
        [SerializeField] private ResourceData _requiredResources;

        public soStateData TargetState { get { return this._targetState; } }
        public TransitionEvents TransitionEvents { get { return this._transitionEvents; } }
        public StateType RequiredCancels { get { return this._targetState.Type; } }
        public TransitionFlags RequiredTransitionFlags { get { return this._requiredTransitionFlags; } }
        public ResourceData RequiredResources { get { return this._requiredResources; } }

        public bool CheckInputs(InputItem[] playerInputs)
        {
            int playerInputLen = playerInputs.Length;
            int requiredInputLen = this._requiredInputs.Length;

            //index for the player's inputs
            int i = 0;
            //index for the required inputs
            int j = 0;

            //current item we are checking against
            //we start with the first item in the required items
            var reqItem = this._requiredInputs[j];
            //how many frames it's been since the last valid input
            int sinceLastMatch = 0;

            //check against the list of player inputs
            while (i < playerInputLen)
            {
                //current player input item we're looking at
                var curPlayerItem = playerInputs[i];

                //current result of the overall check of the input
                bool overallCheck = false;

                //should we skip adding the time lingered?
                //only applied to the second item in the array (that's the first item checked for the input requirements [that aren't controller checks])
                bool skipIncrement = (i == 2) && curPlayerItem.LenientBuffer;

                //if we ignore the time elapsed, don't add the hold duration
                if (!skipIncrement)
                {
                    //add the amount of frames the player lingered on this input
                    sinceLastMatch += curPlayerItem.HoldDuration;
                }

                //if the number of frames passed since the last valid input is more than the input leniency, then break
                bool tooLongSinceLastInput = sinceLastMatch > Spax.SpaxManager.Instance.StaticValues.InputLeniency;
                if (tooLongSinceLastInput) { break; }

                //player's buttons
                var playerItemBtn = curPlayerItem.Input & InputEnum.BUTTONS;
                //player's direction
                var playerItemDir = curPlayerItem.Input & InputEnum.DIRECTIONS;

                //check the items
                //  check if the flags match
                bool checkHasFlags = EnumHelper.HasEnum((uint)curPlayerItem.Flags, (uint)reqItem.Flags, true);
                //      check if there's a hold durations
                bool reqHold = (reqItem.HoldDuration > 0);
                //      this makes it so that we still set it to true if we don't require a hold
                //          but if we do, we force a fail
                checkHasFlags = (!reqHold) && checkHasFlags;

                //      check for the 4way direction flag
                bool reqHas4wayFlag = EnumHelper.HasEnum((uint)reqItem.Flags, (uint)InputFlags.DIR_4WAY, true);


                //  we check if the inputs match
                //      check the buttons
                bool checkInputBtn = EnumHelper.HasEnum((uint)playerItemBtn, (uint)reqItem.Input, true);
                //      check the directions, we do a lenient check if the required flags DOES NOT INCLUDE the 4way flag
                bool checkInput = checkInputBtn && EnumHelper.HasEnum((uint)playerItemDir, (uint)reqItem.Input, !reqHas4wayFlag);

                //for now, just set the overall check to the flag and overall input check
                overallCheck = checkHasFlags && checkInput;

                //  extra checks
                //      check charge and hold inputs
                //          we only check if the initial button and direction check is passed
                //          the hold duration of the input is > 0
                //          AND the current item has the correct buttons and directions we're looking for
                //          AND the current check failed
                bool checkHold = checkInput && reqHold && !overallCheck;
                if (checkHold)
                {
                    //we need to fast forward throught the rest of the inputs, we're looking for a press/release of the required item
                    //we start at i+1 since we already checked the item at i
                    int k = i + 1;
                    //filter the flags to make looking for the press/release we're looking for
                    var lookForFlag = reqItem.Flags & InputFlags.CHECK_CONTROLLER;

                    //amount of frames held
                    int kHeldFrames = 0;

                    while (k < playerInputLen)
                    {

                        //current player input item we're looking at
                        var kCurPlayerItem = playerInputs[k];
                        //add the amount of frames held
                        kHeldFrames += kCurPlayerItem.HoldDuration;

                        //player's buttons
                        var kPlayerItemBtn = kCurPlayerItem.Input & InputEnum.BUTTONS;
                        //player's direction
                        var kPlayerItemDir = kCurPlayerItem.Input & InputEnum.DIRECTIONS;

                        //check the release/press flag
                        bool kCheckHasFlags = EnumHelper.HasEnum((uint)curPlayerItem.Flags, (uint)lookForFlag, true);

                        //      check the buttons
                        bool kCheckInputBtn = kCheckHasFlags && EnumHelper.HasEnum((uint)playerItemBtn, (uint)reqItem.Input, true);
                        //      check the directions, we do a lenient check if the required flags DOES NOT INCLUDE the 4way flag
                        bool kCheckInput = kCheckInputBtn && EnumHelper.HasEnum((uint)playerItemDir, (uint)reqItem.Input, !reqHas4wayFlag);

                        //  we found an input that should stop the loop
                        if (kCheckInput)
                        {
                            //TODO: try to reorder the if/else statements to make it so that we only end the loop in one if/else statement instead of 2

                            //if 4way flag is on the required item, we check the next item to make sure the release/press direction is still valid
                            //  this is to allow for charge partitioning
                            //  for this process, we have a strict leniency of 0 frames, since that's the hold duration of the release for charge partitioning

                            //  NOTE: THIS FAILS IF WE REACH THE END OF THE LIST AND THE LAST ITEM WAS A PRESS, THIS IS BECAUSE WE CANNOT CHECK THE NEXT ITEM TO MAKE SURE IT'S VALID
                            //      the workaround for this is to make the list of player inputs super long to decrease th chance of this happening
                            //      but for now, we just count up the frames and end the loop if that happens
                            if (reqHas4wayFlag && (k < (playerInputLen - 1)))
                            {
                                //TODO: god, this is a mess, try to make this less of a mess

                                //check the release/press flags of the next item
                                //we check to make sure that the next item (for if required press) a release of a valid item, and that that item's held duration is 0
                                bool k2CheckHasFlags = (playerInputs[k + 1].HoldDuration == 0) && EnumHelper.HasEnum((uint)playerInputs[k + 1].Flags, (uint)(lookForFlag ^ InputFlags.CHECK_CONTROLLER), true);
                                //      check the buttons
                                bool k2CheckInputBtn = k2CheckHasFlags && EnumHelper.HasEnum((uint)(playerInputs[k + 1].Input & InputEnum.BUTTONS), (uint)reqItem.Input, true);
                                //      check the directions, we do a lenient check if the required flags DOES NOT INCLUDE the 4way flag
                                bool k2CheckInput = k2CheckInputBtn && EnumHelper.HasEnum((uint)(playerInputs[k + 1].Input & InputEnum.DIRECTIONS), (uint)reqItem.Input, !reqHas4wayFlag);
                                // that wasn't a charge partition, so we end the loop
                                if (!k2CheckInput)
                                {

                                    //the overall check is judged by whether or not the charge duration is met
                                    overallCheck = kHeldFrames >= reqItem.HoldDuration;
                                    //end the loop
                                    break;
                                }

                            }
                            //check the held frames and end the loop
                            else
                            {
                                //the overall check is judged by whether or not the charge duration is met
                                overallCheck = kHeldFrames >= reqItem.HoldDuration;
                                //end the loop
                                break;
                            }
                        }
                    }
                }



                //did the input match what we're looking for?
                if (overallCheck)
                {
                    //found a matched input

                    //increment the index of the required input
                    j++;
                    //did we match every input?
                    bool completed = j >= requiredInputLen;

                    if (completed)
                    {
                        //we did, return true
                        return true;
                    }

                    //we reach this only if we haven't yet matched every input, set new required input item to check against
                    reqItem = this._requiredInputs[j];
                    //deincrement i so that when we increment i later, it's a net swing of 0
                    //  this is so that we don't move the player input index if we got a matching input
                    //  so that one item can contribute as much as it can
                    //  but only if the index is nonzero, since 0 is the controller state
                    i -= (int)EnumHelper.isNotZero((uint)i);
                }

                //increment the player inputs
                i++;
            }



            //only reached if we have not met the required inputs
            return false;

        }
    }
}