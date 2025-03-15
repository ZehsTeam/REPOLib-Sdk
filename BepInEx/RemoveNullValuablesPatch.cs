using HarmonyLib;
using Nomnom.BepInEx.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLibSdk.BepInEx 
{
    [InjectPatch]
    internal sealed class RemoveNullValuablesPatch
    {
        private static readonly Type _runManagerType = AccessTools.TypeByName("RunManager");
        private static readonly Type _levelType = AccessTools.TypeByName("Level");
        private static readonly Type _levelValuablesType = AccessTools.TypeByName("LevelValuables");

        private static readonly string[] _levelValuablesFields = 
        {
            "tiny",
            "small",
            "medium",
            "big",
            "wide",
            "tall",
            "veryTall"
        };
        
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(_runManagerType, "Awake");
        }

        public static void Prefix(MonoBehaviour __instance)
        {
            Debug.Log("Removing missing objects from valuable presets");
            
            IEnumerable<object> levels = (IEnumerable<object>)AccessTools.Field(_runManagerType, "levels").GetValue(__instance);

            foreach (object level in levels)
            {
                IEnumerable<object> presets = (IEnumerable<object>)AccessTools.Field(_levelType, "ValuablePresets").GetValue(level);

                foreach (object preset in presets)
                {
                    foreach (string fieldName in _levelValuablesFields)
                    {
                        List<GameObject> valuables = (List<GameObject>)AccessTools.Field(_levelValuablesType, fieldName).GetValue(preset);
                        valuables.RemoveAll(obj => obj == null);
                    }
                }
            }
        }
    }
}
