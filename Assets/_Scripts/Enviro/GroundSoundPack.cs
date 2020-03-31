using UnityEngine;
using UnityEditor;

namespace Game
{
    [CreateAssetMenu(fileName = "new SoundPack", menuName = "Game SO/SoundPack")]
    public class GroundSoundPack : ScriptableObject
    {
        [SerializeField]
        protected AudioClip[] walking, land;

        public AudioClip GetWalkSound() => walking[Random.Range(0, walking.Length - 1)];
        public AudioClip GetLandSound() => land[Random.Range(0, land.Length - 1)];
    }
}