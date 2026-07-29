using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Dialogue
{
    using Runtime.Dialogue;

    /// <summary>
    /// ノードエディタのウィンドウ本体
    /// ツールバーの配置、アセット選択、セーブ＆ロードの呼び出しを管理します。
    /// </summary>
    public class DialogueEditorWindow : EditorWindow
    {
        private DialogueGraphView graphView;
        private ObjectField containerField;
        private DialogueContainer currentContainer;

        [MenuItem("Window/Dialogue Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<DialogueEditorWindow>();
            window.titleContent = new GUIContent("Dialogue Editor");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            ConstructExtensionWindow();
        }

        private void OnDisable()
        {
            if (graphView != null)
            {
                rootVisualElement.Remove(graphView);
            }
        }

        /// <summary>
        /// ウィンドウの基本レイアウト（ツールバー ＋ キャンバス）を構築します。
        /// </summary>
        private void ConstructExtensionWindow()
        {
            // 1. 上部ツールバーエリアの作成
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.height = 30;
            toolbar.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingLeft = 5;

            // 2. セーブボタン
            var saveButton = new Button(SaveData) { text = "Save Data" };
            toolbar.Add(saveButton);

            // 3. アセット（DialogueContainer）選択フィールド
            containerField = new ObjectField("Target Container")
            {
                objectType = typeof(DialogueContainer),
                allowSceneObjects = false
            };
            containerField.style.width = 300;
            containerField.style.marginLeft = 10;

            // アセット選択変更時にグラフを読み直す
            containerField.RegisterValueChangedCallback(evt =>
            {
                LoadData(evt.newValue as DialogueContainer);
            });
            toolbar.Add(containerField);

            rootVisualElement.Add(toolbar);

            // 4. 下部グラフ描画エリア（キャンバス）を作成して全体に広げる
            graphView = new DialogueGraphView();
            graphView.style.flexGrow = 1;
            rootVisualElement.Add(graphView);
        }

        /// <summary>
        /// 指定された DialogueContainer アセットからノード構成を読み込みます。
        /// </summary>
        private void LoadData(DialogueContainer container)
        {
            currentContainer = container;
            graphView.ClearGraph();

            if (currentContainer == null) return;

            graphView.LoadGraph(currentContainer);
        }

        /// <summary>
        /// 現在エディタ上にあるノード配置と接続関係を DialogueContainer へ保存します。
        /// </summary>
        private void SaveData()
        {
            if (currentContainer == null)
            {
                EditorUtility.DisplayDialog("Error", "保存先の Target Container がセットされていません！", "OK");
                return;
            }

            graphView.SaveGraph(currentContainer);

            EditorUtility.SetDirty(currentContainer);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", $"{currentContainer.name} へデータを正常に保存しました！", "OK");
        }
    }
}