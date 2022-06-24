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
    }
}
