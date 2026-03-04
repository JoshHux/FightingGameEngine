using UnityEngine;
using FixMath.NET;
using FightingGameEngine.Enum;
using UnityEditor.Rendering;
using System.Linq;

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


        //which buttons need to be pressed/released on the controller state
        [SerializeField] private InputItem _requiredControllerState;
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

        public TransitionData(soStateData targetState)
        {
            this._targetState = targetState;
            this._targetStateIndex = -1;
            this._targetUniversalStateIndex = -1;
            this._minimumHeight = 0;

            this._cancelConditions = 0;
            this._requiredTransitionFlags = 0;
            this._requiredResources = new ResourceData();
        }


        public bool CheckTransition(TransitionFlags curFlags, CancelConditions curCan, ResourceData curResources, in InputItem[] playerInputs, InputSnapshot curSnapshot, int facingDir, Fix64 yVel, Fix64 yPos)
        {
            var transCancels = this._cancelConditions;
            var transFlags = this._requiredTransitionFlags;
            var transRsrc = this._requiredResources;

            //if (this._targetState.name == "cATLN_Walk6") { Debug.Log(curCan + " || " + transCancels); }

            bool checkCancels = EnumHelper.HasEnum((uint)curCan, (uint)transCancels, true);
            bool checkFlags = checkCancels && EnumHelper.HasEnum((uint)curFlags, (uint)transFlags, true);
            bool checkHeight = checkFlags && ((this._minimumHeight == 0) || (yVel > 0 && yPos >= this._minimumHeight) || (yVel < 0));
            bool checkResources = checkHeight && (EnumHelper.HasEnum((uint)this._transitionEvents, (uint)TransitionEvents.DODGE_RESOURCE_CHECK) || transRsrc.Check(curResources));
            bool checkInputs = checkResources && ((playerInputs.Length > 0) && this.CheckInputs(playerInputs, curSnapshot, facingDir));
            //if (this._targetState.name == "cATLN_Walk6") { Debug.Log(checkInputs); }
            return checkInputs;
        }

        private bool CheckInputs(in InputItem[] playerInputs, InputSnapshot curSnapshot, int facingDir)
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






            ///===== CHECK THE CONTROLLER STATE =====///
            //if we're checking the controller's state, let's check the first requiered input input and start at index 1

            //check against the current controller state

            //  make sure we edit the controller state to face the correct direction
            var correctedControllerState = curSnapshot.FaceDir(facingDir);

            //  are the right inputs being pressed?
            var checkPressed = ((uint)this._requiredControllerState.PressedInput == 0) || EnumHelper.HasEnum((uint)correctedControllerState.InputStates, (uint)this._requiredControllerState.PressedInput, true);
            //  check that the correct inputs are released
            //      we do this by seeing if ANY of the released inputs are in the current controller state,
            //      if they are then that means that we have unwanted inputs pressed
            var checkReleased = ((uint)this._requiredControllerState.ReleasedInput == 0) || !EnumHelper.HasEnum((uint)correctedControllerState.InputStates, (uint)this._requiredControllerState.ReleasedInput);

            //we failed the check return false
            if (!(checkPressed && checkReleased)) { return false; }
            //if (this._targetState.name == "cATLN_Walk6") { Debug.Log(((uint)this._requiredControllerState.PressedInput == 0) + " || " + ((uint)this._requiredControllerState.ReleasedInput == 0)); }

            if (this._requiredInputs.Count() == 0) { return true; }




            //current item we are checking against
            //we start with the first item in the required items
            var reqItem = this._requiredInputs[j];




            //how many frames it's been since the last valid input
            int sinceLastMatch = 0;


            /*-----PREPPING REQUIRED INPUT INFO-----*/

            //required hold duration
            var reqHoldDur = reqItem.HoldDuration;
            //prepping req input data for easy use
            var reqInputPressed = reqItem.PressedInput;
            var reqInputReleased = reqItem.ReleasedInput;
            //required input flags, removed flags that cannot be applied to player inputs
            uint reqFlagsRaw = (uint)reqItem.Flags;
            //required input flags, removed flags that cannot be applied to player inputs
            uint reqFlags = (reqFlagsRaw & (~(uint)InputFlags.BACKEND_FLAGS));
            //get the solo button and direction input
            //  pressed
            uint reqPresBtn = (uint)(reqInputPressed & InputEnum.BUTTONS);
            uint reqPresDir = (uint)(reqInputPressed & InputEnum.DIRECTIONS);
            //  released
            uint reqReleBtn = (uint)(reqInputReleased & InputEnum.BUTTONS);
            uint reqReleDir = (uint)(reqInputReleased & InputEnum.DIRECTIONS);

            //amount of input leniency allowed
            int inputBuffer = Spax.SpaxManager.Instance.StaticValues.InputBuffer;
            int inputLeniency = Spax.SpaxManager.Instance.StaticValues.InputLeniency;

            //check the controller state
            bool passControllerStateCheck = EnumHelper.HasEnum((uint)curSnapshot.InputStates, (uint)this._requiredControllerState.PressedInput, true);
            //if (this._targetState.name == "cATLN_Walk6") { Debug.Log("first input :: " + playerInputs[i].PressedInput); }

            //check against the list of player inputs
            while (i < playerInputLen)
            {
                var playerInputItem = playerInputs[i].FaceDir(facingDir);

                //is the input too old?
                sinceLastMatch += playerInputItem.HoldDuration;

                //the buffer was already check back when the input item list was made
                //  we're checking is the input is too old, meaning that it is NOT the first player item AND is older than the leniency
                if (((i > 0) && (sinceLastMatch > inputLeniency)) || ((i == 0) && (sinceLastMatch > inputBuffer)))
                {
                    //if (this._targetState.name == "cATLN_Walk6") { Debug.Log("input too old :: " + sinceLastMatch + " / " + inputBuffer); }


                    return false;
                }


                ///===== GRAB THE PLAYER INPUT VARIABLES =====///
                //okay! we passed that

                //get the solo button and direction input
                //  pressed
                uint playerPresBtn = (uint)(playerInputItem.PressedInput & InputEnum.BUTTONS);
                uint playerPresDir = (uint)(playerInputItem.PressedInput & InputEnum.DIRECTIONS);
                //  released
                uint playerReleBtn = (uint)(playerInputItem.ReleasedInput & InputEnum.BUTTONS);
                uint playerReleDir = (uint)(playerInputItem.ReleasedInput & InputEnum.DIRECTIONS);


                ///===== CHECK THE INPUTS NOW =====///
                //  check flags
                var pressed4Way = EnumHelper.HasEnum((uint)reqFlags, (uint)InputFlags.DIR_4WAY);

                //check pressed inputs
                //  buttons
                var checkPressedBtn = EnumHelper.HasEnum((uint)playerPresBtn, (uint)reqPresBtn);
                //  direction is a little weird, if we have the DIR_4WAY flag then we want to put this as a lenient input
                var checkPressedDir = EnumHelper.HasEnum((uint)playerPresDir, (uint)reqPresDir, !pressed4Way);


                //check released inputs
                //  buttons
                var checkReleasedBtn = EnumHelper.HasEnum((uint)playerReleBtn, (uint)reqReleBtn);
                //  see above
                var checkReleasedDir = EnumHelper.HasEnum((uint)playerReleDir, (uint)reqReleDir, !pressed4Way);


                //check if the input has passed the checks
                var passedItemChecks = checkPressedBtn && checkPressedDir && checkReleasedBtn && checkReleasedDir;

                if (passedItemChecks)
                {
                    j++;

                    ///------- SUCCESS!!! --------///
                    //if we reached the end of the list then we passed all checks and should return true
                    if (j >= this._requiredInputs.Count()) { return true; }

                    ///===== WE STILL HAVE MORE REQUIRED INPUTS =====///
                    //reset the variables
                    reqItem = this._requiredInputs[j];


                    //required hold duration
                    reqHoldDur = reqItem.HoldDuration;
                    //prepping req input data for easy use
                    reqInputPressed = reqItem.PressedInput;
                    reqInputReleased = reqItem.ReleasedInput;
                    //required input flags, removed flags that cannot be applied to player inputs
                    reqFlagsRaw = (uint)reqItem.Flags;
                    //required input flags, removed flags that cannot be applied to player inputs
                    reqFlags = (reqFlagsRaw & (~(uint)InputFlags.BACKEND_FLAGS));
                    //get the solo button and direction input
                    //  pressed
                    reqPresBtn = (uint)(reqInputPressed & InputEnum.BUTTONS);
                    reqPresDir = (uint)(reqInputPressed & InputEnum.DIRECTIONS);
                    //  released
                    reqReleBtn = (uint)(reqInputReleased & InputEnum.BUTTONS);
                    reqReleDir = (uint)(reqInputReleased & InputEnum.DIRECTIONS);

                    //since we matched, reset the time since we found a match
                    sinceLastMatch = 0;
                }
                else
                {
                    //increment the player inputs ONLY IF we didn't match yet
                    //  otherwise we want to see if this frame has inputs that match other required items
                    i++;
                }
            }
            //Debug.Log("somehow failed");


            //only reached if we have not met the required inputs
            return false;

        }
    }
}