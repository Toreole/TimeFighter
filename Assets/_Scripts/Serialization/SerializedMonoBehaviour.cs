using UnityEngine;
using System.Collections;

namespace Game.Serialization
{
    public abstract class SerializedMonoBehaviour : MonoBehaviour
    {
        //this object's ID, not to be changed in the editor.
        [HideInInspector]
        public string objectID = "";
        //Load object with the data.
        public abstract void Deserialize(ObjectData data);
        //Save the objects data.
        public abstract ObjectData Serialize();
    }
}