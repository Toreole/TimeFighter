using UnityEngine;
using Game.Patterns.States;
using System.Collections.Generic;

namespace Game.Demo.Boss
{
    public abstract class HandBehaviourState
    {
        //virtual to share some behaviour that can be added to or overridden completely.
        public virtual void OnCollisionEnter(Collision2D collision, BossHand hand)
        {

        }
        public virtual void OnCollisionExit(Collision2D collision, BossHand hand)
        {

        }
        //Exit isnt really required so im just leaving it empty in here to avoid unnecessary lines above.
        public virtual void Exit(BossHand hand) { }
        public abstract void Enter(BossHand hand);

        //OnDamaged only used in one state for "parrying"
        public virtual void OnDamaged() { }

        public abstract void Update(BossHand hand, float speedMultiplier);
    }
}