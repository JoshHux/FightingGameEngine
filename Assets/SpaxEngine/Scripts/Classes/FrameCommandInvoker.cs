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
            this._cmndQueue = lst;
        }

        public void ExecuteCommands(in LivingObject lobj)
        {
            var status = lobj.Status;
            var data = lobj.Data;

            int i = 0;
            int len = this._cmndQueue.Count;

            do
            {
                this._cmndQueue[i].Execute(lobj, lobj.Status, lobj.Data);

                i++;
            } while (i < len);

        }
    }
}