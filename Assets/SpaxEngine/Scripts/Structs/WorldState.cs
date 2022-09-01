namespace FightingGameEngine.Data
{

    [System.Serializable]
    public struct WorldState
    {
        public GameplayState Player1;
        public GameplayState Player2;

        public WorldState(in soCharacterStatus p1, in soCharacterStatus p2)
        {
            this.Player1 = new GameplayState(p1);
            this.Player2 = new GameplayState(p2);
        }

    }
}