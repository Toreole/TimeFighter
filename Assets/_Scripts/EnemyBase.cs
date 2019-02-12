using UnityEngine;
using System.Collections;

namespace Game
{
    public abstract class EnemyBase : MonoBehaviour
    {
        protected int currentHP = 1;

        [SerializeField]
        protected EnemySettings settings;

        protected abstract void Move();
        protected abstract void TurnAround();

        public virtual void Damage(int amount)
        {

        }
    }
}