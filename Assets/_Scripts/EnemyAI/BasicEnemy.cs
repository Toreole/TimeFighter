using UnityEngine;
using System.Collections;

namespace Game
{
    [System.Obsolete]
    public class BasicEnemy : EnemyBase
    {
        public override bool IsGrounded => throw new System.NotImplementedException();

        [System.Obsolete]
        //This basic bitch doesnt do anything lmao
        protected override void UpdateEnemy()
        {

        }
    }
}