using UnityEngine;
using UnityEditor;

namespace STerrainSplit
{
    public class SplitterEditorWindow : EditorWindow
    {

        Splitter splitter;

        [MenuItem("Terrain/Split")]
        static void Init()
        {
            SplitterEditorWindow window = GetWindow<SplitterEditorWindow>();

            window.minSize = new Vector2(200f, 200f);
            window.autoRepaintOnSceneChange = true;
            window.titleContent = new GUIContent("Split terrain");
            window.Show();
        }

        void OnGUI()
        {
            //GUILayout.TextField("Text",null);

            if (GUILayout.Button("Split selected terrains"))
            {
                splitter = new Splitter(4, 4);
                splitter.SplitIt();
            }

        }

        

    }
}
