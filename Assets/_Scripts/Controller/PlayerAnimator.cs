using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Generics;
using Game.Misc;

namespace Game.Controller
{
    /// <summary>
    /// The thing that controls which animation plays on the player (middle-translator for the Animator and PlayerController).
    /// </summary>
    public class PlayerAnimator : AnimatorController<Player, PlayerParameterLink, PlayerAnimation>
    {
        protected PlayerController controller;
        [SerializeField]
        protected Animator animator;

        public void DisableControls() => controller.IgnorePlayerInput = true;
        public void EnableControls() => controller.IgnorePlayerInput = false;

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponentInParent<PlayerController>();
        }

        private void OnEnable()
        {
            controller.OnEnterGround += EnterGround;
            controller.OnLeaveGround += LeaveGround;
            controller.OnTakeDamage += TakeDamage;
            //OnAttackA/B
            //OnSpecialA/B
        }

        private void LateUpdate()
        {
            int id;
            if(enumToID.TryGetValue(PlayerAnimation.XVelFloat, out id))
                animator.SetFloat(id, controller.LastVel.x);
            if (enumToID.TryGetValue(PlayerAnimation.YVelFloat, out id))
                animator.SetFloat(id, controller.LastVel.y);
        }

        void EnterGround(bool roll)
        {
            if(enumToID.TryGetValue(roll? PlayerAnimation.RollTrigger : PlayerAnimation.LandTrigger, out int id ))
                animator.SetTrigger(id);
        }

        void LeaveGround()
        {

        }

        void TakeDamage()
        {

        }

    }
    [Serializable]
    public class PlayerParameterLink : AnimatorParameterLink<PlayerAnimation> { }
    [Serializable]
    public enum PlayerAnimation
    {
        GroundedBool, SlideBool,
        RollTrigger, LandTrigger,
        XVelFloat, YVelFloat,
    }
}
