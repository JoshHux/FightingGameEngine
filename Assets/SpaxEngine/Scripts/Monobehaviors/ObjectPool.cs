using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Data;

namespace FightingGameEngine.ObjectPooler
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance;

        private List<PoolItem> _p1Pool;
        private List<PoolItem> _p2Pool;
        [SerializeField] private List<List<GameObject>> _p1ObjPool;
        [SerializeField] private List<List<GameObject>> _p2ObjPool;


        void Awake()
        {
            ObjectPool.Instance = this;
            this._p1Pool = new List<PoolItem>();
            this._p2Pool = new List<PoolItem>();

            this._p1ObjPool = new List<List<GameObject>>();
            this._p2ObjPool = new List<List<GameObject>>();
            this._p1ObjPool.Add(new List<GameObject>());
            this._p1ObjPool.Add(new List<GameObject>());
            this._p1ObjPool.Add(new List<GameObject>());
            this._p1ObjPool.Add(new List<GameObject>());
            this._p1ObjPool.Add(new List<GameObject>());
            this._p1ObjPool.Add(new List<GameObject>());
            this._p1ObjPool.Add(new List<GameObject>());
            this._p1ObjPool.Add(new List<GameObject>());

            this._p2ObjPool.Add(new List<GameObject>());
            this._p2ObjPool.Add(new List<GameObject>());
            this._p2ObjPool.Add(new List<GameObject>());
            this._p2ObjPool.Add(new List<GameObject>());
            this._p2ObjPool.Add(new List<GameObject>());
            this._p2ObjPool.Add(new List<GameObject>());
            this._p2ObjPool.Add(new List<GameObject>());
            this._p2ObjPool.Add(new List<GameObject>());
        }

        //call to init pool
        public void MakePool(int i, Arr8<PoolItem> proj)
        {
            for (int j = 0; j < 8; j++)
            {
                var hold = proj[j];
                if (hold.get_pooled_object() != null)
                {
                    hold.DeactivatedAll();
                    if (i == 0)
                    {
                        this._p1Pool.Add(hold);
                    }
                    else if (i == 1)
                    {
                        this._p2Pool.Add(hold);
                    }

                    for (int k = 0; k < hold.get_max_amount(); k++)
                    {
                        var go = Instantiate(hold.get_pooled_object(), this.transform.position, this.transform.rotation, this.transform);
                        go.SetActive(false);

                        if (i == 0)
                        {
                            this._p1ObjPool[j].Add(go);
                        }
                        else if (i == 1)
                        {
                            this._p2ObjPool[j].Add(go);
                        }
                    }
                }
            }
        }
    }
}