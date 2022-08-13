using FightingGameEngine.Enum;


namespace FightingGameEngine.Data
{
    [System.Serializable]
    public struct InputItem
    {
        public InputEnum Input;
        public InputFlags Flags;
        public int HoldDuration;
        //bool that tells whther or not we should ignore the buffer time when checking the input
        //only matters for the SECOND item in the input array
        //[UnityEngine.HideInInspector]
        public bool _lenientBuffer;

        public bool LenientBuffer { get { return this._lenientBuffer; } }

        public InputItem(InputEnum input)
        {
            this.Input = input;
            this.Flags = 0;
            this.HoldDuration = 0;
            this._lenientBuffer = false;
        }

        public InputItem(InputEnum input, InputFlags flags, bool lenBuf)
        {
            this.Input = input;
            this.Flags = flags;
            this.HoldDuration = 0;
            this._lenientBuffer = lenBuf;
        }

        public InputItem(InputEnum input, InputFlags flags, bool lenBuf, int holdDur)
        {
            this.Input = input;
            this.Flags = flags;
            this.HoldDuration = holdDur;
            this._lenientBuffer = lenBuf;
        }

        //returns true if the x-axis is being pressed
        public bool WantsToMove()
        {
            var ret = EnumHelper.HasEnum((uint)this.Input, (uint)InputEnum.X_NONZERO);

            return ret;
        }

        //returns -1 or 1 depending on x-axis
        public int X()
        {
            //returns 1 if input has either left or right input
            //negX is -1 because we add posX and negX for the return
            int negX = -1 * (int)EnumHelper.isNotZero((uint)(this.Input & InputEnum.X_NEGATIVE));
            int posX = (int)EnumHelper.isNotZero((uint)(this.Input & InputEnum.X_POSITIVE));

            //posX and negX both cannot have a nonzero value because of input stuff
            //so one has to be 0, so the total will be positive if right input and negative if right input
            var ret = negX + posX;

            return ret;
        }

        //returns the set of InputEnum that are lost from this InputItem to the other InputItem
        public InputEnum GetInputsLost(InputItem other)
        {
            var otherEnum = other.Input;

            //get the differences in the enum
            var diff = this.Input ^ otherEnum;

            //AND the differences so that we ONLY get the inputs that were lost from THIS Input Item's perspective
            var ret = diff & this.Input;

            return ret;

        }

        //returns an input item with a L/R possibly flipped
        public InputItem GetDirection(int dir)
        {
            //if dir is negative, this is 1
            uint dirIsNeg = ((uint)dir) >> 31;

            //direction component of input
            var inputDir = this.Input & InputEnum.DIRECTIONS;

            //gets the X-bits we want to check
            var xDir = inputDir & InputEnum.X_NONZERO;

            //step 1: xor directions 7,9 ; 4,6 ; 1,3
            var xorXUp = (xDir & InputEnum.X_NONZERO_UP) ^ InputEnum.X_NONZERO_UP;
            var xorXMid = (xDir & InputEnum.X_NONZERO_MID) ^ InputEnum.X_NONZERO_MID;
            var xorXLow = (xDir & InputEnum.X_NONZERO_LOW) ^ InputEnum.X_NONZERO_LOW;
            //step 2: divide by original respective x-nonzero values
            //      if the xor resulted in a bit getting lost, dividing by what was xor-ed would result in 0
            var divXUp = ((uint)xorXUp / (uint)InputEnum.X_NONZERO_UP);
            var divXMid = ((uint)xorXMid / (uint)InputEnum.X_NONZERO_MID);
            var divXLow = ((uint)xorXLow / (uint)InputEnum.X_NONZERO_LOW);
            //step 3: xor all the divided values by 1
            //      this is so that, for the divisions that resulted in 1, they become 0 and 0 becomes 1
            var multXUp = (uint)divXUp ^ 1;
            var multXMid = (uint)divXMid ^ 1;
            var multXLow = (uint)divXLow ^ 1;
            //step 4: multiply the xor in step 1 with the respective values we find in step 4
            //      this is so that we only keep the parts of the x-nonzero mask that lost bits (AKA the bits we want to flip along the y axis)
            var partXUp = (InputEnum)((uint)InputEnum.X_NONZERO_UP * multXUp);
            var partXMid = (InputEnum)((uint)InputEnum.X_NONZERO_MID * multXMid);
            var partXLow = (InputEnum)((uint)InputEnum.X_NONZERO_LOW * multXLow);


            //step 5: construct the final mask
            var dirMask = partXUp | partXMid | partXLow;

            //if the direction is positive, multiply by 0 and the mask does nothing
            var finalMask = (InputEnum)((uint)dirMask * dirIsNeg);

            //xor the final mask (directions 8,5,2 all end up with the mask as 0, so they don't effect the end direction)
            var newDir = inputDir ^ finalMask;
            var newInput = (this.Input & InputEnum.BUTTONS) | newDir;


            //create the new input based on the this item ans return
            var ret = new InputItem(newInput, this.Flags, this.LenientBuffer, this.HoldDuration);

            return ret;
        }
    }
}