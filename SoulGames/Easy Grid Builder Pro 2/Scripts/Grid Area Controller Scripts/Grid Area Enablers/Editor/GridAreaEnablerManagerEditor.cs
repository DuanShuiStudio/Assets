using UnityEditor;
using UnityEngine.UIElements;

namespace SoulGames.EasyGridBuilderPro
{
    [CustomEditor(typeof(GridAreaEnablerManager))]
    public class GridAreaEnablerManagerEditor : Editor
    {
        public VisualTreeAsset visualTreeAsset = default;
        private VisualElement root;

        private Label headerField;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            if (visualTreeAsset == null)
            {
                string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
                string scriptDirectory = System.IO.Path.GetDirectoryName(scriptPath);
                string uxmlPath = System.IO.Path.Combine(scriptDirectory, "GridAreaEnablerManagerEditor.uxml");
                visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            }
            visualTreeAsset.CloneTree(root);

            MonoScript script = MonoScript.FromMonoBehaviour((GridAreaEnablerManager)target);
            headerField = root.Q<Label>("Header");
            headerField.RegisterCallback<ClickEvent>(evt => EditorGUIUtility.PingObject(script));

            return root;
        }
    }
}