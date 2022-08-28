using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class TransitionData
    {
        [SerializeField] private soStateData _targetState;

        [SerializeField] private int _targetStateIndex = -1;
        [SerializeField] private int _targetUniversalStateIndex = -1;
        //only relevant if character has a positive y-velocity
        [SerializeField] private Fix64 _minimumHeight;
        [SerializeField] private CancelConditions _cancelConditions;

        [SerializeField] private InputItem[] _requiredInputs;
        [SerializeField] private TransitionFlags _requiredTransitionFlags;
        [SerializeField] private TransitionEvents _transitionEvents;
        [SerializeField] private ResourceData _requiredResources;

        public soStateData TargetState { get { return this._targetState; } }
        public int TargetStateIndex { get { return this._targetStateIndex; } }
        public int TargetUniversalStateIndex { get { return this._targetUniversalStateIndex; } }
        public TransitionEvents TransitionEvents { get { return this._transitionEvents; } }
        public CancelConditions RequiredCancels { get { return this._cancelConditions; } }
        public TransitionFlags RequiredTransitionFlags { get { return this._requiredTransitionFlags; } }
        public ResourceData RequiredResources { get { return this._requiredResources; } }


        public bool CheckTransition(TransitionFlags curFlags, CancelConditions curCan, ResourceData curResources, InputItem[] playerInputs, int facingDir, Fix64 yVel, Fix64 yPos)
        {
            var transCancels = this._cancelConditions;
            var transFlags = this._requiredTransitionFlags;
            var transRsrc = this._requiredResources;


            bool checkCancels = EnumHelper.HasEnum((uint)curCan, (uint)transCancels, true);
            bool checkFlags = checkCancels && EnumHelper.HasEnum((uint)curFlags, (uint)transFlags, true);
            bool checkHeight = checkFlags && ((this._minimumHeight == 0) || (yVel > 0 && yPos >= this._minimumHeight) || (yVel < 0));
            bool checkResources = checkHeight && (EnumHelper.HasEnum((uint)this._transitionEvents, (uint)TransitionEvents.DODGE_RESOURCE_CHECK) || transRsrc.Check(curResources));
            bool checkInputs = checkResources && ((this._requiredInputs.Length == 0) || ((playerInputs.Length > 0) && this.CheckInputs(playerInputs, facingDir)));

            return checkInputs;
        }

        private bool CheckInputs(InputItem[] playerInputs, int facingDir)
        {

            int playerInputLen = playerInputs.Length;
            int requiredInputLen = this._requiredInputs.Length;

            //if no required inputs, just return a true
            //if (requiredInputLen == 0) { return true; }
            //if no player inputs, no pass the check
            //else if (playerInputLen == 0) { return false; }


            //index for the player's inputs
            int i = 0;
            //index for the required inputs
            int j = 0;

            //current item we are checking against
            //we start with the first item in the required items
            var reqItem = this._requiredInputs[j];
            //how many frames it's been since the last valid input
            int sinceLastMatch = 0;


            /*-----PREPPING REQUIRED INPUT INFO-----*/

            //required hold duration
            var reqHoldDur = reqItem.HoldDuration;
            //prepping req input data for easy use
            var reqInput = reqItem.Input;
            //required input flags, removed flags that cannot be applied to player inputs
            uint reqFlagsRaw = (uint)reqItem.Flags;
            //required input flags, removed flags that cannot be applied to player inputs
            uint reqFlags = (reqFlagsRaw & (~(uint)InputFlags.BACKEND_FLAGS));
            //get the solo button and direction input
            uint reqBtn = (uint)(reqInput & InputEnum.BUTTONS);
            uint reqDir = (uint)(reqInput & InputEnum.DIRECTIONS);

            //amount of input leniency allowed
            int inputLeniency = Spax.SpaxManager.Instance.StaticValues.InputBuffer;
            //int inputBuffer = Spax.SpaxManager.Instance.StaticValues.InputBuffer;

            //check against the list of player inputs
            while (i < playerInputLen)
            {
                //current player input item we're looking at
                var curPlayerItem = playerInputs[i].GetDirection(facingDir);

                //current result of the overall check of the input
                bool overallCheck = false;

                //should we skip adding the time lingered?
                //only applied to the second item in the array (that's the first item checked for the input requirements [that aren't controller checks])
                bool skipIncrement = (i == 1) && curPlayerItem.LenientBuffer;

                //if we ignore the time elapsed, don't add the hold duration
                if (!skipIncrement)
                {
                    //Debug.Log("adding this input's duration - " + curPlayerItem.HoldDuration + " | i - " + i);
                    //add the amount of frames the player lingered on this input
                    sinceLastMatch += curPlayerItem.HoldDuration;
                }
                //else
                //{
                //    Debug.Log("skipping this input's duration " + i + " | " + curPlayerItem.LenientBuffer + " | " + curPlayerItem.Input + " | " + curPlayerItem.Flags + " | " + curPlayerItem.HoldDuration);
                //}

                //     check if there's a hold durations
                bool reqHold = (reqHoldDur > 0);

                //if the number of frames passed since the last valid input is more than the input leniency, then break
                bool tooLongSinceLastInput = /*(j > 0) &&(!reqHold) &&*/ (sinceLastMatch > inputLeniency);

                //player's buttons
                uint playerItemBtn = (uint)(curPlayerItem.Input & InputEnum.BUTTONS);
                //player's direction
                uint playerItemDir = (uint)(curPlayerItem.Input & InputEnum.DIRECTIONS);
                //player's flags
                uint playerItemFlags = (uint)curPlayerItem.Flags;


                if (tooLongSinceLastInput)
                {
                    //Debug.Log("too long since last valid input - " + i);
                    return false;
                }
                //else if (this._targetState != null && this._targetState.name == "Prejump") { Debug.Log(i + " | passed - " + curPlayerItem.HoldDuration + " " + sinceLastMatch + "/" + inputLeniency + " | " + reqFlags + " | " + playerItemBtn + " " + playerItemFlags); }

                //check the items
                //  check if the flags match
                bool checkHasFlags = EnumHelper.HasEnum(playerItemFlags, reqFlags, true);

                //      this makes it so that we still set it to true if we don't require a hold
                //          but if we do, we force a fail
                var checkHasFlags2 = (!reqHold) && checkHasFlags;

                //      check for the 4way direction flag
                bool reqHas4wayFlag = EnumHelper.HasEnum(reqFlagsRaw, (uint)InputFlags.DIR_4WAY, true);

                //      check for the controller state doesn't have these inputs
                bool reqHasUpFlag = EnumHelper.HasEnum(reqFlagsRaw, (uint)InputFlags.CHECK_IS_UP, true);

                //      check for the controller state for any of there inputs
                bool reqHasAnyDown = EnumHelper.HasEnum(reqFlagsRaw, (uint)InputFlags.ANY_IS_DOWN, true);


                //are we checking if these inputs are currently up?
                //      we only check if we're looking at the first item in the player's input (the controller state)
                bool checkUp = (i == 0) && reqHasUpFlag;


                //  we check if the inputs match
                //      check the buttons
                //      we're so a lenient check if we're checking for if this input is not being pressed
                //          this is because if ANY of the inputs are registered, then we gotta fail it
                //          so we gotta turn the check into a fail flag if checkUp is true
                bool checkInputBtn = EnumHelper.HasEnum(playerItemBtn, reqBtn, reqHasAnyDown || !checkUp) != checkUp;
                //      check the directions, we do a lenient check if the required flags DOES NOT INCLUDE the 4way flag
                bool checkInput = checkInputBtn && (EnumHelper.HasEnum(playerItemDir, reqDir, reqHasAnyDown || !(reqHas4wayFlag || checkUp)) != checkUp);

                //for now, just set the overall check to the flag and overall input check
                overallCheck = checkHasFlags2 && checkInput;

                //if (this._targetState != null && this._targetState.name == "FlashKick" && j > 0)
                //{
                //    Debug.Log("overall check :: " + overallCheck + " | required lenient direction :: " + (!(reqHas4wayFlag || checkUp)) + " | 4way :: " + EnumHelper.HasEnum(reqFlagsRaw, (uint)InputFlags.DIR_4WAY, true) + " | check up :: " + checkUp);
                //    Debug.Log("required flags :: " + reqFlagsRaw + " | required buttons :: " + reqBtn + " | required direction :: " + reqDir);
                //    Debug.Log("player flags :: " + playerItemFlags + " | player buttons :: " + playerItemBtn + " | player direction :: " + playerItemDir);
                //    Debug.Log("passed input leniency check | flag check :: " + checkHasFlags + " | input check :: " + checkInput + " | direction check :: " + (EnumHelper.HasEnum((uint)playerItemDir, (uint)reqDir, (!(reqHas4wayFlag || checkUp)))));
                //    Debug.Log("button check - p :: " + playerItemBtn + " | r :: " + reqBtn + " - " + checkInputBtn);
                //}

                //  extra checks
                //      check charge and hold inputs
                //          we only check if the initial button and direction check is passed
                //          the hold duration of the input is > 0
                //          AND the current item has the correct buttons and directions we're looking for
                //          AND the current check failed
                bool checkHold = !overallCheck && checkHasFlags && checkInput && reqHold;
                if (checkHold)
                {
                    //Debug.Log("checking hold | player input len - " + playerInputLen + " | i - " + i);
                    //we need to fast forward throught the rest of the inputs, we're looking for a press/release of the required item
                    //we start at i+1 since we already checked the item at i
                    int k = i + 1;
                    //filter the flags to make looking for the press/release we're looking for
                    var lookForFlag = InputFlags.PRESSED;
                    var lookForFlag2 = InputFlags.RELEASED;

                    //amount of frames held
                    int kHeldFrames = 0;
                    //Debug.Log("overall check :: " + overallCheck + " | required lenient direction :: " + (!(reqHas4wayFlag || checkUp)) + " | 4way :: " + EnumHelper.HasEnum(reqFlagsRaw, (uint)InputFlags.DIR_4WAY, true) + " | check up :: " + checkUp);
                    //Debug.Log("required flags :: " + reqFlagsRaw + " | required buttons :: " + reqBtn + " | required direction :: " + reqDir);
                    //Debug.Log("player flags :: " + playerItemFlags + " | player buttons :: " + playerItemBtn + " | player direction :: " + playerItemDir);
                    //Debug.Log("passed input leniency check | flag check :: " + checkHasFlags + " | input check :: " + checkInput + " | direction check :: " + (EnumHelper.HasEnum((uint)playerItemDir, (uint)reqDir, (!(reqHas4wayFlag || checkUp)))));
                    //Debug.Log("button check - p :: " + playerItemBtn + " | r :: " + reqBtn + " - " + checkInputBtn);

                    //-1 is so we dodge the first neutral item
                    while ((k < (playerInputLen)) && (kHeldFrames < reqHoldDur))
                    {
                        //current player input item we're looking at
                        var kCurPlayerItem = playerInputs[k];


                        //player's buttons
                        var kPlayerItemBtn = kCurPlayerItem.Input & InputEnum.BUTTONS;
                        //player's direction
                        var kPlayerItemDir = kCurPlayerItem.Input & InputEnum.DIRECTIONS;

                        //check the release/press flag
                        bool kCheckHasFlags = EnumHelper.HasEnum((uint)kCurPlayerItem.Flags, (uint)lookForFlag, true);

                        //      check the buttons
                        bool kCheckInputBtn = EnumHelper.HasEnum((uint)kPlayerItemBtn, reqBtn, true);
                        //      check the directions, we do a lenient check if the required flags DOES INCLUDE the 4way flag
                        bool kCheckInput = kCheckHasFlags && kCheckInputBtn && EnumHelper.HasEnum((uint)kPlayerItemDir, reqDir, !reqHas4wayFlag);

                        //if we found a released flag we immediately want to end the loop, released means that we ended the charge WITHOUT adding the duration of that input
                        bool kCheckHasReleasedFlags = EnumHelper.HasEnum((uint)kCurPlayerItem.Flags, (uint)lookForFlag2, true) && kCheckInputBtn && EnumHelper.HasEnum((uint)kPlayerItemDir, reqDir, !reqHas4wayFlag);

                        if (kCheckHasReleasedFlags) { break; }

                        //no reason to break the charge, add the amount of frames held
                        kHeldFrames += kCurPlayerItem.HoldDuration;

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
                                //new player input stuff
                                //current player input item we're looking at
                                var k2CurPlayerItem = playerInputs[k + 1];
                                //player's flags
                                var k2PlayerItemFlg = k2CurPlayerItem.Flags;
                                //player's direction
                                var k2PlayerItemDir = kCurPlayerItem.Input & InputEnum.DIRECTIONS;

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
                                bool prevHasPressed = !(EnumHelper.HasEnum((uint)k2PlayerItemFlg, (uint)InputFlags.RELEASED) && EnumHelper.HasEnum((uint)k2PlayerItemDir, reqDir, !reqHas4wayFlag));

                                bool breakCharge = stickGoesTo5 || prevHasPressed;

                                // that wasn't a charge partition, so we end the loop
                                if (breakCharge)
                                {
                                    //Debug.Log("broken charge");
                                    //the overall check is judged by whether or not the charge duration is met
                                    overallCheck = kHeldFrames >= reqHoldDur;
                                    //end the loop
                                    break;
                                }

                            }
                            //check the held frames and end the loop
                            else
                            {
                                //Debug.Log("check made it this far");
                                //the overall check is judged by whether or not the charge duration is met
                                overallCheck = kHeldFrames >= reqHoldDur;
                                //end the loop
                                break;
                            }
                        }
                        k++;
                    }
                    //Debug.Log("ending loop - " + kHeldFrames + "/" + reqItem.HoldDuration);

                    overallCheck = kHeldFrames >= reqHoldDur;
                }




                //did the input match what we're looking for?
                if (overallCheck)
                {
                    //Debug.Log("found a matching input - " + j + " in " + this._targetState.name);
                    //found a matched input

                    //increment the index of the required input
                    j++;
                    //did we match every input?
                    bool completed = j >= requiredInputLen;

                    if (completed)
                    {
                        //Debug.Log("found transition to - " + this._targetState.name);
                        //we did, return true
                        return true;
                    }

                    //if we completed this item and it's NOT a check controller state, then we change the input leniency to the leniency instead of the buffer'
                    //  this is so that we avoid a scenario where inputs with leniency don't cause a scneario where the inputs before it have 10 frames of lenie3ncy when they shouldn't
                    bool reqHasCheck = EnumHelper.HasEnum(reqFlagsRaw, (uint)InputFlags.CHECK_CONTROLLER, true);
                    if (!reqHasCheck) { inputLeniency = Spax.SpaxManager.Instance.StaticValues.InputLeniency; }



                    //we reach this only if we haven't yet matched every input, set new required input item to check against
                    reqItem = this._requiredInputs[j];

                    //required hold duration
                    reqHoldDur = reqItem.HoldDuration;
                    //prepping req input data for easy use
                    reqInput = reqItem.Input;
                    //required input flags, removed flags that cannot be applied to player inputs
                    reqFlagsRaw = (uint)reqItem.Flags;
                    reqFlags = (uint)(reqItem.Flags & (~InputFlags.BACKEND_FLAGS));
                    //get the solo button and direction input
                    reqBtn = (uint)(reqInput & InputEnum.BUTTONS);
                    reqDir = (uint)(reqInput & InputEnum.DIRECTIONS);

                    //reset the time passed so far
                    sinceLastMatch = 0;

                    //deincrement i so that when we increment i later, it's a net swing of 0
                    //  this is so that we don't move the player input index if we got a matching input
                    //  so that one item can contribute as much as it can
                    //  but only if the index is nonzero, since 0 is the controller state
                    i -= (int)EnumHelper.isNotZero((uint)i);
                }
                //if the input is lenient, AND it failed to complete the item, add the duration to the amount of time passed
                else if (i == 1 && curPlayerItem.LenientBuffer)
                {
                    sinceLastMatch += curPlayerItem.HoldDuration;
                }

                //increment the player inputs
                i++;
            }



            //only reached if we have not met the required inputs
            return false;

        }
    }
}