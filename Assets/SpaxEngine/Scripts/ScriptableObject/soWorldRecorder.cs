using System.Collections.Generic;
using UnityEngine;

namespace FightingGameEngine.Data
{
    [CreateAssetMenu(fileName = "WorldStates", menuName = "WorldStates", order = 1)]
    public class soWorldRecorder : ScriptableObject
    {
        [SerializeField] private List<WorldState> _worldStates;
        [SerializeField] private List<InputSnapshot> _p1Inputs;
        [SerializeField] private List<InputSnapshot> _p2Inputs;
        public List<InputSnapshot> P1Inputs { get { return this._p1Inputs; } set { this._p1Inputs = value; } }
        public List<InputSnapshot> P2Inputs { get { return this._p2Inputs; } set { this._p2Inputs = value; } }

        public int WorlStateCount { get { return this._worldStates.Count; } }


        public void Init()
        {
            this._worldStates = new List<WorldState>();
            this._p1Inputs = null;
            this._p2Inputs = null;
        }

        public void End() { this._worldStates.Clear(); }
        public WorldState GetWorldState(int i)
        {
            return this._worldStates[i];
        }

        public int GetWorldStateCount() { return this._worldStates.Count; }

        public void AddWorldState(WorldState newState)
        {
            this._worldStates.Add(newState);
        }


    }
}