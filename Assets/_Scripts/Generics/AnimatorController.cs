using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Misc;

namespace Game.Generics
{
    public abstract class AnimatorController<TParam, TEnum> : MonoBehaviour where TParam : AnimatorParameterLink<TEnum> where TEnum : Enum
    {
        [SerializeField]
        protected Entity entity;
        [SerializeField]
        protected Animator animator; 
        [SerializeField]
        protected List<TParam> parameters;

        /// <summary>
        /// The dictionary that maps the Enum values from the parameters to the IDs in the animator.
        /// </summary>
        protected Dictionary<TEnum, int> enumToID = new Dictionary<TEnum, int>();

        protected virtual void Awake()
        {
            //init the Dictionary
            foreach (var param in parameters)
                if(!enumToID.ContainsKey(param.purpose))
                    enumToID.Add(param.purpose, Animator.StringToHash(param.paramName));
        }

        public List<TParam> GetParams() => parameters; //bruh
    }
}
