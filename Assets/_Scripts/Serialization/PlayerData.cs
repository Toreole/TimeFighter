namespace Game.Serialization
{
    [System.Serializable]
    public class PlayerData
    {
        //simple information.
        public string currentLevel;
        public Float3 position;
        //inventory, gamestate, "objectives", etc.
        //public SerializeInventory inventory;
        //public GameState state;
        //public SerializeObjectives objectives;
    }

}
