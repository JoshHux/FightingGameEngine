using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    public struct FrameEventData
    {
        public int Duration;
        public CancelConditions ToggleCancels;
        public StateConditions ToggleStateConditions;
        public bool IsValid()
        {
            bool ret = this.Duration > 0;
            return ret;
        }
    }
}