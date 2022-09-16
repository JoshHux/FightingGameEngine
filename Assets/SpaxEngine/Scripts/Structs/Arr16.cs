
namespace FightingGameEngine.Data
{

    [System.Serializable]
    public struct Arr16<T>
    {
        public T _0;
        public T _1;
        public T _2;
        public T _3;
        public T _4;
        public T _5;
        public T _6;
        public T _7;
        public T _8;
        public T _9;
        public T _10;
        public T _11;
        public T _12;
        public T _13;
        public T _14;
        public T _15;

        public T GetValue(int i)
        {
            T ret = this._0;
            switch (i)
            {
                case 1:
                    ret = this._1;
                    break;
                case 2:
                    ret = this._2;
                    break;
                case 3:
                    ret = this._3;
                    break;
                case 4:
                    ret = this._4;
                    break;
                case 5:
                    ret = this._5;
                    break;
                case 6:
                    ret = this._6;
                    break;
                case 7:
                    ret = this._7;
                    break;
                case 8:
                    ret = this._8;
                    break;
                case 9:
                    ret = this._9;
                    break;
                case 10:
                    ret = this._10;
                    break;
                case 11:
                    ret = this._11;
                    break;
                case 12:
                    ret = this._12;
                    break;
                case 13:
                    ret = this._13;
                    break;
                case 14:
                    ret = this._14;
                    break;
                case 15:
                    ret = this._15;
                    break;
            }

            return ret;
        }

        public void SetValue(int i, T val)
        {
            switch (i)
            {
                case 1:
                    this._1 = val;
                    break;
                case 2:
                    this._2 = val;
                    break;
                case 3:
                    this._3 = val;
                    break;
                case 4:
                    this._4 = val;
                    break;
                case 5:
                    this._5 = val;
                    break;
                case 6:
                    this._6 = val;
                    break;
                case 7:
                    this._7 = val;
                    break;
                case 8:
                    this._8 = val;
                    break;
                case 9:
                    this._9 = val;
                    break;
                case 10:
                    this._10 = val;
                    break;
                case 11:
                    this._11 = val;
                    break;
                case 12:
                    this._12 = val;
                    break;
                case 13:
                    this._13 = val;
                    break;
                case 14:
                    this._14 = val;
                    break;
                case 15:
                    this._15 = val;
                    break;
                default:
                    this._0 = val;
                    break;
            }

        }

    }
}