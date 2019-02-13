using UnityEngine;

namespace Game
{
    public class AttackHitData
    {
        public int Damage       { get; set; } = 0;
        public Vector2 Position { get; set; } = Vector2.positiveInfinity;
        public float HitForce   { get; set; } = 0f;
    }
}
