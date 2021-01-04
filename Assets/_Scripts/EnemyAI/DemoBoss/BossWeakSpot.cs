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

        public override void Interact(Game.Controller.Player entity)
        {
            boss.Damage(damagePerInteraction);
            
            Vector2 forceDirection = entity.Position - (Vector2)transform.position;
            forceDirection.Normalize();
            entity.Body.AddForce(forceDirection * knockbackStrength * entity.Body.mass, ForceMode2D.Impulse);
        }
    }
}