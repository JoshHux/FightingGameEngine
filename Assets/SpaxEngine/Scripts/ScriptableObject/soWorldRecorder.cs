using System.Collections.Generic;
using UnityEngine;

namespace FightingGameEngine.Data
{
    [CreateAssetMenu(fileName = "WorldStates", menuName = "WorldStates", order = 1)]
    public class soWorldRecorder : ScriptableObject
    {
        [SerializeField] private List<WorldState> _worldStates;

        public void Init()
        {
            this._worldStates = new List<WorldState>();
        }

        public void End() { this._worldStates.Clear(); }
        public WorldState GetWorldState(int i)
        {
            return this._worldStates[i];
        }

        public void AddWorldState(WorldState newState)
        {
            this._worldStates.Add(newState);
        }


    }
}