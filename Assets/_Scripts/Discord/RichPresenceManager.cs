using UnityEngine;
using System.Collections;
using Discord;
using UnityEngine.SceneManagement;

namespace Game
{
    [System.Serializable]
    public class DiscordJoinEvent : UnityEngine.Events.UnityEvent<string> { }

    [System.Serializable]
    public class DiscordSpectateEvent : UnityEngine.Events.UnityEvent<string> { }

    [System.Serializable]
    public class DiscordJoinRequestEvent : UnityEngine.Events.UnityEvent<DiscordRpc.DiscordUser> { }

    public class RichPresenceManager : MonoBehaviour
    {
        public static RichPresenceManager instance;

        [Header("Application Information")]
        [SerializeField]
        protected string applicationId;
        [SerializeField]
        protected string optionalSteamId;
        protected bool isRunning = false;

        [Header("Which Scene to Load next"), SerializeField]
        protected string nextScene;

        [Header("Event Callbacks")]
        public UnityEngine.Events.UnityEvent onConnect;
        public UnityEngine.Events.UnityEvent onDisconnect;
        public UnityEngine.Events.UnityEvent hasResponded;
        public DiscordJoinEvent onJoin;
        public DiscordSpectateEvent onSpectate;
        public DiscordJoinRequestEvent onJoinRequest;

        protected DiscordRpc.DiscordUser joinRequest;
        protected DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
        
        DiscordRpc.EventHandlers handlers;

        private void Start()
        {
            if (instance != null)
                return;

            Init();
            SceneManager.LoadScene(nextScene);
        }

        private void Init()
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            isRunning = true;

            handlers = new DiscordRpc.EventHandlers();
            handlers.readyCallback += ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;
            handlers.joinCallback += JoinCallback;
            handlers.spectateCallback += SpectateCallback;
            handlers.requestCallback += RequestCallback;
            DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);
            StartCoroutine(LateStart());
        }

        private void OnApplicationQuit()
        {
            DiscordRpc.Shutdown();
        }

        void Update()
        {
            //why u no work
            if (isRunning)
            {
                DiscordRpc.RunCallbacks();
            }
        }

        IEnumerator LateStart()
        {
            yield return new WaitForSeconds(1f);
			
            presence.details = "Actually no....";
            presence.state = "Checking out Anime Tiddy.";
            DiscordRpc.UpdatePresence(presence);
        }

        #region Discord_Callbacks

        public void RequestRespondYes()
        {
            Debug.Log("Discord: responding yes to Ask to Join request");
            DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
            hasResponded.Invoke();
        }

        public void RequestRespondNo()
        {
            Debug.Log("Discord: responding no to Ask to Join request");
            DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
            hasResponded.Invoke();
        }

        public void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser)
        {
            Debug.Log(string.Format("Discord: connected to {0}#{1}: {2}", connectedUser.username, connectedUser.discriminator, connectedUser.userId));
            onConnect.Invoke();
        }

        public void DisconnectedCallback(int errorCode, string message)
        {
            Debug.Log(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
            onDisconnect.Invoke();
        }

        public void ErrorCallback(int errorCode, string message)
        {
            Debug.Log(string.Format("Discord: error {0}: {1}", errorCode, message));
        }

        public void JoinCallback(string secret)
        {
            Debug.Log(string.Format("Discord: join ({0})", secret));
            onJoin.Invoke(secret);
        }

        public void SpectateCallback(string secret)
        {
            Debug.Log(string.Format("Discord: spectate ({0})", secret));
            onSpectate.Invoke(secret);
        }

        public void RequestCallback(ref DiscordRpc.DiscordUser request)
        {
            Debug.Log(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
            joinRequest = request;
            onJoinRequest.Invoke(request);
        }
        #endregion
    }
}