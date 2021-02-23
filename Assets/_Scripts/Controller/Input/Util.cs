using System;
using System.Collections.Generic;
using Game.Controller.Input;
using UnityEngine;

namespace Game
{
    //Mostly UI / Input helpers in this part
    public static partial class Util
    {
        /// <summary>
        /// Define some special strings for certain KeyCodes since they look ugly on UI lmao.
        /// </summary>
        public static string GetString(this KeyCode key)
        {
            switch(key)
            {
                case KeyCode.None:
                    return "";

                case KeyCode.Alpha0:
                case KeyCode.Alpha1:
                case KeyCode.Alpha2:
                case KeyCode.Alpha3:
                case KeyCode.Alpha4:
                case KeyCode.Alpha5:
                case KeyCode.Alpha6:
                case KeyCode.Alpha7:
                case KeyCode.Alpha8:
                case KeyCode.Alpha9:
                    return key.ToString().Substring(5);
                    
                case KeyCode.DownArrow:
                    return "Down";
                case KeyCode.UpArrow:
                    return "Up";
                case KeyCode.RightArrow:
                    return "Right";
                case KeyCode.LeftArrow:
                    return "Left";

                case KeyCode.Mouse0:
                    return "LMB";
                case KeyCode.Mouse1:
                    return "RMB";
                case KeyCode.Mouse2:
                    return "MMB";

                case KeyCode.Keypad0:
                case KeyCode.Keypad1:
                case KeyCode.Keypad2:
                case KeyCode.Keypad3:
                case KeyCode.Keypad4:
                case KeyCode.Keypad5:
                case KeyCode.Keypad6:
                case KeyCode.Keypad7:
                case KeyCode.Keypad8:
                case KeyCode.Keypad9:
                    return "Num" + key.ToString().Substring(6);

                case KeyCode.Escape:
                    return "Esc";
                case KeyCode.RightControl:
                    return "RCtrl";
                case KeyCode.LeftControl:
                    return "LCtrl";
                case KeyCode.RightAlt:
                    return "RAlt";
                case KeyCode.LeftAlt:
                    return "LAlt";
                case KeyCode.RightShift:
                    return "RShift";
                case KeyCode.LeftShift:
                    return "LShift"; 
            }
            return key.ToString();
        }

        public static bool InRange(this float x, float min, float max)
            => min <= x && x <= max;

        //public static bool HasDuplicates(this ControlScheme scheme, out List<DuplicateKeyBind> duplicates)
        //{
        //    duplicates = new List<DuplicateKeyBind>();
        //    for (int i = 0; i < scheme.Actions.Count; i++)
        //    {
        //        //check for skip?
        //        InputBinding bind = scheme.Actions[i].Bindings[0];
        //        if (bind.Type != InputType.Button && bind.Type != InputType.DigitalAxis)
        //            continue;
        //        //Debug.Log(bind.Positive.GetString() + " - " + bind.Negative.GetString());
        //        DuplicateKeyBind dupe = new DuplicateKeyBind();
        //        dupe.bindName = scheme.Actions[i].Name;
        //        for(int j = 0; j < scheme.Actions.Count; j++)
        //        {
        //            //dont compare to yourself
        //            if (i == j)
        //                continue;
        //            //type skip condition
        //            InputBinding other = scheme.Actions[j].Bindings[0];
        //            if (other.Type != InputType.Button && other.Type != InputType.DigitalAxis)
        //                continue;

        //            if(other.Type == InputType.Button)
        //            {
        //                if(bind.Type == InputType.Button)
        //                {
        //                    //both are buttons
        //                    dupe.positiveKeyIsDuplicate |= bind.Positive == other.Positive;
        //                    dupe.negativeKeyIsDuplicate = false;
        //                }
        //                else
        //                {
        //                    //only bind is axis
        //                    dupe.positiveKeyIsDuplicate |= bind.Positive == other.Positive;
        //                    dupe.negativeKeyIsDuplicate |= bind.Negative == other.Positive;
        //                }
        //            }
        //            else
        //            {
        //                if (bind.Type == InputType.Button)
        //                {
        //                    //only other is axis
        //                    dupe.positiveKeyIsDuplicate |= bind.Positive == other.Positive || bind.Positive == other.Negative;
        //                    dupe.negativeKeyIsDuplicate = false;
        //                }
        //                else
        //                {
        //                    //both are axis
        //                    dupe.positiveKeyIsDuplicate |= bind.Positive == other.Positive || bind.Positive == other.Negative;
        //                    dupe.negativeKeyIsDuplicate |= bind.Negative == other.Positive || bind.Negative == other.Negative;
        //                }
        //            }
        //            if (dupe.BothAre)
        //                break;
        //        }
        //        //Debug.Log(dupe.positiveKeyIsDuplicate + "-" + dupe.negativeKeyIsDuplicate);
        //        //if dupe has been marked, 
        //        if (dupe.EitherIsDuplicate)
        //            duplicates.Add(dupe);
        //    }
        //    return duplicates.Count > 0;
        //}
    }
}
