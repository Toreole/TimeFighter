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
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance = null;

        [Header("Entities")]
        [SerializeField]
        private GameObject player;
        private PlayerController controller;

        [SerializeField]
        private List<EnemyBase> enemies;
        private int enemyCount;
        private int deadEnemies;

        [Header("Countdown")]
        [SerializeField]
        private TextMeshProUGUI countdownText;
        [SerializeField]
        private float countdownLength = 3.5f;

        [Header("Timer")]
        [SerializeField]
        private Slider levelTimeSlider;
        [SerializeField]
        private Image sliderFill;
        [SerializeField]
        private Gradient sliderGradient;
        private AttackHitData timerDamage = new AttackHitData();

        [Header("Level Settings")]
        [SerializeField]
        private float clearTime = 10.0f;

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
            if (instance != null)
            {
                Destroy(instance.gameObject);
                instance = null;
            }
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

        /// <summary>
        /// Initial setup for the player health
        /// </summary>
        private void SetupPlayer()
        {
            controller.health = clearTime;
            controller.currentHealth = clearTime;

            levelTimeSlider.maxValue = clearTime;
            levelTimeSlider.value = clearTime;
            sliderFill.color = sliderGradient.Evaluate(1);
        }

        /// <summary>
        /// Update player health every frame and also check stuff
        /// </summary>
        private void LateUpdate()
        {
            if (!GameStarted)
                return;
            timerDamage.Damage = Time.deltaTime;
            controller.ProcessHit(timerDamage, true);
            levelTimeSlider.value = controller.currentHealth;
            sliderFill.color = sliderGradient.Evaluate(controller.currentHealth / controller.health);

            if (controller.IsDead)
                OnLevelFail?.Invoke();
            if (deadEnemies >= enemyCount && !controller.IsDead)
                OnLevelComplete?.Invoke();
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

        private void LevelFailed()
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