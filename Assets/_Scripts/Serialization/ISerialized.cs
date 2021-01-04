namespace Game.Serialization
{
    public interface ISerialized
    {
        string ObjectID {get; set;}
        void Deserialize(ObjectData data);
        //Save the objects data.
        ObjectData Serialize();
    }
}