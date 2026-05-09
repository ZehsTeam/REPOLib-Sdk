using REPOLib.Objects.Sdk;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace REPOLibSdk.Editor
{
    public class ExportWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset _visualTreeAsset;
        
        private static Mod _selectedMod;

        private const string OutputPathPrefsKey = "REPOLib-Sdk_OutputPath";
        
        [MenuItem("Window/REPOLib Exporter")]
        public static void ShowFromMenu()
        {
            Open(null);
        }
        
        public static void Open(Mod mod)
        {
            _selectedMod = mod;
            var window = GetWindow<ExportWindow>();
            window.titleContent = new GUIContent("REPOLib Exporter");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Add(_visualTreeAsset.CloneTree());

            var modField = root.Q<ObjectField>("mod");
            modField.value = _selectedMod;
            
            string savedPath = EditorPrefs.GetString(OutputPathPrefsKey, string.Empty);
            var pathField = root.Q<TextField>("output-path");
            pathField.value = savedPath;
            
            var contentList = root.Q<VisualElement>("content-list");
            var contents = contentList.Q<Foldout>("contents");
            var dependencies = contentList.Q<Foldout>("dependencies");
            
            var noModLabel = contentList.Q("no-mod-label");

            var exportButton = root.Q<Button>("export-button");
            exportButton.clicked += () => {
                PackageExporter.ExportPackage((Mod)modField.value, pathField.value);
            };
            
            var refreshButton = root.Q<Button>("refresh-button");
            refreshButton.clicked += () => {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                RefreshContentList();
            };
            
            modField.RegisterValueChangedCallback(evt => {
                _selectedMod = evt.newValue as Mod;
                RefreshContentList();
            });

            pathField.RegisterValueChangedCallback(evt => {
                EditorPrefs.SetString(OutputPathPrefsKey, evt.newValue);
                UpdateButtonState();
            });

            RefreshContentList();

            return;

            void RefreshContentList()
            {
                contents.Clear();
                dependencies.Clear();
                
                noModLabel.SetVisible(_selectedMod == null);
                contents.SetVisible(_selectedMod != null);
                dependencies.SetVisible(_selectedMod != null);
                
                UpdateButtonState();

                if (_selectedMod == null) return;
                
                var settings = ModExportSettingsSource.GetSettings(_selectedMod);
                (Object, bool)[] extraBundleAssets = settings.ExtraBundleFiles?
                    .Where(f => f != null)
                    .Select(f => (f, true))
                    .ToArray()
                    ?? System.Array.Empty<(Object, bool)>();

                (Object, bool)[] includedAssets = PackageExporter.FindContents(_selectedMod)
                    .Where(tuple => !tuple.Path.EndsWith(".dll"))
                    .Select(tuple => (AssetDatabase.LoadAssetAtPath<Object>(tuple.Path), tuple.IsDependency))
                    .Concat(extraBundleAssets)
                    .Distinct()
                    .OrderBy(tuple => tuple.Item1.GetType().Name)
                    .ThenBy(tuple => tuple.Item1.name)
                    .ToArray();

                int contentCount = 0;
                int dependencyCount = 0;

                foreach ((var asset, bool isDependency) in includedAssets)
                {
                    var type = asset.GetType();
                    if (
                        isDependency &&
                        (typeof(MonoScript).IsAssignableFrom(type) ||
                         typeof(Content).IsAssignableFrom(type) ||
                         type == typeof(Mod))
                    )
                    {
                        continue;
                    }
                        
                    var field = new ObjectField
                    {
                        value = asset
                    };
                    field.SetEnabled(false);

                    if (isDependency) dependencyCount++;
                    else contentCount++;
                        
                    var parent = isDependency ? dependencies : contents;
                    parent.Add(field);
                }

                contents.text = $"Content ({contentCount})";
                dependencies.text = $"Assets ({dependencyCount})";
            }

            void UpdateButtonState()
            {
                exportButton.SetEnabled(
                    _selectedMod != null && 
                    !string.IsNullOrWhiteSpace(pathField.value)
                );
            }
        }
    }
}
