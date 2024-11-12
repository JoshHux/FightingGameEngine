using System.Collections.Generic;
using FightingGameEngine.Gameplay;
namespace FightingGameEngine.Commands
{
    public class FrameCommandInvoker
    {
        private List<ICommand> _cmndQueue;

        public FrameCommandInvoker()
        {
            this._cmndQueue = new List<ICommand>();
        }

        public void set_comn_queue(List<ICommand> lst)
        {
            this._cmndQueue = new List<ICommand>(lst);
        }

        public void ExecuteCommands(in LivingObject lobj)
        {
            int len = this._cmndQueue.Count;
            //UnityEngine.Debug.Log(len);

            if (len == 0) { return; }

            var status = lobj.Status;
            var data = lobj.Data;

            int i = 0;

            do
            {
                var hold = this._cmndQueue[i];
                UnityEngine.Debug.Log(hold.GetType());
                hold.Execute(lobj, lobj.Status, lobj.Data);

                i++;
            } while (i < len);

            this._cmndQueue.Clear();

        }
    }
}