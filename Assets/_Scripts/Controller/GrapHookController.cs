using UnityEngine;
using System;
using Game.Controller.Utility;
using System.Collections;
using Game.Controller.PlayerStates;
using UnityEngine.InputSystem;

namespace Game.Controller
{
    [RequireComponent(typeof(PlayerController))]
    public class GrapHookController : MonoBehaviour
    {
        [SerializeField]
        protected GameObject hookPrefab;
        [SerializeField]
        protected float throwSpeed = 20, maxDistance = 8;
        [SerializeField]
        protected LayerMask hitMask;
        
        SpriteRenderer activeHookRenderer;
        Transform activeHook;
        PlayerController controller;

        //true => away from player | false => towards player.
        bool goingOutward = true;
        bool ongoing = false;
        float currentDistance = 0f;

        //some initial setup.
        private void Start()
        {
            controller = GetComponent<PlayerController>();
        }

        //Throw the hook hotward.
        public void Throw()
        {
            if (ongoing)
                return;
            //Instantiate a new hook.
            GameObject instance = Instantiate(hookPrefab);
            activeHook = instance.transform;
            activeHookRenderer = instance.GetComponent<SpriteRenderer>();

            Vector2 direction;
            //TODO: maybe find a nicer way to do this.
            if(KeybOrController.UseController)
            {
                var gamepad = Gamepad.current;
                direction = gamepad.leftStick.ReadValue();
                direction.Normalize();
            }
            else
            {
                var mouse = Mouse.current;
                Vector2 screenPos = mouse.position.ReadValue();
                Vector3 worldPos = CameraController.Current.ScreenToWorldPoint(screenPos);
                direction = worldPos - transform.position;
                direction.Normalize();
            }
            //Start the damn thing.
            StartCoroutine(DoUpdate(direction));
        }

        //move forward.
        IEnumerator DoUpdate(Vector2 direction)
        {
            //0. correct rotation and reset distance counter
            activeHook.up = direction;
            currentDistance = 0f;
            RaycastHit2D hit;
            for (; currentDistance < maxDistance; currentDistance += Time.deltaTime * throwSpeed)
            {
                //1. Move to center.
                activeHook.position = (Vector2)transform.position + direction * (currentDistance * 0.5f);
                //2. adjust size of hook sprite.
                activeHookRenderer.size = new Vector2(1, currentDistance);
                //3. check for valid hit
                if(hit = Physics2D.Raycast(transform.position, direction, currentDistance, hitMask))
                {
                    if (hit.transform.GetComponent<IHookTarget>() != null)
                        controller.SwitchToState(new HookFlyPlayerState(hit.point, activeHookRenderer));
                    else
                        Destroy(activeHook.gameObject);
                    ongoing = false;
                    yield break;
                }
                yield return null;
            }
            //no target, destroy.
            Destroy(activeHook.gameObject);
        }

        //cancel the ongoing hook.
        void Interrupt()
        {
            StopAllCoroutines();
            ongoing = false;
            //not sure what else huh.
        }

    }
}