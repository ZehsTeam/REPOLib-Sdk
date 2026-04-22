using REPOLib.Objects.Sdk;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace REPOLibSdk.Editor
{
    [CustomEditor(typeof(Mod))]
    public class ModEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var exportSettings = new SerializedObject(ModExportSettingsSource.GetSettings((Mod)target));
            
            var root = new VisualElement();

            var nameField = new TextField
            {
                label = "Name",
                bindingPath = "_name"
            };
            
            root.Add(nameField);
            
            var helpBox = new HelpBox
            {
                messageType = HelpBoxMessageType.Error
            };
            string modName = serializedObject.FindProperty("_name").stringValue;
            UpdateHelpBoxState(modName, helpBox);
            nameField.RegisterValueChangedCallback(evt =>
            {
                UpdateHelpBoxState(evt.newValue, helpBox);
            });
            
            root.Add(helpBox);
            
            Util.PropertyField("_author", serializedObject, root);
            Util.PropertyField("_version", serializedObject, root);
            Util.PropertyField("_description", serializedObject, root);
            Util.PropertyField("_websiteUrl", serializedObject, root);
            Util.PropertyField("_dependencies", serializedObject, root);
            
            root.Add(new VisualElement { style = { height = 8 }});
            
            Util.PropertyField("_icon", serializedObject, root);
            Util.PropertyField("_readme", serializedObject, root);
            Util.PropertyField("_extraFiles", exportSettings, root);
            Util.PropertyField("_extraBundleFiles", exportSettings, root);

            var button = new Button(() => {
                ExportWindow.Open((Mod)target);
            })
            {
                text = "Export",
                style = { marginTop = 5 }
            };
            root.Add(button);

            return root;
        }

        private static void UpdateHelpBoxState(string name, HelpBox box)
        {
            string error = GetError(name);
            if (string.IsNullOrEmpty(error))
            {
                box.SetVisible(false);
            }
            else
            {
                box.text = error;
                box.SetVisible(true);
            }
            
            return;

            string GetError(string modName)
            {
                if (modName.Contains(" ")) return "Name cannot contain spaces!";
                if (modName.Contains("-")) return "Name cannot contain hyphens!";
                if (modName.Contains(".")) return "Name cannot contain periods!";
                if (modName.Length == 0) return "Name cannot be empty!";
                if (!Regex.IsMatch(modName, "^[a-zA-Z0-9_]+$")) return "Name contains invalid characters!";

                return null;
            }
        }
    }
}
