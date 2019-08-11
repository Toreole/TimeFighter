using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UInput = UnityEngine.Input;

namespace Game.Controller.Input
{
    public class PlayerInput : MonoBehaviour
    {
        //Just to avoid having to do something like Find()
        public static PlayerInput Instance { get; private set; }

        //TODO: bool usesController = false; -> Init that in the PlayerInput setup scene.

        [SerializeField]
        protected TextAsset fallbackInputMap;
        
        //the input map created from the json files in the start.
        protected InputMap runtimeInputMap;
        public InputMap RuntimeInputMap => runtimeInputMap;

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
                var userInput = InputMap.FromJson(content);
                //TODO: Instead of completely overwriting the user created input map, add any bindings to the map that arent in there yet! This will be great with cross versions.
                if (userInput.versionID == GameInfo.Version)
                    runtimeInputMap = userInput;
                else
                    runtimeInputMap = InputMap.FromJson(fallbackInputMap);
                stream.Flush();
                stream.Close();
            }
            else
            {
                //else set the default stuff.
                runtimeInputMap = InputMap.FromJson(fallbackInputMap);
            }
            DontDestroyOnLoad(gameObject);
            IsControllerPresent(); 
        }

        //With this i could detect whether a joystick is present.
        public static bool IsControllerPresent()
        {
            var joysticks = UInput.GetJoystickNames();
            if(joysticks.Length > 0)
            {
                var nEmpty = string.IsNullOrEmpty(joysticks[0]);
                Debug.Log(joysticks[0] + " - " + nEmpty.ToString());
                return !nEmpty; 
            }
            return false;
        }

        //just for saving new maps.
        internal void OverrideInputMap(InputMap map)
        {
            runtimeInputMap = map;
        }
        public void OverrideInputBind(string bindName, KeyCode key, bool positive)
        {
            var bind = runtimeInputMap.GetBinding(bindName);
            if (bind is null)
                return;
            if (positive)
                bind.positive = key;
            else
                bind.negative = key;
        }
        
        public bool HasDuplicateBinds(out List<DuplicateKeyBind> duplicates)
        {
            duplicates = new List<DuplicateKeyBind>();
            //loop through the stuff
            for(int i = 0; i < runtimeInputMap.bindings.Length; i++)
            {
                var original = runtimeInputMap.bindings[i];
                for(int j = 0; j < runtimeInputMap.bindings.Length; j++)
                {
                    //dont compare itself lol
                    if (i == j)
                        continue;

                    var second = runtimeInputMap.bindings[j];
                    var dup = new DuplicateKeyBind
                    {
                        bindName = original.name,
                        positiveKeyIsDuplicate = original.positive == KeyCode.None? false : original.positive == second.positive || original.positive == second.negative,
                        negativeKeyIsDuplicate = original.negative == KeyCode.None? false : original.negative == second.negative || original.negative == second.positive
                    };
                    if (dup.positiveKeyIsDuplicate || dup.negativeKeyIsDuplicate)
                        duplicates.Add(dup);
                }
            }
            return duplicates.Count > 0;
        }

        public void ResetInputMap()
        {
            runtimeInputMap = InputMap.FromJson(fallbackInputMap);
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