using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

using UInput = UnityEngine.Input;

namespace Game.Controller.Input
{
    public class PlayerInput : MonoBehaviour
    {
        //Just to avoid having to do something like Find()
        public static PlayerInput Instance { get; private set; }

        [SerializeField]
        protected TextAsset fallbackInputMap;
        
        //the input map created from the json files in the start.
        public InputMap runtimeInputMap;

        //file name for input
        const string playerInputMap = "CustomInput.json";

        //Set up the input.
        private void Awake()
        {
            Instance = this;
            //See if a custom thing has been made already
            var path = Path.Combine(SaveManager.SaveLocation, playerInputMap);
            if (File.Exists(path))
            {
                FileStream stream = File.Open(path, FileMode.Open);
                byte[] buffer = new byte[512];
                stream.Read(buffer, 0, (int)stream.Length);
                var content = Encoding.ASCII.GetString(buffer);
                runtimeInputMap = InputMap.FromJson(content);
                stream.Flush();
                stream.Close();
            }
            else
            {
                //else set the default stuff.
                runtimeInputMap = InputMap.FromJson(fallbackInputMap);
            }
            DontDestroyOnLoad(gameObject);
        }

        //just for saving new maps.
        internal void OverrideInputMap(InputMap map)
        {
            runtimeInputMap = map;
        }

        /// <summary>
        /// Save the user changed input scheme
        /// </summary>
        public void SaveCustomControls()
        {
            var path = Path.Combine(SaveManager.SaveLocation, playerInputMap);

            if (File.Exists(path))
                File.Delete(path);

            FileStream stream = File.Create(path);
            string content = runtimeInputMap.ToJson();
            byte[] buffer = Encoding.ASCII.GetBytes(content);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
            stream.Close();
        }

        public float GetAxis(InputButtonAxis axis)
        {
            var binding = runtimeInputMap.GetBinding(axis);
            if (!(binding is null))
            {
                bool neg = UInput.GetKey(binding.negative);
                bool pos = UInput.GetKey(binding.positive);
                if (neg && pos)
                {
                    switch (binding.fightOutcome)
                    {
                        case AxisFightOutcome.Negative:
                            return -1;
                        case AxisFightOutcome.Neither:
                            return 0;
                        case AxisFightOutcome.Positive:
                            return 1;
                    }
                }
                return neg ? -1 : pos ? 1 : 0;
            }
            Debug.LogError("Input Button Axis not set up: " + axis.ToString());
            return 0;
        }

        public float GetAxis(string axis)
        {
            var binding = runtimeInputMap.GetBinding(axis);
            if (!(binding is null))
            {
                bool neg = UInput.GetKey(binding.negative);
                bool pos = UInput.GetKey(binding.positive);
                if (neg && pos)
                {
                    switch (binding.fightOutcome)
                    {
                        case AxisFightOutcome.Negative:
                            return -1;
                        case AxisFightOutcome.Neither:
                            return 0;
                        case AxisFightOutcome.Positive:
                            return 1;
                    }
                }
                return neg ? -1 : pos ? 1 : 0;
            }
            Debug.LogError("Input Button Axis not set up: " + axis);
            return 0;
        }
        
        public bool GetButton(string button)
        {
            var binding = runtimeInputMap.GetBinding(button);
            if (!(binding is null))
            {
                return UInput.GetKey(binding.positive) || UInput.GetKey(binding.negative);
            }
            Debug.LogError("Button does not exist: " + button);
            return false;
        }

        public bool GetButtonDown(string button)
        {
            var binding = runtimeInputMap.GetBinding(button);
            if (!(binding is null))
            {
                return UInput.GetKeyDown(binding.positive) || UInput.GetKeyDown(binding.negative);
            }
            Debug.LogError("Button does not exist: " + button);
            return false;
        }

        public bool GetButtonUp(string button)
        {
            var binding = runtimeInputMap.GetBinding(button);
            if (!(binding is null))
            {
                return UInput.GetKeyUp(binding.positive) || UInput.GetKeyUp(binding.negative);
            }
            Debug.LogError("Button does not exist: " + button);
            return false;
        }

        public bool GetMouseButton(int button)
            => UInput.GetMouseButton(button);

        public bool GetMouseButtonDown(int button)
            => UInput.GetMouseButtonDown(button);

        public bool GetMouseButtonUp(int button)
            => UInput.GetMouseButtonUp(button);

        public float GetMouseX()
            => UInput.GetAxis("Mouse X");

        public float GetMouseY()
            => UInput.GetAxis("Mouse Y");
    }
}