using UnityEngine;

namespace Game
{
    public class AttackHitData
    {
        public int Damage       { get; set; } = 0;
        public Vector2 Position { get; set; } = Vector2.positiveInfinity;
        public float HitForce   { get; set; } = 0f;

        public AttackHitData() { }
        public AttackHitData(int damage)
        {
            Damage = damage;
        }
        public AttackHitData(int dmg, Vector2 pos)
        {
            Damage = dmg;
            Position = pos;
        }
        public AttackHitData(int dmg, Vector2 pos, float force)
        {
            Damage = dmg;
            Position = pos;
            HitForce = force;
        }
        public AttackHitData(Vector2 pos, float force)
        {
            Position = pos;
            HitForce = force;
        }
    }
}
