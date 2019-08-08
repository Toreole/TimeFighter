using System;
using UnityEngine;

namespace Game.Controller
{
    public abstract class PlayerComponent : MonoBehaviour
    {
        protected Player player;

        //instead of start, the player just calls this
        public void SetPlayer(Player p)
        {
            player = p;
        }
    }
}
