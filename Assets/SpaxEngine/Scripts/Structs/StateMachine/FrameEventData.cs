using FightingGameEngine.Enum;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    public struct FrameEventData
    {
        public CancelConditions ToggleCancels;
        public StateConditions ToggleStateConditions;
        public int Duration;
        public bool IsValid()
        {
            bool ret = this.Duration > 0;
            return ret;
        }
    }
}