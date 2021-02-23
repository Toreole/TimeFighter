﻿using System.Collections.Generic;
using Game.Patterns.States;
using UnityEngine;

namespace Game.Demo.Boss
{
    ///<summary>Outsourcing some movement from the BossController. Takes some commands from the BC.</summary>
    public class BossHand : MonoBehaviour, IDamageable
    {
        internal static readonly HandNoControlState NoControlState = new HandNoControlState();

        [SerializeField] //obviously required for animations to work.
        private Animator animator;
        [SerializeField]
        private Rigidbody2D body; //rigidbody movement? probably right?
        [SerializeField]
        private Transform locale;
        [SerializeField]
        private new Collider2D collider;
        //[SerializeField, NaughtyAttributes.Tag]
        //internal string playerTag;

        private Vector3 startingPosition;
        internal float trackSpeed, slamSpeed, punchSpeed; //TODO: instead of internal make this somehow nicer with properties or having the settings directly on the hand instead of on the boss???
        internal float speedMultiplier;

        //just so the boss controller knows whats going on here.
        public HandState ActivityStatus {get; set;} = HandState.Returning;
        public bool IsReady => ActivityStatus == HandState.Ready;
        public Rigidbody2D Body => body;
        public Bounds Bounds => collider.bounds;
        
        [System.NonSerialized] internal Vector2 returnVelocity = Vector2.zero;

        public Vector3 preferredPosition => locale.position;

        //This is way too complicated for this scope. dont do it.
        //internal bool isPlayerAttached = false; //is the player currently attached?
        //internal float playerAttachTime = 0f; //when did the player attach?

        List<Collider2D> ignoredColliders = new List<Collider2D>();

        HandBehaviourState currentState = BossHand.NoControlState; //hand starts out without control.

        private void Awake() 
        {
            startingPosition = transform.position;
        }

        public void SetSpeeds(float trackSpeed, float slamSpeed, float punchSpeed, float multiplier = 1.0f)
        {
            this.trackSpeed = trackSpeed * multiplier;
            this.slamSpeed = slamSpeed * multiplier;
            this.punchSpeed = punchSpeed * multiplier;
            this.speedMultiplier = multiplier;
        }

        //--TODO: Smooth it out.
        public void ResetHand()
        {
            transform.position = startingPosition;
            ActivityStatus = HandState.Returning;
            currentState = NoControlState;
            foreach(var col in ignoredColliders)
                Physics2D.IgnoreCollision(col, this.collider, false);
            ignoredColliders.Clear();
        }

        //As collisions and physics are handled in FixedUpdate, also handle ignored collisions in here.
        private void FixedUpdate() 
        {
            //this should probably use fixedupdate instead.
            currentState.Update(this, speedMultiplier);
            for(int i = 0; i < ignoredColliders.Count; i++)
            {
                var other = ignoredColliders[i];
                if(other && other.Distance(collider).distance > 0.1f)//if it doesnt overlap with the hands collider anymore
                { 
                    Physics2D.IgnoreCollision(other, collider, false);// re-enable the collision between the two.
                    ignoredColliders.Remove(other);
                    return;
                }
            }
        }

        ///<summary>Temporarily ignores collisions with this collider</summary>
        public void IgnoreCollisionWith(Collider2D other)
        {
            Physics2D.IgnoreCollision(other, this.collider);
            ignoredColliders.Add(other);
        }

        //perform a slam attack on the target.
        public void Slam(Entity target)
        {
            TransitionToState(new HandTrackTargetState(target));
        }

        //Clap both hands at the target.
        public void Clap(Entity target, float side)
        {
            TransitionToState(new HandClapPrepState(target, side));
        }

        //From IDamagable, used by punches
        public void Damage(float amount)
        {
            currentState.OnDamaged();
        }

        //collision enter MIGHT be used, but checking the rigidbody contacts could suffice.
        //collision is relevant for both punch and slam so idk. Maybe also for detecing whether the player is mounted while this hand is waiting/returning?
        private void OnCollisionEnter2D(Collision2D other)
        {
            currentState.OnCollisionEnter(other, this);
        }

        private void OnCollisionExit2D(Collision2D other) 
        {
            currentState.OnCollisionExit(other, this);
        }

        public void TransitionToState(HandBehaviourState s)
        {
            currentState.Exit(this);
            s.Enter(this);
            currentState = s;
        }

        public void SetActiveCollision(bool activeCollision)
        {
            collider.enabled = activeCollision;
        }

#region DEBUGGING
        public HandBehaviourState GetCurrentState() => currentState;
#endregion
    }

    public enum HandState 
    {
        Undefined = 0,
        Ready = 1,
        Attacking = 2,
        Waiting = 3,
        Returning = 4,
        //MountedByPlayer = 5,
        Disabled = 6
    }
}