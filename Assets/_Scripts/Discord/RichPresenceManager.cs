using UnityEngine;
using System.Collections;
using Discord;

namespace Game
{
    public class RichPresenceManager : MonoBehaviour
    {
        [SerializeField]
        protected long application_id;
        [SerializeField]
        DiscordRpc.RichPresence presence;
    }
}