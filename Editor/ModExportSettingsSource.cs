using REPOLib.Objects.Sdk;
using SingularityGroup.HotReload;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace REPOLibSdk.Editor
{
    public class ModExportSettingsSource : ScriptableObject
    {
        [SerializeField]
        private List<ModExportSettings> _settings = new List<ModExportSettings>();

        public static ModExportSettings GetSettings(Mod mod)
        {
            return GetInstance().GetSettingsInternal(mod);
        }
        
        private ModExportSettings GetSettingsInternal(Mod mod)
        {
            var settings = _settings.Find(settings => settings.Mod == mod);
            if (settings != null) return settings;
            
            ClearUnusedSettings();
            
            settings = CreateInstance<ModExportSettings>();
            settings.Mod = mod;
            settings.name = mod.FullName;
            
            AssetDatabase.AddObjectToAsset(settings, this);
            
            _settings.Add(settings);
            return settings;
        }

        private void ClearUnusedSettings()
        {
            for (int i = 0; i < _settings.Count; i++)
            {
                var settings = _settings[i];
                if (settings.Mod != null) continue;
                
                AssetDatabase.RemoveObjectFromAsset(settings);
                DestroyImmediate(settings, allowDestroyingAssets: true);
                
                _settings.RemoveAt(i);
                i--;
            }
        }

        private static ModExportSettingsSource GetInstance()
        {
            ModExportSettingsSource[] sources = AssetDatabase.FindAssets("t:ModExportSettingsSource")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ModExportSettingsSource>)
                .ToArray();

            switch (sources.Length)
            {
                case 0:
                    Log.Info("No ModExportSettingsSource found, creating a new one.");
                    
                    var source = CreateInstance<ModExportSettingsSource>();
                    AssetDatabase.CreateAsset(source, "Assets/ModExportSettingsSource.asset");
                    return source;
                
                case 1:
                    return sources[0];
                
                default:
                    Debug.LogWarning("Multiple ModExportSettingsSources exist in the project!");
                    return sources[0];
            }
        }
    }
}
