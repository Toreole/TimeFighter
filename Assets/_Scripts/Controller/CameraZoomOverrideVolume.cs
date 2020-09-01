using UnityEngine;
using System.Collections;

namespace Game.Controller
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class CameraZoomOverrideVolume : MonoBehaviour
    {
        [SerializeField, Min(2)]
        protected float zoom;
        [SerializeField]
        protected float zoomInTime = 1, zoomOutTime = 1;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                CameraController.DynamicZoom(zoom, zoomInTime);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
                CameraController.ResetZoom(zoomOutTime);
        }

    }
}