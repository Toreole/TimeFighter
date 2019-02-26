using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu]
    public class Throwable : ScriptableObject
    {
        [SerializeField]
        protected internal GameObject prefab;
        [SerializeField]
        protected internal int startAmount = 2;
        [SerializeField]
        protected internal int maxAmount = 3;
        [SerializeField]
        protected internal float startVelocity = 10f;
    }
}