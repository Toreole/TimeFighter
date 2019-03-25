using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Controller;

namespace Game
{
    /// <summary>
    /// Manages the Game. Starts the game after some delay in the scene.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance = null;

        [Header("Level Settings")]
        [SerializeField, Tooltip("All basic Info about this level")]
        protected LevelInfo info;

        [Header("Entities")]
        [SerializeField]
        protected GameObject player;
        protected PlayerController controller;

        [SerializeField]
        protected List<EnemyBase> enemies;
        protected int enemyCount;
        protected int deadEnemies;

        [Header("Countdown")]
        [SerializeField]
        private TextMeshProUGUI countdownText;
        [SerializeField]
        private float countdownLength = 3.5f;

        [Header("Timer")]
        [SerializeField]
        protected Slider levelTimeSlider;
        [SerializeField]
        protected Image sliderFill;
        [SerializeField]
        protected Gradient sliderGradient;
        protected AttackHitData timerDamage = new AttackHitData();

        protected bool justCompleted = false;
        protected float timeLeft = 10f;

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
        protected void Awake()
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
            instance = this;
        }

        //Entry point for StartLevel. nothing fancy.
        protected void Start()
        {
            if (RichPresenceManager.instance != null)
                RichPresenceManager.instance.SetActiveLevel(info.DescriptiveName);
            if (player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p == null)
                    Debug.LogError("Could not find GameObject with tag Player");
                else
                {
                    player = p;
                }
            }
            if (player == null)
                return;
            controller = player.GetComponent<PlayerController>();
            if (enemies.Count == 0)
            {
                Debug.Log("Trying to find enemies in scene...");
                enemies = new List<EnemyBase>(FindObjectsOfType<EnemyBase>());
            }
            enemyCount = enemies.Count;
            SetupPlayer();
            StartCoroutine(StartLevel());
            OnLevelFail += LevelFailed;
        }

        //TODO: player time and health should be seperated
        /// <summary>
        /// Initial setup for the player health
        /// </summary>
        protected void SetupPlayer()
        {
            if (info.LevelTime > 0)
            {
                timeLeft = info.LevelTime;

                levelTimeSlider.maxValue = info.LevelTime;
                levelTimeSlider.value = info.LevelTime;
                sliderFill.color = sliderGradient.Evaluate(1);
            }
            else
            {
                levelTimeSlider.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Update player health every frame and also check stuff
        /// </summary>
        protected void LateUpdate()
        {
            if (!GameStarted)
                return;
            if (info.LevelTime <= 0)
                return;
            timeLeft -= Time.deltaTime;
            levelTimeSlider.value = timeLeft;
            sliderFill.color = sliderGradient.Evaluate(timeLeft / info.LevelTime);

            if (controller.IsDead || timeLeft <= 0f)
                OnLevelFail?.Invoke();
            if (deadEnemies >= enemyCount && !controller.IsDead && !justCompleted)
            {
                justCompleted = true;
                OnLevelComplete?.Invoke();
            }
        }

        /// <summary>
        /// Start the game after counting down some time.
        /// </summary>
        protected IEnumerator StartLevel()
        {
            //Debug.Log("Start...");
            countdownText.gameObject.SetActive(true);
            for(float t = countdownLength; t > 0f; t -= Time.deltaTime)
            {
                if (t < 0)
                    t = 0.000f;
                countdownText.text = t.ToString("00.00") + "s";
                yield return null;
            }
            countdownText.gameObject.SetActive(false);
            OnLevelStart?.Invoke();
            //Debug.Log("Started");
            GameStarted = true;
        }

        //If this doesnt exist, update the instance just in case.
        protected void OnDestroy()
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

        protected void LevelFailed()
        {
            ResetLevel();
            GameStarted = false;
        }

        public void ResetLevel()
        {
            //instead of just reloading the scene, actually Reset all values of the entities n shit
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            foreach (var entity in enemies)
                entity.ResetEntity();
            controller.ResetEntity();
            SetupPlayer();
            StartCoroutine(StartLevel());
        }
    }
}