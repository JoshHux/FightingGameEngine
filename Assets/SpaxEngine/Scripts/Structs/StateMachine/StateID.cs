[System.Serializable]
public struct StateID
{
    //4 smallest bytes correspont to char combination that is unique to each character, the other bits describe the states's index in that character's list
    //the sign bit indicates whether or not this is a universal state, positive for non universal state and negative for universals state
    [UnityEngine.SerializeField] public long ID;

#if UNITY_EDITOR
    public void SetIndex(long index)
    {
        var mask = (long)-1;

        long hold = ID & mask;
        long add = (index << 32);

        long newID = add | hold;

        this.ID = newID;
    }

    public void SetAsUniversal()
    {
        this.ID |= long.MinValue;
    }
#endif
    public bool IsUniversal()
    {
        return (this.ID & long.MinValue) > 0;
    }

    //returns the index bits of ID
    public long GetIndex() { return this.ID >> 32; }

    //returns a string of character that corresponds to the character(aka. the first 4 bytes)
    public static string CharString(StateID id)
    {
        var hold = id.ID;

        char char1 = (char)(hold & 0b11111111);
        char char2 = (char)((hold >> 8) & 0b11111111);
        char char3 = (char)((hold >> 16) & 0b11111111);
        char char4 = (char)((hold >> 24) & 0b11111111);

        char[] word = { char1, char2, char3, char4 };
        var ret = new string(word);

        return ret;
    }

    //returns long by converting first 4 characters of string to longs and performing bit operations
    public static long FromString(string charName)
    {

        long char1 = (long)charName[0];
        long char2 = (long)charName[1];
        long char3 = (long)charName[2];
        long char4 = (long)charName[3];

        long ret = (char4 << 24) | (char3 << 16) | (char2 << 8) | char1;

        //UnityEngine.Debug.Log(char4 + ", " + char3 + ", " + char1 + ", " + char1 + " | " + ret);

        return ret;
    }



    public static bool operator ==(StateID a, StateID b)
    {
        return a.ID == b.ID;
    }
    public static bool operator !=(StateID a, StateID b)
    {
        return a.ID != b.ID;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
