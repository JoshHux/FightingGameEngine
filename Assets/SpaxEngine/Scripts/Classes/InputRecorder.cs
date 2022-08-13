using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class InputRecorder
    {
        //current controller state, yet to be buffered
        [SerializeField] private InputItem _controllerState;
        //last controller state that was buffered
        [SerializeField] private InputItem _previousControllerState;
        [SerializeField] private List<InputItem> _recordedChanges;

        private int _lenLimit = 200;

        public InputItem CurrentControllerState { get { return this._controllerState; } set { this._controllerState = value; } }

        public InputRecorder()
        {
            this._controllerState = new InputItem();
            this._previousControllerState = new InputItem();
            this._recordedChanges = new List<InputItem>();
            //add default value to list, needs at least 1 element to make buffer behave
            this._recordedChanges.Add(new InputItem());
        }

        public void ResetLeniency()
        {
            int lastInd = this._recordedChanges.Count - 1;
            var toReplace = this._recordedChanges[lastInd];

            var newThing = new InputItem(toReplace.Input, toReplace.Flags, false, toReplace.HoldDuration);

            this._recordedChanges[lastInd] = newThing;
        }

        //returns true if new element is added to vector or if the framesHeld on that element is <=leniencey
        //difference is calculated by using controllerState
        public bool BufferInput(bool bufferLeniency)
        {

            //Debug.Log("buffering input | leniency? :: " + bufferLeniency);

            //get the last buffered change
            //index of last buffered
            int recordedLen = this._recordedChanges.Count;
            int lastInd = recordedLen - 1;
            var lastBuffered = this._recordedChanges[lastInd];
            //InputEnum of last buffered input changes
            var lastEnum = lastBuffered.Input;

            //get inputs released
            var releasedInputs = this._previousControllerState.GetInputsLost(this._controllerState);

            //if there were inputs released
            if (releasedInputs != 0)
            {
                //get whether or not we the previous buffered change is the same as this one
                var noChangeFlag = (InputFlags)(EnumHelper.HasEnumInt((uint)releasedInputs, (uint)lastEnum) << 2);
                //total flags we apply to the new InputItem to be buffered, released tag since we're releasing this input
                var totalFlags = noChangeFlag | InputFlags.RELEASED;

                //the InputItem to add
                var toAdd = new InputItem(releasedInputs, totalFlags, bufferLeniency);

                //if too meany inputs remove oldest item
                //if no input was in last enum, remove last enum

                if ((this._recordedChanges.Count >= this._lenLimit) || (lastEnum == 0)) { this._recordedChanges.RemoveAt(0); lastInd -= 1; }


                //add the new changes to the end of the list
                this._recordedChanges.Add(toAdd);

                //added new changes, assign new lastBuffered (useful for future)
                lastBuffered = toAdd;

                //increment lastInd, USEFUL LATER
                lastInd += 1;

                //change the lastEnum, since we just added the last input changes
                lastEnum = releasedInputs;
            }

            //get inputs released
            var pressedInputs = this._controllerState.GetInputsLost(this._previousControllerState);

            //if there were inputs pressed
            if (pressedInputs != 0)
            {
                //get whether or not we the previous buffered change is the same as this one
                var noChangeFlag = (InputFlags)(EnumHelper.HasEnumInt((uint)pressedInputs, (uint)lastEnum) << 2);
                //total flags we apply to the new InputItem to be buffered, released tag since we're releasing this input
                var totalFlags = noChangeFlag | InputFlags.PRESSED;

                //the InputItem to add
                var toAdd = new InputItem(pressedInputs, totalFlags, bufferLeniency);

                if ((this._recordedChanges.Count >= this._lenLimit) || (lastEnum == 0)) { this._recordedChanges.RemoveAt(0); lastInd -= 1; }


                //add the new changes to the end of the list
                this._recordedChanges.Add(toAdd);

                //increment lastInd, USEFUL LATER
                lastInd += 1;

                //added new changes, assign new lastBuffered (useful for future)
                lastBuffered = toAdd;
            }
            //lastBuffered.LenientBuffer = bufferLeniency;

            //increment the hold duration of the last item in the list
            //don't ask why I'm doing it like this, I don't even want to know, I'm tired
            lastBuffered.HoldDuration +=1;
            this._recordedChanges[lastInd] = lastBuffered;

            //TODO: *IF* processing inputs takes too long, add method here to hard limit the number of input changes we have 

            //the previous controller state is now the current controller state
            this._previousControllerState = this._controllerState;

            //we return true, IF
            //  last added element has a hold duration LESS than the static input leniency
            //  OR the last added element has LenientBuffer set to true
            bool durCheck = lastBuffered.HoldDuration < Spax.SpaxManager.Instance.StaticValues.InputBuffer;
            bool leniencyCheck = lastBuffered.LenientBuffer;

            //FINALLY, we set the or condition and return
            var ret = durCheck || leniencyCheck;

            return ret;
        }

        //returns the list of player inputs, with a backwards order and the current controller state as the first element
        public InputItem[] GetInputs()
        {
            var controllerState = this._controllerState;
            //controller state should only have the CHECK_CONTROLLER flag
            controllerState.Flags = InputFlags.CHECK_CONTROLLER;


            //check if we should return the rest of the inputs
            bool sendInputs = true;
            int lastInd = this._recordedChanges.Count - 1;
            int secondLastInd = lastInd - 1;

            if (lastInd > 0)
            {
                var firstItem = this._recordedChanges[lastInd];
                var secondItem = this._recordedChanges[secondLastInd];

                int inputBufferLeniency = Spax.SpaxManager.Instance.StaticValues.InputBuffer;

                sendInputs = (firstItem.HoldDuration <= inputBufferLeniency) || (firstItem.LenientBuffer && (secondItem.HoldDuration <= inputBufferLeniency));
            }

            //list to return
            var hold = new List<InputItem>();

            //if we want to send inputs
            if (sendInputs)
            {
                //copy the list into a seperate list, this so we can add the controller state to the end, flip it and turn it into an array
                hold = new List<InputItem>(this._recordedChanges);
            }

            //Debug.Log(hold.Count);

            //add the controller state to the end
            hold.Add(controllerState);

            //reverse the order of hold
            hold.Reverse();

            //now, turn it into an array and return
            var ret = hold.ToArray();


            return ret;
        }
    }
}