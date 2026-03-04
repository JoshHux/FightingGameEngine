using System.Security;
using FightingGameEngine.Enum;
using UnityEngine.InputSystem.EnhancedTouch;


namespace FightingGameEngine.Data
{
    //state of the controller at any given point
    [System.Serializable]
    public struct InputSnapshot
    {
        public InputEnum InputStates;

        public int HoldDuration;
        public InputSnapshot(InputEnum inputs)
        {
            this.InputStates = inputs;
            this.HoldDuration = 0;
        }

        public InputSnapshot(InputEnum inputs, int duration)
        {
            this.InputStates = inputs;
            this.HoldDuration = duration;
        }

        public static InputItem DiffFromNew(InputSnapshot oldSnapshot, InputSnapshot newSnapshot)
        {
            var pressedInputs = (oldSnapshot.InputStates ^ newSnapshot.InputStates) & newSnapshot.InputStates;
            var releasedInputs = (oldSnapshot.InputStates ^ newSnapshot.InputStates) & oldSnapshot.InputStates;

            var ret = new InputItem(pressedInputs, releasedInputs);
            return ret;
        }


        //returns true if the x-axis is being pressed
        public bool WantsToMove()
        {
            var ret = EnumHelper.HasEnum((uint)this.InputStates, (uint)InputEnum.X_NONZERO);

            return ret;
        }

        //returns -1 or 1 depending on x-axis
        public int X()
        {
            //returns 1 if input has either left or right input
            //negX is -1 because we add posX and negX for the return
            int negX = -1 * (int)EnumHelper.isNotZero((uint)(this.InputStates & InputEnum.X_NEGATIVE));
            int posX = (int)EnumHelper.isNotZero((uint)(this.InputStates & InputEnum.X_POSITIVE));

            //posX and negX both cannot have a nonzero value because of input stuff
            //so one has to be 0, so the total will be positive if right input and negative if right input
            var ret = negX + posX;

            return ret;
        }

        public InputSnapshot FaceDir(int dir)
        {
            return new InputSnapshot(EnumHelper.FaceDir(dir, this.InputStates), this.HoldDuration);

        }
    }
}