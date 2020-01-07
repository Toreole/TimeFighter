using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Misc
{
    [Serializable]
    public class AnimatorParameterLink
    {
        /// <summary>
        /// the name of the parameter in the animator
        /// </summary>
        public string paramName;
        public AnimatorParameterLink()
        {
            paramName = "";
        }
    }
    [Serializable]
    public class AnimatorParameterLink<T> : AnimatorParameterLink where T : Enum
    {
        public T purpose; 
    }
}
