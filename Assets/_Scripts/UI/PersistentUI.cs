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
        [SerializeField]
        protected RectTransform interactionIconHolder;
        [SerializeField]
        protected GameObject interactionKeyboardIcon, interactionGamepadIcon;

        private new Camera camera;

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

        private void Start() 
        {
            camera = Game.Controller.CameraController.Current ?? Camera.main; //use the CameraController first, if unavailable use Camera.main
            Debug.Log(camera == true);
        }

        public static void AutoSave(bool activeSaving) 
            =>   Instance.autoSaveIcon.SetActive(activeSaving);

        private static PersistentUI CreateInstance()
        {
            var pui = Instantiate(Resources.Load<GameObject>("UI/PersistentUI")).GetComponent<PersistentUI>();
            //just making sure that there is a camera.
            pui.camera = Game.Controller.CameraController.Current ?? Camera.main;
            return pui;
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


        private Transform lastInteractTarget;
        private Vector3 offset;


        //LateUpdate adjusts the Interaction Icon. //TODO: Scale it appropriately, on a higher camera orthographic size, scale should be smaller.
        private void LateUpdate() 
        {
            if(lastInteractTarget)
            {
                var rt = Instance.transform as RectTransform;
                var sizeDelta = rt.sizeDelta;
                Vector2 uiOffset = new Vector2(sizeDelta.x * 0.5f, sizeDelta.y * 0.5f);

                Vector2 viewportPos = Instance.camera.WorldToViewportPoint(lastInteractTarget.position + offset);
                Vector2 proportionalPos = new Vector2(viewportPos.x * sizeDelta.x, viewportPos.y * sizeDelta.y);
                Instance.interactionIconHolder.localPosition = proportionalPos - uiOffset;

                Instance.interactionGamepadIcon.SetActive(Game.Controller.Utility.KeybOrController.UseController);
                Instance.interactionKeyboardIcon.SetActive(!Game.Controller.Utility.KeybOrController.UseController);
            }
        }

        //TODO: maybe fade it or something idk, make it look nice at some point
        public static void PlaceInteractionAt(Transform worldSpaceTarget, Vector3 offset)
        {
            Instance.lastInteractTarget = worldSpaceTarget;
            Instance.offset = offset;
            Instance.interactionIconHolder.gameObject.SetActive(true);
        }

        public static void HideInteraction()
        {
            Instance.lastInteractTarget = null;
            Instance.interactionIconHolder.gameObject.SetActive(false);
        }
    }
}
