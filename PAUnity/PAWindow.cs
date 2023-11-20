using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PAWindow : EditorWindow {
    [MenuItem("Window/UI Toolkit/PolyArchitect")]
    public static void ShowExample() {
        PAWindow wnd = GetWindow<PAWindow>();
        wnd.titleContent = new GUIContent("PAWindow");
    }
    public void CreateGUI() {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        var paRoot = GameObject.FindObjectOfType<PARoot>();
        var label = new Label(paRoot.GetCSGTreeString());
        root.Add(label);
    }
}