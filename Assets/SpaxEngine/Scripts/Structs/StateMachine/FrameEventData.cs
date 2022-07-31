using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    public struct FrameEventData
    {
        public int Duration;
        public CancelConditions CancelsConditions;
        public StateConditions StateConditions;

    }
}