namespace FightingGameEngine.Data
{
    [System.Serializable]
    public struct HitboxHolder
    {
        public HitboxData Hitbox0;
        public HitboxData Hitbox1;
        public HitboxData Hitbox2;
        public HitboxData Hitbox3;
        public HitboxData Hitbox4;
        public HitboxData Hitbox5;
        public HitboxData Hitbox6;
        public HitboxData Hitbox7;
        public HitboxData Hitbox8;
        public HitboxData Hitbox9;

        public HitboxData GetHitbox(int i)
        {
            //default value is the first hitbox
            var ret = Hitbox0;
            switch (i)
            {
                case 1:
                    ret = Hitbox1;
                    break;
                case 2:
                    ret = Hitbox2;
                    break;
                case 3:
                    ret = Hitbox3;
                    break;
                case 4:
                    ret = Hitbox4;
                    break;
                case 5:
                    ret = Hitbox5;
                    break;
                case 6:
                    ret = Hitbox6;
                    break;
                case 7:
                    ret = Hitbox7;
                    break;
                case 8:
                    ret = Hitbox8;
                    break;
                case 9:
                    ret = Hitbox9;
                    break;
            }

            //return the value set to ret
            return ret;
        }
    }
}