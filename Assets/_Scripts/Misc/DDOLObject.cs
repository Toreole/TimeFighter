using UnityEngine;
namespace Game.Misc
{
    public class DDOLObject : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}