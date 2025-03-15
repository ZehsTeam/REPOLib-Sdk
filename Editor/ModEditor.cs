using REPOLib.Objects.Sdk;
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

            Util.PropertyField("_name", serializedObject, root);
            Util.PropertyField("_author", serializedObject, root);
            Util.PropertyField("_version", serializedObject, root);
            Util.PropertyField("_description", serializedObject, root);
            Util.PropertyField("_websiteUrl", serializedObject, root);
            Util.PropertyField("_dependencies", serializedObject, root);
            
            root.Add(new VisualElement { style = { height = 8 }});
            
            Util.PropertyField("_icon", serializedObject, root);
            Util.PropertyField("_readme", serializedObject, root);
            Util.PropertyField("_extraFiles", exportSettings, root);

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
    }
}
