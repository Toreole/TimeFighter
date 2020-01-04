using UnityEngine;
using System.Collections;

namespace Game
{
    [System.Obsolete]
    public class MageEnemy : EnemyBase
    {
        [SerializeField, Tooltip("The positions this mage can teleport inbetween")]
        protected Transform[] positions;

        int posIndex = 1;
        bool canTeleport = true;

        public override void Damage(float amount)
        {
            throw new System.NotImplementedException();
        }

        //public override bool IsGrounded => throw new System.NotImplementedException();
        
        internal void Teleport()
        {
            Debug.Log("Teleport");
            canTeleport = false;
            transform.position = positions[posIndex].position;
            posIndex = (posIndex + 1) % positions.Length;
        }
        
        //public override void ProcessHit(AttackHitData hitData)
        //{
        //    Debug.Log("Mage");
        //    if(canTeleport)
        //    {
        //        Teleport();
        //        return;
        //    }
        //    base.ProcessHit(hitData);
        //}
        //public override void ProcessHit(AttackHitData hitData, bool onlyDamage)
        //{
        //    if (canTeleport)
        //    {
        //        Teleport();
        //        return;
        //    }
        //    base.ProcessHit(hitData, onlyDamage);
        //}
    }
}