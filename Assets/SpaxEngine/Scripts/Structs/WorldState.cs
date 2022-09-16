namespace FightingGameEngine.Data
{

    [System.Serializable]
    public struct WorldState
    {
        public GameplayState Player1;
        public GameplayState Player2;

        public WorldState(in CharStateInfo[] chars)
        {
            this.Player1 = new GameplayState(chars[0]);
            this.Player2 = new GameplayState(chars[1]);
        }

    }
}