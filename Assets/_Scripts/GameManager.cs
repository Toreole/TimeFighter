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
        private GameObject player;

        [SerializeField]
        private List<EnemyBase> enemies;
        private int enemyCount;
        private int deadEnemies;

        [SerializeField]
        private TextMeshProUGUI countdownText;
        [SerializeField]
        private float countdownLength = 3.5f;

        [SerializeField]
        private Slider levelTimeSlider;

        //Might come in handy.
        public delegate void GameEvent();
        public static GameEvent OnLevelStart    { get { return instance.LevelStartEvent;    } set { instance.LevelStartEvent = value; } }
        public static GameEvent OnLevelFail     { get { return instance.LevelFailEvent;     } set { instance.LevelFailEvent = value; } }
        public static GameEvent OnLevelComplete { get { return instance.LevelCompleteEvent; } set { instance.LevelCompleteEvent = value; } }

        protected event GameEvent LevelStartEvent = null;
        protected event GameEvent LevelFailEvent = null;
        protected event GameEvent LevelCompleteEvent = null;

        public static bool GameStarted { get; private set; } = false;

        public Transform PlayerTransform { get { return player.transform; } }

        //Set the instance boiii
        private void Awake()
        {
            instance = this;
            //reset these bois just in case.
            OnLevelStart    = null;
            OnLevelFail     = null;
            OnLevelComplete = null;
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
                    player = p;
            }
            if(enemies.Count == 0)
            {
                Debug.Log("Trying to find enemies in scene...");
                enemies = new List<EnemyBase>(FindObjectsOfType<EnemyBase>());
            }
            enemyCount = enemies.Count;
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
            for(float t = countdownLength; t > 0f; t -= Time.deltaTime)
            {
                if (t < 0)
                    t = 0.000f;
                string timeLeft = t.ToString();
                if (timeLeft.Length > 4)
                    timeLeft = timeLeft.Remove(4);
                countdownText.text = timeLeft + "s";
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

        public void RegisterDead(EnemyBase deadEnemy)
        {
            if(enemies.Contains(deadEnemy))
            {
                enemies.Remove(deadEnemy);
                deadEnemies++;

                if(deadEnemies >= enemyCount && GameStarted)
                {
                    OnLevelComplete?.Invoke();
                }
            }
        }

        public void ResetLevel()
        {
            //How tho?
        }
    }
}