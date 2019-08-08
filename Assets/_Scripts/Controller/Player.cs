using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Diagnostics;

namespace Game.Controller
{
    /// <summary>
    /// The Player as a whole. The main component. 
    /// </summary>
    public class Player : Entity
    {
        [Header("Player Fields")]
        [SerializeField]
        protected PlayerController controller;

        public override void Damage(float amount)
        {
            throw new System.NotImplementedException();
        }

        //testing
        private void Start()
        {
            //this.SendMessage("OnStart");  
            var watch = new Stopwatch();
            watch.Start();
                var method = controller.GetType().GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                method.Invoke(controller, null);
            watch.Stop();
                print("a " + watch.ElapsedTicks);

            watch.Reset();

            watch.Start();
                controller.OnStart();
            watch.Stop();
                print("b " + watch.ElapsedTicks);

            watch.Reset();

            watch.Start();
                method.Invoke(controller, null);
            watch.Stop();
                print("c " + watch.ElapsedTicks);
            watch.Reset();

            watch.Start();
                controller.SendMessage("OnStart");
            watch.Stop();
                print("d " + watch.ElapsedTicks);
        }
    }
}