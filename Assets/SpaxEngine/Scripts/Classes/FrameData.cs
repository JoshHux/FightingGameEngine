using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Commands;

namespace FightingGameEngine.Data
{
    [System.Serializable]
    public class FrameData
    {
        //the minimum atframe value should be 1, if 0, then it's invalid
        [SerializeField] private int _atFrame;
        [SerializeField] private FrameEventData _timerEvents;
        [SerializeField, SerializeReference] private List<ICommand> _events;

        //the minimum atframe value should be 1, if 0, then it's invalid
        public int AtFrame { get { return this._atFrame; } }
        public FrameEventData TimerEvent { get { return this._timerEvents; } }

        public FrameData(int f)
        {
            this._atFrame = f;
            this._events = new List<ICommand>();

        }

        public void SetEvents(List<ICommand> lst)
        {
            //deep copy so that the next frame's commands don't override the previous one's
            this._events = new List<ICommand>(lst);
        }

        public List<ICommand> get_events()
        {

            return this._events;
        }
    }
}