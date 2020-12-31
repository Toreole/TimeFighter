﻿using UnityEngine;

namespace Game.Demo.Boss
{
    ///<summary>Outsourcing some movement from the BossController. Takes some commands from the BC.</summary>
    public class BossHand : MonoBehaviour
    {
        [SerializeField] //obviously required for animations to work.
        protected Animator animator;
        [SerializeField]
        protected Rigidbody2D body; //rigidbody movement? probably right?

    }

    public enum HandState 
    {
        Undefined = 0,
        Ready = 1,
        Attacking = 2,
        Waiting = 3,
        Returning = 4
    }
}