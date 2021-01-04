using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    /// <summary>
    /// Static interface for Game UI, no matter where in the game.
    /// Usecases: Popups / Alerts / AutoSave icon.
    /// </summary>
    public class PersistentUI : MonoBehaviour
    {
        [SerializeField]
        protected GameObject autoSaveIcon;
        [SerializeField]
        protected GameObject bossDataDisplay;
        [SerializeField]
        protected Slider bossHealthbar;
        [SerializeField]
        protected TextMeshProUGUI bossName;

        private static PersistentUI Instance
        {
            get
            {
                if (!_instance)
                    return CreateInstance();
                return _instance;
            }
        }
        private static PersistentUI _instance;

        private void Awake()
        {
            if(_instance)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        public static void AutoSave(bool activeSaving) 
            =>   Instance.autoSaveIcon.SetActive(activeSaving);

        private static PersistentUI CreateInstance()
        {
            return Instantiate(Resources.Load<GameObject>("UI/PersistentUI")).GetComponent<PersistentUI>();
        }

        public static Slider GetBossHealthAndSetupDisplay(string name, float maxHealth, float currentHealth)
        {
            PersistentUI pui = Instance;
            pui.bossDataDisplay.SetActive(true);
            pui.bossName.text = name;
            var healthbar = pui.bossHealthbar;
            healthbar.maxValue = maxHealth;
            healthbar.value = currentHealth;
            return healthbar;
        }

        public static void HideBossUI()
            => Instance.bossDataDisplay.SetActive(false);
    }
}
