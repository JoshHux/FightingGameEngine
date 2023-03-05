using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class ApplyRsrcEvent : ICommand
    {
        private ResourceData _resources;
        public ApplyRsrcEvent(ResourceData rs)
        {
            this._resources = rs;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            //UnityEngine.Debug.Log("adding rsrc");
            lobj.AddCurrentResources(this._resources);
        }
    }
}