using System;
using System.Collections.Generic;
using Game.Controller.Input;
using Luminosity.IO;

namespace Game
{
    public static partial class Util
    {
        public static bool HasDuplicates(this ControlScheme scheme, out List<DuplicateKeyBind> duplicates)
        {
            duplicates = new List<DuplicateKeyBind>();
            List<Luminosity.IO.InputBinding> bindings = new List<Luminosity.IO.InputBinding>();

            //Filter only the first bindings. (those are mapped to keyboard / mouse buttons)
            foreach(var axis in scheme.Actions)
            {
                var temp = axis.Bindings[0];
                if (temp.Type == InputType.Button || temp.Type == InputType.DigitalAxis)
                    bindings.Add(temp);
            }

            //start searching for duplicates
            for(int i = 0; i < bindings.Count; i++)
            {
                var original = bindings[i];
                bool originalIsButton = original.Type == InputType.Button;
                var dupe = new DuplicateKeyBind();
                //go through all other bindings
                for(int j = 0; j < bindings.Count; j++)
                {
                    if (i == j)
                        continue;
                    //check for duplicate binds.
                    var comparer = bindings[j];
                    bool comparerIsButton = comparer.Type == InputType.Button;

                    dupe.positiveKeyIsDuplicate = (original.Positive == comparer.Positive)
                        || (comparerIsButton ? false : original.Positive == comparer.Negative);

                    dupe.negativeKeyIsDuplicate = 
                        originalIsButton || comparerIsButton ? false : original.Negative == comparer.Negative
                        || original.Negative == comparer.Positive;

                    //if one of the keys is a duplicate, add the bind.
                    if(dupe.positiveKeyIsDuplicate || dupe.negativeKeyIsDuplicate)
                    {
                        dupe.bindName = scheme.Actions[i].Name;
                        duplicates.Add(dupe);
                    }
                }
            }
            return duplicates.Count > 0;
        }
    }
}
