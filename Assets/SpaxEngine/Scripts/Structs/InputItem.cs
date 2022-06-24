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
        [UnityEngine.HideInInspector] public bool LenientBuffer;

        public InputItem(InputEnum input)
        {
            this.Input = input;
            this.Flags = 0;
            this.HoldDuration = 0;
            this.LenientBuffer = false;
        }

        public InputItem(InputEnum input, InputFlags flags, bool lenBuf)
        {
            this.Input = input;
            this.Flags = flags;
            this.HoldDuration = 0;
            this.LenientBuffer = lenBuf;
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
    }
}