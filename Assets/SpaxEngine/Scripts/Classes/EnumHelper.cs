namespace FightingGameEngine.Enum
{
    public static class EnumHelper
    {
        //checks to see if val has the enums
        public static bool HasEnum(uint val, uint compare)
        {
            return EnumHelper.HasEnum(val, compare, false);
        }

        //if strict is true, it will check to see if the whole enum is in val, not just if it exists
        //otherwise, it will only check for if any of the enum exists in val
        public static bool HasEnum(uint val, uint compare, bool strict)
        {

            if (compare == 0) { return true; }
            if (strict)
            {
                return (val & compare) == compare;
            }

            return (val & compare) > 0;
        }

        //checks to see if val has the compare enums
        //NOTE: compare should ALWAYS be nonzero
        public static int HasEnumInt(uint val, uint compare)
        {
            if (compare == 0) { return 0; }
            //should be 0 if enum isn't there and 1 if it is
            uint check = (val & compare) / compare;
            int ret = (int)check;
            return ret;
        }

        //checks to see if val has the compare enums
        public static int HasEnumInt(int val, int compare)
        {
            //should be 0 if enum isn't there and 1 if it is
            int ret = (val & compare) / compare;
            return ret;
        }

        //from: https://stackoverflow.com/questions/3912112/check-if-a-number-is-non-zero-using-bitwise-operators-in-c
        //branchless check for nonzero I found online
        //  it or's the flip of unisgned n, if I understand this right, ~n+1 always equals a nonzero value only if n is nonzero as well
        //  it makes sure that the 32nd bit always has a 1, so only returns 0 if (n | (~n + 1)) is 0
        //  overall, seems to work
        public static uint isNotZero(uint n)
        { // unsigned is safer for bit operations
            return ((n | (~n + 1)) >> 31) & 1;
        }

        //SPECIFICALLY for InputEnum, flips to the correct direction depending on the input int
        //  -1 for looking left and 1 for looking right
        public static InputEnum FaceDir(int dir, InputEnum inputEnum)
        {

            //if dir is negative, this is 1
            uint dirIsNeg = ((uint)dir) >> 31;

            //direction component of input
            var inputDir = inputEnum & InputEnum.DIRECTIONS;

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
            var ret = (inputEnum & InputEnum.BUTTONS) | newDir;



            return ret;
        }
    }
}
