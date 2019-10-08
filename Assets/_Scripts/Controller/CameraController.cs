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

            camera.orthographicSize = defaultSize;
        }

        private void LateUpdate()
        {
            if (!target)
                return;
            Vector2 pos = target.position;
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        }


    }
}