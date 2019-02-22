using UnityEngine;
using System.Collections;

namespace Game
{
    /// <summary>
    /// Extra info about a type of enemy.
    /// </summary>
    [CreateAssetMenu]
    public class EnemySettings : ScriptableObject
    {
        [SerializeField]
        float timeGain;
        [SerializeField]
        int hp;
        [SerializeField]
        float turnSpeed;
        [SerializeField]
        MovementPattern movement;
        [SerializeField]
        float movementSpeed;
        [SerializeField]
        float wanderDistance;

#if UNITY_EDITOR
        public int HP { get { return hp; } set { hp = value; } }
        public float TimeGain { get { return timeGain; } set { timeGain = value; } }
        public float TurnSpeed { get { return turnSpeed; } set { turnSpeed = value; } }
        public float MovementSpeed { get { return movementSpeed; } set { movementSpeed = value; } }
        public MovementPattern Movement { get { return movement; } set { movement = value; } }
        public float WanderDistance { get { return wanderDistance; } set { wanderDistance = value; } }
#else
        public int   HP { get { return hp; } }
        public float TimeGain { get { return timeGain; } }
        public float TurnSpeed { get { return turnSpeed; } }
        public float MovementSpeed { get { return movementSpeed; } }
        public MovementPattern Movement { get { return movement; } }
        public float WanderDistance { get { return wanderDistance; } }
#endif
    }
}