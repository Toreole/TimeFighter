using System.Collections.Generic;
using UnityEngine;

namespace Game.Controller
{
    [CreateAssetMenu]
    public class Throwable : ScriptableObject
    {
        [SerializeField]
        protected GameObject prefab;
        [SerializeField]
        protected int startAmount = 2;
        [SerializeField]
        protected int maxAmount = 3;
        [SerializeField]
        protected float startVelocity = 10f;

        public GameObject Prefab => prefab;
        public int StartAmount => startAmount;
        public int MaxAmount => maxAmount;
        public float StartVelocity => startVelocity;
    }
}