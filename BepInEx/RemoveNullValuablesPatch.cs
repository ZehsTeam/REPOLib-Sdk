using HarmonyLib;
using Nomnom.BepInEx.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Type _prefabRefType = AccessTools.TypeByName("PrefabRef");

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
                CheckLevel(level);
            }
        }

        private static void CheckLevel(object level)
        {
            IEnumerable<object> presets = (IEnumerable<object>)AccessTools.Field(_levelType, "ValuablePresets").GetValue(level);

            foreach (object preset in presets)
            {
                CheckLevelValuables(preset);
            }
        }

        private static void CheckLevelValuables(object preset)
        {
            foreach (string fieldName in _levelValuablesFields)
            {
                CheckValuableList(preset, fieldName);
            }
        }

        private static void CheckValuableList(object preset, string fieldName)
        {
            var field = AccessTools.Field(_levelValuablesType, fieldName);
            var valuablesObj = field.GetValue(preset);

            if (valuablesObj is not IEnumerable valuablesEnumerable)
            {
                Debug.LogWarning($"[REPOLib-Sdk] RemoveNullValuablesPatch: Field \"{fieldName}\" on {_levelValuablesType.Name} is not enumerable!");
                return;
            }

            // Create a mutable list to remove invalid items
            var valuablesList = valuablesEnumerable.Cast<object>().ToList();

            for (int i = valuablesList.Count - 1; i >= 0; i--)
            {
                var valuable = valuablesList[i];

                if (valuable == null)
                {
                    valuablesList.RemoveAt(i);
                    continue;
                }

                var resourcePathProp = AccessTools.Property(_prefabRefType, "ResourcePath");
                string resourcePath = (string)resourcePathProp.GetValue(valuable);

                if (Resources.Load<GameObject>(resourcePath) == null)
                {
                    valuablesList.RemoveAt(i);
                }
            }

            // If the original field is a List<T>, update it
            if (valuablesObj is IList originalList)
            {
                originalList.Clear();

                foreach (var v in valuablesList)
                {
                    originalList.Add(v);
                }
            }
        }
    }
}
