using UnityEngine;
using FightingGameEngine.Gameplay;
using FightingGameEngine.Data;
namespace FightingGameEngine.Commands
{
    //[System.Serializable]
    public interface ICommand
    {
        public void Execute(in LivingObject lobj, in soCharacterStatus status, in soCharacterData data);
    }
}