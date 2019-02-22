using UnityEngine;
using System.Collections.Generic;

namespace Game.Controller
{
    [System.Serializable]
    public class PlayerInput
    {
        [System.Serializable]
        public struct InputAxis
        {
            public static implicit operator bool (InputAxis input)
            {
                if (string.IsNullOrEmpty(input.name))
                    return false;
                return true;
            }

            public string name;
            public KeyCode positive, negative;
        }

        [SerializeField]
        protected List<InputAxis> axes;

        /// <summary>
        /// Returns the value of the axis.
        /// if the positive key is pressed => 1
        /// negative => -1
        /// both => 0
        /// </summary>
        public int GetAxis(string name)
        {
            var axis = axes.Find(x => x.name == name);
            if(!axis)
            {
                Debug.Log("There exists no axis called " +  name);
                return 0;
            }
            return Input.GetKey(axis.positive)? (Input.GetKey(axis.negative)? 0 : 1) : (Input.GetKey(axis.negative)? -1 : 0);
        }
    }
}
