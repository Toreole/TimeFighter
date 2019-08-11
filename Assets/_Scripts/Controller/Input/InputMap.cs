using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Controller.Input
{
    [System.Serializable]
    public class InputMap
    {
        public string versionID = "0.1.2019_Alpha_01";
        public InputBinding[] bindings;

        //public string this[string x] => x;

        public InputBinding GetBinding(string name)
        {
            foreach(var x in bindings)
            {
                if(x.name == name)
                    return x;
            }
            return null;        
        }
        public InputBinding GetBinding(InputButtonAxis axis)
        {
            foreach (var x in bindings)
            {
                if (x.axis == axis)
                    return x;
            }
            return null;
        }
        //default input map creator.
        public InputMap()
        {
            bindings = new InputBinding[1];
            bindings[0] = new InputBinding(); 
        }

        public static InputMap FromJson(TextAsset asset)
        {
            return JsonUtility.FromJson<InputMap>(asset.text);
        }
        public static InputMap FromJson(string content)
        {
            return JsonUtility.FromJson<InputMap>(content);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }

    //public class ControllerInputBinding { } ???

    [System.Serializable]
    public class InputBinding
    {
        //public string altControllerInput ?
        public string name;
        public InputButtonAxis axis;
        public KeyCode positive;
        public KeyCode negative;
        public AxisFightOutcome fightOutcome;
        public bool show;

        public InputBinding()
        {
            name = "Horizontal";
            axis = InputButtonAxis.Horizontal;
            positive = KeyCode.D;
            negative = KeyCode.A;
            fightOutcome = AxisFightOutcome.Neither;
        }
    }
    
    public enum AxisFightOutcome
    {
        Positive, Negative, Neither
    }

    public enum InputButtonAxis
    {
        Horizontal, Vertical, None
    }

    public class DuplicateKeyBind
    {
        public string bindName;
        public bool positiveKeyIsDuplicate = false;
        public bool negativeKeyIsDuplicate = false;
    }
}