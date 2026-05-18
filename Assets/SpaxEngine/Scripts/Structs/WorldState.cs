using System.IO;
using PleaseResync;
namespace FightingGameEngine.Data
{

    [System.Serializable]
    public struct WorldState : IGameState
    {
        public GameplayState Player1;
        public GameplayState Player2;

        public WorldState(in CharStateInfo[] chars)
        {
            this.Player1 = new GameplayState(chars[0]);
            this.Player2 = new GameplayState(chars[1]);
        }
        public void Setup() { }
        public void GameLoop(byte[] playerInput) { }
        public void SaveState(BinaryWriter bw) { }
        public void LoadState(BinaryReader br) { }
        public void Render() { }
        public byte[] GetLocalInput(int PlayerID, int InputSize) { return new byte[0]; }

    }
}