using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Controller;

namespace Game
{
    /// <summary>
    /// Manages the Game. Starts the game after some delay in the scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance = null;

        [SerializeField]
        private PlayerController player;

        [SerializeField]
        private GameObject[] enemies;

        [SerializeField]
        private TextMeshProUGUI countdownText;
        [SerializeField]
        private float countdownLength = 3.5f;

        [SerializeField]
        private Slider levelTimeSlider;

        //Might come in handy.
        public delegate void GameEvent();
        public static event GameEvent OnLevelStart    = null;
        public static event GameEvent OnLevelFail     = null;
        public static event GameEvent OnLevelComplete = null;

        public static bool GameStarted { get; private set; } = false;

        //Set the instance boiii
        private void Awake()
        {
            instance = this;
        }

        //Entry point for StartLevel. nothing fancy.
        private void Start()
        {
            if (player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p == null)
                    Debug.LogError("Could not find GameObject with tag Player");
                else
                    player = p.GetComponent<PlayerController>();
            }
            StartCoroutine(StartLevel());
        }

        //Update Health/Time
        private void LateUpdate()
        {
            if (!GameStarted)
                return;
            //levelTimeSlider.value = PLAYER.HEALTH;
            //if(PLAYER.HEALTH >= 0f) OnLevelFailed?.Invoke();
            //if(ALL_ENEMIES_DEAD AND PLAYER_ALIVE) OnLevelComplete?.Invoke();
        }

        /// <summary>
        /// Start the game after counting down some time.
        /// </summary>
        private IEnumerator StartLevel()
        {
            countdownText.gameObject.SetActive(true);
            for(float t = countdownLength; t > 0; t -= Time.deltaTime)
            {
                if (t < 0)
                    t = 0.000f;
                string timeLeft = t.ToString().Remove(4);
                countdownText.text = timeLeft;
                yield return null;
            }
            countdownText.gameObject.SetActive(false);
            OnLevelStart?.Invoke();
            GameStarted = true;
        }

        //If this doesnt exist, update the instance just in case.
        private void OnDestroy()
        {
            instance = null;
        }
    }
}