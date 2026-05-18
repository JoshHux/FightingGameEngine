using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class InputRecorder
    {
        //current controller state, yet to be buffered
        [SerializeField] private InputSnapshot _controllerState;
        //last controller state that was buffered
        [SerializeField] private InputSnapshot _previousControllerState;
        [SerializeField] private List<InputSnapshot> _recordedInputs;

        [SerializeField] private List<InputItem> _debugInputs;

        private int _lenLimit = 200;

        public InputSnapshot CurrentControllerState { get { return this._controllerState; } set { this._controllerState = value; } }

        public List<InputSnapshot> RecordedInputs { get { return this._recordedInputs; } set { this._recordedInputs = value; } }

        public InputRecorder()
        {
            this._controllerState = new InputSnapshot();
            this._previousControllerState = new InputSnapshot();
            this._recordedInputs = new List<InputSnapshot>();
            //add default value to list, needs at least 1 element to make buffer behave
            this._recordedInputs.Add(new InputSnapshot());
        }

        public void ResetLeniency()
        {
            int lastInd = this._recordedInputs.Count - 1;
            var toReplace = this._recordedInputs[lastInd];

            var newThing = new InputSnapshot(toReplace.InputStates, toReplace.HoldDuration);

            this._recordedInputs[lastInd] = newThing;
        }

        //returns true if new element is added to vector or if the framesHeld on that element is <=leniencey
        //difference is calculated by using controllerState
        public bool BufferInput(bool bufferLeniency)
        {

            //Debug.Log("buffering input | leniency? :: " + bufferLeniency);

            //get the last buffered change
            //index of last buffered
            int recordedLen = this._recordedInputs.Count;
            int lastInd = recordedLen - 1;
            var lastBuffered = this._recordedInputs[lastInd];
            //InputEnum of last buffered input changes
            var lastEnum = lastBuffered.InputStates;

            //buffer last input
            bool checkControllerState = lastEnum == this._controllerState.InputStates;

            if (!checkControllerState)
            {
                this._recordedInputs.Add(new InputSnapshot(this._controllerState.InputStates));
                lastBuffered = this._recordedInputs[lastInd];
            }

            //lastBuffered.LenientBuffer = bufferLeniency;

            //increment the hold duration of the last item in the list
            //don't ask why I'm doing it like this, I don't even want to know, I'm tired
            lastBuffered.HoldDuration += 1;
            //if we're in a situation where we shouldn't be lenient to inputs, increase unlenient time
            //if (!bufferLeniency) { lastBuffered.HoldDuration += 1; }
            this._recordedInputs[lastInd] = lastBuffered;

            //TODO: *IF* processing inputs takes too long, add method here to hard limit the number of input changes we have 

            //the previous controller state is now the current controller state
            this._previousControllerState = this._controllerState;

            //we return true, IF
            //  last added element has a hold duration LESS than the static input leniency
            //  OR the last added element has LenientBuffer set to true
            bool durCheck = lastBuffered.HoldDuration < Spax.SpaxManager.Instance.StaticValues.InputBuffer;
            bool leniencyCheck = true;// lastBuffered.LenientBuffer;

            //FINALLY, we set the or condition and return
            var ret = durCheck;
            return ret;
        }

        //returns the list of player inputs, with a backwards order and the current controller state as the first element
        public InputItem[] GetInputs()
        {




            //list to return
            var hold = new List<InputItem>();



            //start from the most recent input and go backwards
            int i = this._recordedInputs.Count - 2;

            //this is to make sure that there are no interruptions between the most recently pressed input and the pressed input before that  
            InputEnum prevItemPressed = 0;
            int indexOfLastPressed = -1;

            while (i >= 0)
            {
                var toAdd = InputSnapshot.DiffFromNew(this._recordedInputs[i], this._recordedInputs[i + 1]);
                //to edit later just need a way to check to uninterrupted inputs
                if (0 != prevItemPressed && toAdd.PressedInput == prevItemPressed)
                {
                    //Debug.Log("no interrupt detected :: " + prevItemPressed + " | " + toAdd.PressedInput);
                    hold[indexOfLastPressed] = new InputItem(hold[indexOfLastPressed].PressedInput, hold[indexOfLastPressed].ReleasedInput, InputFlags.NO_INTERRUPT, hold[indexOfLastPressed].HoldDuration);
                }

                //how long has it been since the change?
                //  answer: as long as the new input has been around
                toAdd.HoldDuration = this._recordedInputs[i + 1].HoldDuration;

                //if ((uint)this._recordedInputs[i].InputStates == 0) { toAdd.HoldDuration = this._recordedInputs[i].HoldDuration; }

                hold.Add(toAdd);

                if (toAdd.PressedInput != 0)
                {
                    prevItemPressed = toAdd.PressedInput;
                    indexOfLastPressed = this._recordedInputs.Count - i - 2;
                    //Debug.Log(prevItemPressed);
                }

                i--;
            }

            //now, turn it into an array and return
            var ret = hold.ToArray();

            this._debugInputs = hold;


            return ret;
        }
    }
}