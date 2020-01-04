using UnityEngine;
using System.Collections;

namespace Game
{
    /// <summary>
    /// The base class for all enemies
    /// </summary>
    [System.Obsolete]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : Entity
    {
       
    }
}