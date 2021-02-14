using UnityEngine;
using Game.Interactions;

namespace Game.Demo.Boss
{
    public class BossWeakSpot : Interactable
    {
        [SerializeField]
        private BossController boss;
        [SerializeField]
        private float damagePerInteraction;
        [SerializeField]
        private float knockbackStrength;

        public override void Interact(Game.Controller.Player player)
        {
            boss.Damage(damagePerInteraction);
            
            Vector2 selfPosition = transform.position;
            float xDirection = Util.Normalized(player.Position.x - selfPosition.x);
            Vector2 forceDirection = new Vector2(xDirection, 0.7f);
            forceDirection.Normalize();
            player.Stun(2f, false);
            //why isnt this adding force????
            player.Body.AddForce(forceDirection * knockbackStrength * player.Body.mass, ForceMode2D.Impulse);
        }
    }
}