using UnityEngine;
using System.Collections;

namespace Game.Serialization
{
    public abstract class SerializedMonoBehaviour : MonoBehaviour
    {
        //this object's ID, not to be changed in the editor.
        protected string objectID = "";
        //get property, can be overridden if necessary. (like NPCs with custom IDs that persist across scenes)
        public string ObjectID { get; }
#if UNITY_EDITOR
        internal void OverrideID(string id) => objectID = id;
#endif
        //Load object with the data.
        public abstract void Deserialize(ObjectData data);
        //Save the objects data.
        public abstract ObjectData Serialize();
    }
}