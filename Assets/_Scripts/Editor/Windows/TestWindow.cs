using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Editor
{
    public class TestWindow : EditorWindow
    {
        //god fucking damnit  
        [MenuItem("Window/Custom/TestWindow")]
        public static void ShowWindow()
        {
            GetWindow<TestWindow>(false, "Test", true);
        }
    }
}