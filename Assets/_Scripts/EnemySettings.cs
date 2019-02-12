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

        public float TimeGain
        {
            get
            {
                return timeGain;
            }
#if UNITY_EDITOR
            set
            {
                timeGain = value;
            }
#endif
        }

        public int HP
        {
            get
            {
                return hp;
            }
#if UNITY_EDITOR
            set
            {
                hp = value;
            }
#endif
        }
        public float TurnSpeed
        {
            get
            {
                return turnSpeed;
            }
#if UNITY_EDITOR
            set
            {
                turnSpeed = value;
            }
#endif
        }

        public MovementPattern Movement
        {
            get
            {
                return movement;
            }
#if UNITY_EDITOR
            set
            {
                movement = value;
            }
#endif
        }
        public float MovementSpeed
        {
            get
            {
                return movementSpeed;
            }
#if UNITY_EDITOR
            set
            {
                movementSpeed = value;
            }
#endif
        }
    }
}