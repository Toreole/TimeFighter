using System.Collections;
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

        //The actual camera object
        protected new Camera camera;

        [SerializeField]
        protected Transform target;
        [SerializeField]
        protected float defaultSize = 9.5f;
        [SerializeField]
        protected AnimationCurve zoomCurve;

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
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
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