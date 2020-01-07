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

        private int paramID;

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
            if (enumToID.TryGetValue(PlayerAnimation.GroundedBool, out paramID))
                animator.SetBool(paramID, controller.IsGrounded);
            //OnAttackA/B
            //OnSpecialA/B
        }

        private void OnDisable()
        {
            controller.OnEnterGround -= EnterGround;
            controller.OnLeaveGround -= LeaveGround;
            controller.OnTakeDamage -= TakeDamage;
        }

        private void LateUpdate()
        {
            if (enumToID.TryGetValue(PlayerAnimation.XVelFloat, out paramID))
                animator.SetFloat(paramID, Mathf.Abs(controller.LastVel.x));
            if (enumToID.TryGetValue(PlayerAnimation.YVelFloat, out paramID))
                animator.SetFloat(paramID, controller.LastVel.y);
        }

        /// <summary>
        /// Play the correct landing animation.
        /// </summary>
        /// <param name="landing"></param>
        void EnterGround(LandingType landing)
        {
            if (landing == LandingType.Roll && enumToID.TryGetValue(PlayerAnimation.RollTrigger, out paramID))
                animator.SetTrigger(paramID);
            else if (landing == LandingType.HardLanding && enumToID.TryGetValue(PlayerAnimation.LandTrigger, out paramID))
                animator.SetTrigger(paramID);
            if (enumToID.TryGetValue(PlayerAnimation.GroundedBool, out paramID))
                animator.SetBool(paramID, true);
        }

        void LeaveGround()
        {
            if (enumToID.TryGetValue(PlayerAnimation.GroundedBool, out paramID))
                animator.SetBool(paramID, false);
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
