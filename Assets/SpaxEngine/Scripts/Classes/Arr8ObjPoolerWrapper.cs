using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Data;
using FightingGameEngine.ObjectPooler;

[System.Serializable]
public class Arr8ObjPoolerWrapper
{
    private Arr8<PoolItem> _arr;

    public PoolItem _0;
    public PoolItem _1;
    public PoolItem _2;
    public PoolItem _3;
    public PoolItem _4;
    public PoolItem _5;
    public PoolItem _6;
    public PoolItem _7;



    public Arr8ObjPoolerWrapper()
    {
        this._arr = new Arr8<PoolItem>();
        this._arr[0] = new PoolItem();
    }

    public Arr8<PoolItem> get_arr8() { return this._arr; }
    public void set_arr8(Arr8<PoolItem> a) { this._arr = a; }
#if UNITY_EDITOR
    public void ApplyGuiVal()
    {
        this._arr[0] = this._0;
        this._arr[1] = this._1;
        this._arr[2] = this._2;
        this._arr[3] = this._3;
        this._arr[4] = this._4;
        this._arr[5] = this._5;
        this._arr[6] = this._6;
        this._arr[7] = this._7;
    }
#endif
}
