using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Dialogue
{
    using Runtime.Dialogue; // 本編用のデータ構造を参照

    /// <summary>
    /// メニューバーから開くノードエディタのウィンドウ本体（分岐対応・最終決定版）
    /// </summary>
    public class DialogueEditorWindow : EditorWindow
    {
        private DialogueGraphView graphView;
        private ObjectField containerField;
        private DialogueContainer currentContainer;

        // Unityの上部メニューバー（Window > Dialogue Editor）にボタンを追加
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
        /// ウィンドウのレイアウト（ボタン、アセット選択枠、キャンバス）を構築する
        /// </summary>
        private void ConstructExtensionWindow()
        {
            // 1. 上部のツールバーエリアを作成
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.height = 30;
            toolbar.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingLeft = 5;

            // 2. セーブボタンの作成
            var saveButton = new Button(() => SaveData()) { text = "Save Data" };
            toolbar.Add(saveButton);

            // 3. アセット（DialogueContainer）をセットする選択枠の作成
            containerField = new ObjectField("Target Container")
            {
                objectType = typeof(DialogueContainer),
                allowSceneObjects = false
            };
            containerField.style.width = 300;
            containerField.style.marginLeft = 10;

            // アセットが切り替わったらグラフを読み直すイベントを登録
            containerField.RegisterValueChangedCallback(evt =>
            {
                LoadData(evt.newValue as DialogueContainer);
            });
            toolbar.Add(containerField);

            rootVisualElement.Add(toolbar);

            // 4. 下部のグラフ描画エリア（キャンバス）を作成してウィンドウ全体に広げる
            graphView = new DialogueGraphView();
            graphView.style.flexGrow = 1; // 画面いっぱいに広げる
            rootVisualElement.Add(graphView);
        }

        /// <summary>
        /// アセットからノードの繋がりを読み込んでエディタに配置する（ロード）
        /// </summary>
        private void LoadData(DialogueContainer container)
        {
            currentContainer = container;
            graphView.ClearGraph();

            if (currentContainer == null) return;

            // アセットに保存されているノードデータをキャンバス上に復元
            graphView.LoadGraph(currentContainer);
        }

        /// <summary>
        /// 現在エディタ上にあるノードの配置と繋がりをアセットに書き出す（セーブ）
        /// </summary>
        private void SaveData()
        {
            if (currentContainer == null)
            {
                EditorUtility.DisplayDialog("Error", "保存先の Target Container がセットされていません！", "OK");
                return;
            }

            // キャンバス上のノード位置と配線をアセットへ書き込み
            graphView.SaveGraph(currentContainer);

            // アセットを強制保存・変更を確定するUnityエディタ用の処理
            EditorUtility.SetDirty(currentContainer);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", $"{currentContainer.name} へデータを正常に保存しました！", "OK");
        }
    }
}
