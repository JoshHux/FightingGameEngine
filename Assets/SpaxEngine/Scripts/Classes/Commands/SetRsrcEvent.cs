using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class SetRsrcEvent : ICommand
    {
    private ResourceData _resources;
        public SetRsrcEvent(ResourceData rs)
        {
            this._resources=rs;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            lobj.SetCurrentResources(this._resources);
        }
    }
}