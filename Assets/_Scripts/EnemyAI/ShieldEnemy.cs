using UnityEngine;
using System.Collections;

namespace Game
{
    [System.Obsolete]
    public class ShieldEnemy : EnemyBase
    {
        [System.Obsolete]
        protected override void UpdateEnemy()
        {
            //Uhhhh spaghet
        }

        public override void ProcessHit(AttackHitData hitData)
        {
            //1. get the direction from the hit relative to this objects x position
            if(hitData.Position == Vector2.positiveInfinity)
            {
                Debug.LogError("hitData position was positiveInfinity.");
                return;
            }

            var relativeX = hitData.Position.x - transform.position.x;
            var normalizedHitDirection = (int) (relativeX / relativeX);
            if (facingDirection != normalizedHitDirection)
            {
                //The attack hits this enemy.
                ProcessHit(hitData, true);
            }
            else
            {
                //the enemy is blocking this attack.
            }
        }
    }
}