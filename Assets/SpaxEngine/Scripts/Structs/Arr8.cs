using MessagePack;

namespace FightingGameEngine.Data
{

    [System.Serializable]
    [MessagePackObject]

    public struct Arr8<T>
    {

        [Key(0)]
        public T _0;
        [Key(1)]
        public T _1;
        [Key(2)]
        public T _2;
        [Key(3)]
        public T _3;
        [Key(4)]
        public T _4;
        [Key(5)]
        public T _5;
        [Key(6)]
        public T _6;
        [Key(7)]
        public T _7;


        //https://stackoverflow.com/questions/424669/how-do-i-overload-the-operator-in-c-sharp
        public T this[int key]
        {
            get => GetValue(key);
            set => SetValue(key, value);
        }


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
                default:
                    this._0 = val;
                    break;
            }

        }
    }
}