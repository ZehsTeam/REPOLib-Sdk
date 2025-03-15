using UnityEditor;
using UnityEngine.UIElements;

namespace REPOLibSdk.Editor
{
    [CustomEditor(typeof(ModExportSettings))]
    public class ModExportSettingsEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return new Label("This file is internal to REPOLib-Sdk and not intended to be edited.");
        }
    }
}
