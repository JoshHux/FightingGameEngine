using UnityEngine;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class TransitionData
    {
        [SerializeField] private soStateData _targetState;
        [SerializeField] private CancelConditions _cancelConditions;

        [SerializeField] private InputItem[] _requiredInputs;
        [SerializeField] private TransitionFlags _requiredTransitionFlags;
        [SerializeField] private TransitionEvents _transitionEvents;
        [SerializeField] private ResourceData _requiredResources;

        public soStateData TargetState { get { return this._targetState; } }
        public TransitionEvents TransitionEvents { get { return this._transitionEvents; } }
        public CancelConditions RequiredCancels { get { return this._cancelConditions; } }
        public TransitionFlags RequiredTransitionFlags { get { return this._requiredTransitionFlags; } }
        public ResourceData RequiredResources { get { return this._requiredResources; } }

        public bool CheckInputs(InputItem[] playerInputs)
        {

            int playerInputLen = playerInputs.Length;
            int requiredInputLen = this._requiredInputs.Length;

            //if no required inputs, just return a true
            if (requiredInputLen == 0) { return true; }
            //if no player inputs, no pass the check
            else if (playerInputLen == 2) { return false; }
            //Debug.Log("checking inputs in transition");

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
                bool skipIncrement = (i == 1) && curPlayerItem.LenientBuffer;

                //if we ignore the time elapsed, don't add the hold duration
                if (!skipIncrement)
                {
                    //Debug.Log("adding this input's duration");
                    //add the amount of frames the player lingered on this input
                    sinceLastMatch += curPlayerItem.HoldDuration;
                }
                //else
                //{
                //    Debug.Log("skipping this input's duration");
                //}

                //if the number of frames passed since the last valid input is more than the input leniency, then break
                bool tooLongSinceLastInput = /*(j > 0) &&*/ (sinceLastMatch > Spax.SpaxManager.Instance.StaticValues.InputLeniency);
                if (tooLongSinceLastInput) { break; }


                //player's buttons
                var playerItemBtn = curPlayerItem.Input & InputEnum.BUTTONS;
                //player's direction
                var playerItemDir = curPlayerItem.Input & InputEnum.DIRECTIONS;

                //check the items
                //  check if the flags match
                bool checkHasFlags = EnumHelper.HasEnum((uint)curPlayerItem.Flags, (uint)(reqItem.Flags & (~InputFlags.DIR_4WAY)), true);
                //      check if there's a hold durations
                bool reqHold = (reqItem.HoldDuration > 0);
                //      this makes it so that we still set it to true if we don't require a hold
                //          but if we do, we force a fail
                var checkHasFlags2 = (!reqHold) && checkHasFlags;

                //      check for the 4way direction flag
                bool reqHas4wayFlag = EnumHelper.HasEnum((uint)reqItem.Flags, (uint)InputFlags.DIR_4WAY, true);


                //  we check if the inputs match
                //      check the buttons
                bool checkInputBtn = EnumHelper.HasEnum((uint)playerItemBtn, (uint)(reqItem.Input & InputEnum.BUTTONS), true);
                //      check the directions, we do a lenient check if the required flags DOES NOT INCLUDE the 4way flag
                bool checkInput = checkInputBtn && EnumHelper.HasEnum((uint)playerItemDir, (uint)(reqItem.Input & InputEnum.DIRECTIONS), !reqHas4wayFlag);

                //for now, just set the overall check to the flag and overall input check
                overallCheck = checkHasFlags2 && checkInput;

                //if (this._targetState.name == "Jab")
                //{
                //Debug.Log("required flags :: " + reqItem.Flags);
                //Debug.Log("passed input leniency check | flag check :: " + checkHasFlags + " | direction check :: " + EnumHelper.HasEnum((uint)(reqItem.Input & InputEnum.DIRECTIONS), (uint)playerItemDir, !reqHas4wayFlag));
                //}

                //  extra checks
                //      check charge and hold inputs
                //          we only check if the initial button and direction check is passed
                //          the hold duration of the input is > 0
                //          AND the current item has the correct buttons and directions we're looking for
                //          AND the current check failed
                bool checkHold = checkHasFlags && checkInput && reqHold && !overallCheck;
                if (checkHold)
                {
                    Debug.Log("checking hold");
                    //we need to fast forward throught the rest of the inputs, we're looking for a press/release of the required item
                    //we start at i+1 since we already checked the item at i
                    int k = i + 1;
                    //filter the flags to make looking for the press/release we're looking for
                    var lookForFlag = InputFlags.PRESSED;

                    //amount of frames held
                    int kHeldFrames = 0;

                    //-1 is so we dodge the first neutral item
                    while (k < (playerInputLen - 1))
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
                        bool kCheckHasFlags = EnumHelper.HasEnum((uint)kCurPlayerItem.Flags, (uint)lookForFlag, true);

                        //      check the buttons
                        bool kCheckInputBtn = EnumHelper.HasEnum((uint)kPlayerItemBtn, (uint)(reqItem.Input & InputEnum.BUTTONS), true);
                        //      check the directions, we do a lenient check if the required flags DOES INCLUDE the 4way flag
                        bool kCheckInput = kCheckHasFlags && kCheckInputBtn && EnumHelper.HasEnum((uint)kPlayerItemDir, (uint)(reqItem.Input & InputEnum.DIRECTIONS), !reqHas4wayFlag);

                        //Debug.Log("hold check loop - " + k + " | adding : " + kCurPlayerItem.HoldDuration);
                        //Debug.Log("button check :: " + kCheckInputBtn + " | flag check :: " + kCheckHasFlags + " | direction check :: " + EnumHelper.HasEnum((uint)kPlayerItemDir, (uint)(reqItem.Input & InputEnum.DIRECTIONS), !reqHas4wayFlag) + " | 4way? :: " + reqHas4wayFlag);
                        //  we found an input that should stop the loop
                        if (kCheckInput)
                        {
                            //Debug.Log("potential input to break the hold");
                            //TODO: try to reorder the if/else statements to make it so that we only end the loop in one if/else statement instead of 2

                            //if 4way flag is on the required item, we check the next item to make sure the release/press direction is still valid
                            //  this is to allow for charge partitioning
                            //  for this process, we have a strict leniency of 0 frames, since that's the hold duration of the release for charge partitioning

                            //  NOTE: THIS FAILS IF WE REACH THE END OF THE LIST AND THE LAST ITEM WAS A PRESS, THIS IS BECAUSE WE CANNOT CHECK THE NEXT ITEM TO MAKE SURE IT'S VALID
                            //      the workaround for this is to make the list of player inputs super long to decrease th chance of this happening
                            //      but for now, we just count up the frames and end the loop if that happens
                            if (reqHas4wayFlag && (k < playerInputLen - 2))
                            {
                                //Debug.Log("potential break");
                                //TODO: god, this is a mess, try to make this less of a mess

                                //so, this part only applies to a directional charge
                                //the charge is broken IF:
                                //      the previous input's directionn is not what we want

                                //reasons to break charge:
                                //    1) stick returns to neutral
                                //          indicated by the current released input's hold durations > 0
                                bool stickGoesTo5 = kCurPlayerItem.HoldDuration > 0;
                                //    2) charge is NOT being partitioned
                                //          indicated by the current input being a press, but the released input IS part of the 4-way (if has tag)
                                bool prevHasPressed = !(EnumHelper.HasEnum((uint)playerInputs[k + 1].Flags, (uint)InputFlags.RELEASED) && EnumHelper.HasEnum((uint)(playerInputs[k + 1].Input & InputEnum.DIRECTIONS), (uint)(reqItem.Input & InputEnum.DIRECTIONS), !reqHas4wayFlag));

                                bool breakCharge = stickGoesTo5 || prevHasPressed;

                                // that wasn't a charge partition, so we end the loop
                                if (breakCharge)
                                {
                                    //Debug.Log("broken charge");
                                    //the overall check is judged by whether or not the charge duration is met
                                    overallCheck = kHeldFrames >= reqItem.HoldDuration;
                                    //end the loop
                                    break;
                                }

                            }
                            //check the held frames and end the loop
                            else
                            {
                                //Debug.Log("check made it this far");
                                //the overall check is judged by whether or not the charge duration is met
                                overallCheck = kHeldFrames >= reqItem.HoldDuration;
                                //end the loop
                                break;
                            }
                        }
                        k++;
                    }
                    //Debug.Log("ending loop - " + kHeldFrames + "/" + reqItem.HoldDuration);

                    overallCheck = kHeldFrames >= reqItem.HoldDuration;
                }



                //did the input match what we're looking for?
                if (overallCheck)
                {
                    //Debug.Log("found a matching input - " + j);
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

                    //reset the time passed so far
                    sinceLastMatch = 0;
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