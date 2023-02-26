using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FightingGameEngine.ObjectPooler
{
    [System.Serializable]
    public class PoolItem
    {
        [SerializeField] private GameObject _pooledObject;
        [SerializeField] private int _maxAmount;
        [SerializeField] private int _activeCount;

        public PoolItem()
        {
            this._pooledObject = null;
            this._maxAmount = 0;
            this._activeCount = 0;
        }
        public PoolItem(GameObject obj, int ma, int ac)
        {
            this._pooledObject = obj;
            this._maxAmount = ma;
            this._activeCount = ac;
        }

        public GameObject get_pooled_object() { return this._pooledObject; }
        public int get_max_amount() { return this._maxAmount; }
        public int get_active_count() { return this._activeCount; }

        public void ObjectActivated()
        {
            this._activeCount += 1;
            if (this._activeCount > this._maxAmount) { this._activeCount = this._maxAmount; }
        }
        public void ObjectDeactivated()
        {
            this._activeCount = 1;
            if (this._activeCount < 0) { this._activeCount = 0; }
        }

        public void ActivatedAll()
        {
            this._activeCount = this._maxAmount;
        }


        public void DeactivatedAll()
        {
            this._activeCount = 0;
        }
    }
}