using MessagePack;
namespace FightingGameEngine.Data
{

    [System.Serializable]
    [MessagePackObject]

    public struct TimerInfo
    {
        [Key(0)]
        public int TimeElapsed;
        [Key(1)]
        public int EndTime;

        public TimerInfo(FrameTimer timer)
        {
            this.TimeElapsed = timer.TimeElapsed;
            this.EndTime = timer.EndTime;
        }
    }
}