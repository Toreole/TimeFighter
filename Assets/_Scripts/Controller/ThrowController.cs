using System;
using System.Collections.Generic;
using UnityEngine;
using Game;

namespace Game.Controller
{
    /// <summary>
    /// Throw shit lol
    /// </summary>
    public class ThrowController : BaseAction
    {
        [SerializeField]
        protected Throwable throwable;

        protected int currentAmmo;

        public override void CancelAction()
        {
            throw new NotImplementedException();
        }

        public override void PerformAction()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// An Entity claims ownership of this.
        /// </summary>
        /// <param name="owner"></param>
        public override void ClaimOwnership(Entity owner)
        {
            if (HasOwner)
                return;
            base.ClaimOwnership(owner);
            Setup();
        }

        /// <summary>
        /// Additional setup for this action controller
        /// </summary>
        protected void Setup()
        {
            if (entity.IsPlayer)
            {
                //The player should load the last equipped throwable.
                var save = GameManager.FetchSave();
                if (save == null || string.IsNullOrEmpty(save.equippedThrowable))
                {
                    entity.RemoveAction(this);
                    Destroy(this);
                    return;
                }
                throwable = Resources.Load<Throwable>(save.equippedThrowable);
            }
            currentAmmo = throwable.StartAmount;
            CanPerform = true;
        }
    }
}
