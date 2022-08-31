namespace FightingGameEngine.Data
{
    
    [System.Serializable]
    public struct TimerInfo
    {
        public int TimeElapsed;
        public int EndTime;

        public TimerInfo(FrameTimer timer)
        {
            this.TimeElapsed = timer.TimeElapsed;
            this.EndTime = timer.EndTime;
        }
    }
}