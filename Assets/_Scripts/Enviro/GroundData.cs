using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace Game
{
    /// <summary>
    /// Additional Data of ground/wall colliders.
    /// </summary>
    public class GroundData : MonoBehaviour, Game.Controller.IHookTarget
    {
        [SerializeField]
        protected GroundSoundPack soundPack;
        [SerializeField, EnumFlags]
        protected GroundFlags info;

        public bool Valid => HasFlag(GroundFlags.Hookable);

        public bool HasFlag(GroundFlags flag) => info.HasFlag(flag);
    }

    [System.Flags]
    public enum GroundFlags
    {
        //basic ones.
        Climbable = 1 << 0,
        Sticky = 1 << 1,
        Slippery = 1 << 2,
        Hookable = 1 << 3,
        Fragile = 1 << 4,
        Transparent = 1 << 5,
        Moving = 1 << 6,
        Bouncy = 1 << 7,
        Platform = 1 << 8
    }
}
