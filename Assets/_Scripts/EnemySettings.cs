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
    }
}