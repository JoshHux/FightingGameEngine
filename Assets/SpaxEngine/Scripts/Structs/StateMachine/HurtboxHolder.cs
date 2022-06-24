namespace FightingGameEngine.Data
{
    [System.Serializable]
    public struct HurtboxHolder
    {

        public HurtboxData Hurtbox0;
        public HurtboxData Hurtbox1;
        public HurtboxData Hurtbox2;
        public HurtboxData Hurtbox3;
        public HurtboxData Hurtbox4;
        public HurtboxData Hurtbox5;
        public HurtboxData Hurtbox6;
        public HurtboxData Hurtbox7;
        public HurtboxData Hurtbox8;
        public HurtboxData Hurtbox9;


        public HurtboxData GetHurtbox(int i)
        {
            //default value is the first hitbox
            var ret = Hurtbox0;
            switch (i)
            {
                case 1:
                    ret = Hurtbox1;
                    break;
                case 2:
                    ret = Hurtbox2;
                    break;
                case 3:
                    ret = Hurtbox3;
                    break;
                case 4:
                    ret = Hurtbox4;
                    break;
                case 5:
                    ret = Hurtbox5;
                    break;
                case 6:
                    ret = Hurtbox6;
                    break;
                case 7:
                    ret = Hurtbox7;
                    break;
                case 8:
                    ret = Hurtbox8;
                    break;
                case 9:
                    ret = Hurtbox9;
                    break;
            }

            //return the value set to ret
            return ret;
        }
    }
}