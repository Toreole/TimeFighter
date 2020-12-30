using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Patterns.States;

namespace Game.Demo.Boss
{
    public class BossController : StateMachine<BossController>
    {
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            currentState.Update(this);
        }
    }
}