using FightingGameEngine.Enum;

namespace FightingGameEngine
{

    [System.Serializable]
    public class ConditionTimer : FrameTimer
    {
        [UnityEngine.SerializeField] private StateConditions _stateConditions;
        public StateConditions StateConditions { get { return this._stateConditions; } }

        public ConditionTimer(int endTime, int elapsedTime, StateConditions cond) : base(endTime, elapsedTime)
        {
            this._stateConditions = cond;
        }
        public ConditionTimer(int endTime, StateConditions cond) : base(endTime)
        {
            this._stateConditions = cond;
        }
        public ConditionTimer() : base()
        {
            this._stateConditions = 0;
        }

        protected override bool OnTimerEnd()
        {
            this._stateConditions = 0;
            return true;
        }

    }
}