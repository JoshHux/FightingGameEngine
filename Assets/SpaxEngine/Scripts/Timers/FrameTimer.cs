namespace FightingGameEngine
{
    [System.Serializable]
    public class FrameTimer
    {
        [UnityEngine.SerializeField] private int _timeElapsed;
        [UnityEngine.SerializeField] private int _endTime;

        public int TimeElapsed { get { return this._timeElapsed; } }
        public int EndTime { get { return this._endTime; } }

        public FrameTimer()
        {
            this._endTime = 0;
            this._timeElapsed = 0;
        }


        public FrameTimer(int endTime)
        {
            this._endTime = endTime;
            this._timeElapsed = 0;
        }

        public FrameTimer(int endTime, int elapsedTime)
        {
            this._endTime = endTime;
            this._timeElapsed = elapsedTime;
        }

        //ticks timer forward 1 frame, returns true if timer is not finished
        public bool TickTimer()
        {
            //this._timeElapsed++;
            //a >= check so that we only tick up the timer if the end time is nonzero
            var timerCheck = this._timeElapsed < this._endTime;
            //the same check as the line above, but doing it this way makes it so that time elapsed only increments
            var incrementCheck = (timerCheck) && (++this._timeElapsed < this._endTime);

            //var incrementCheck = ((this._timeElapsed + 0) < this._endTime) && (++this._timeElapsed < this._endTime);


            var ret = incrementCheck;


            //only here to run OnTriggerEnd ONLY when the timer ticks AND the timer ends
            var timerEnd = timerCheck && (this._timeElapsed == this._endTime) && this.OnTimerEnd();

            return ret;
        }

        //are we done ticking? true if 
        public bool IsDone()
        {
            var ret = this._timeElapsed >= this._endTime;

            return ret;
        }


        public int GetTimeRemaining()
        {
            var ret = this._endTime - this._timeElapsed;
            return ret;
        }

        protected virtual bool OnTimerEnd() { return true; }
    }
}