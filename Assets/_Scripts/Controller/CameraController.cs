﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Controller
{
    /// <summary>
    /// Should have some static calls like CameraController.DynamicZoom i guess maybe lol idk
    /// </summary>
    //TODO: everything about this
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        //protected instance so it can be used in a static context
        protected static CameraController _instance;
        public static Camera Current => _instance.camera;

        //The actual camera object
        protected new Camera camera;

        [SerializeField]
        protected Transform target;
        [SerializeField]
        protected float defaultSize = 9.5f;
        [SerializeField]
        protected AnimationCurve zoomCurve;
        [SerializeField]
        protected float maxSpeed = 5f;
        [SerializeField]
        protected float minSpeed = 1f;
        [SerializeField, Range(0f, 0.49f)]
        protected float criticalDistance = 0.1f; //onscreen viewport distance from the edges of the screen.
        [SerializeField]
        protected float criticalAcceleration = 100f; //multiplier for acceleration rate when at the edge of the screen.
        [SerializeField]
        protected float criticalDistanceSpeedMultiplier = 4f;
        [SerializeField]
        protected float accelDistance = 2f;
        [SerializeField]
        protected float idleDistance = 0.4f;
        [SerializeField]
        protected float acceleration = 4f;

        private float currentSpeed = 0f;

        //helper to reset zoom.
        public static float DefaultSize => _instance.defaultSize;

        //set up all the stuff
        void Start()
        {
            if (_instance)
            {
                Destroy(this.gameObject);
                return;
            }
            if (!camera)
                camera = GetComponent<Camera>();
            _instance = this;
            camera.orthographicSize = defaultSize;
        }

        //camera movement is done in lateupdate to not act weirdly with anything that hapens in update
        //could potentially use fixedupdate to get the position of the target, but lateupdate should be fine on its own.
        private void LateUpdate()
        {
            if (!target)
                return;
            Vector2 pos = target.position;

            //1. speed for the camera
            var targetPos = new Vector3(pos.x, pos.y, transform.position.z);
            var distanceToTarget = Vector3.Distance(targetPos, transform.position);

            //check if the player is close to the edge of the screen, if so accelerate much faster.
            Vector2 viewportTarget = camera.WorldToViewportPoint(target.position);
            if (viewportTarget.x <= criticalDistance || viewportTarget.x >= 1f-criticalDistance
                || viewportTarget.y <= criticalDistance || viewportTarget.y >= 1f-criticalDistance)
            {
                //the camera should accelerate a LOT when the player is at the edge of the screen.
                currentSpeed = Mathf.MoveTowards(currentSpeed, distanceToTarget * criticalDistanceSpeedMultiplier, acceleration * criticalAcceleration * Time.deltaTime);
            }
            else if(distanceToTarget >= accelDistance)
            {
                //large distance from the player, should accelerate to a given limit to go behind the player.
                if(currentSpeed > maxSpeed)
                    currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * criticalAcceleration * Time.deltaTime);
                else 
                    currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
            }
            else if(distanceToTarget >= idleDistance)
            {
                //camera should slow down because its catching up.
                currentSpeed = Mathf.MoveTowards(currentSpeed, minSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                //camera should move to idle.
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, acceleration / 2f * Time.deltaTime);
            }

            //2. move the camera towards the target.
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);
            
            transform.position = newPosition;
            //transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        }

        /// <summary>
        /// private zoom function thats mainly used through the static DynamicZoom function
        /// </summary>
        void M_DynamicZoom(float size, float time)
        {
            if (Mathf.Approximately(size, camera.orthographicSize))
                return; //if the size is the same as the current ortho size, dont do anything
            StopAllCoroutines(); //just to make sure
            StartCoroutine(ZoomIn());
            IEnumerator ZoomIn()
            {
                float startSize = camera.orthographicSize;
                for(float t = 0; t < time; t += Time.deltaTime) //pretty standard stuff
                {
                    float tValue = zoomCurve.Evaluate(t / time);
                    camera.orthographicSize = Mathf.Lerp(startSize, size, tValue);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Zooms the main camera to the given size over the time.
        /// </summary>
        /// <param name="size">the orthographic size to zoom to.</param>
        /// <param name="time">the time that the zoom takes.</param>
        public static void DynamicZoom(float size, float time = 0.4f)
        {
            _instance.M_DynamicZoom(size, time);
        }
    }
}