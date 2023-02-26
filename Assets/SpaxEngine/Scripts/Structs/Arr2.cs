
namespace FightingGameEngine.Data
{

    [System.Serializable]

    public struct Arr2<T>
    {
        public T _0;
        public T _1;

        //https://stackoverflow.com/questions/424669/how-do-i-overload-the-operator-in-c-sharp
        public T this[int key]
        {
            get => GetValue(key);
            set => SetValue(key, value);
        }

        public T GetValue(int i)
        {
            T ret = this._0;
            if (i == 1)
            {
                ret = this._1;
            }


            return ret;
        }

        public void SetValue(int i, T val)
        {
            if (i == 1)
            {
                this._1 = val;
            }
            else
            {
                this._0 = val;
            }
        }
    }
}
