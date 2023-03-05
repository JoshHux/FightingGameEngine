using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    [System.Serializable]
    public class ArmorEvent : ICommand
    {
        private int _armorHits;
        public ArmorEvent(int a)
        {
            this._armorHits = a;
        }

        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data)
        {
            (lobj as VulnerableObject).set_armor_hits(this._armorHits);
        }
    }
}