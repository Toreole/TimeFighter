using UnityEngine;
using System.Collections;

namespace Game
{
    public class ShieldEnemy : EnemyBase
    {
        protected override void Move()
        {
            switch(settings.Movement)
            {
                case MovementPattern.ShortDistance:
                    Wander();
                    break;
                case MovementPattern.EdgeToEdge:
                    WalkUntilEdge();
                    break;
                default:
                    return; //stationary by default
            }
        }

        protected override void UpdateEnemy()
        {
            Move();
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
            if (facingDirection == normalizedHitDirection)
            {
                //The attack hits this enemy.
                Damage(hitData.Damage);
            }
            else
            {
                //the enemy is blocking this attack.
            }
        }
    }
}