using UnityEditor;
using UnityEngine.UIElements;

namespace REPOLibSdk.Editor
{
    [CustomEditor(typeof(ModExportSettingsSource))]
    public class ModExportSettingsSourceEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            return new Label("This file is internal to REPOLib-Sdk and not intended to be edited.");
        }
    }
}
