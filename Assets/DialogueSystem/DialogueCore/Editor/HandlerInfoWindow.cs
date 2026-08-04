using System.Reflection;
using UnityEditor;
using UnityEngine;

using Runtime.Dialogue;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;

namespace DialogueSystem.Editor
{
    public class HandlerInfoWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        // ウィンドウを開く処理
        public static void ShowWindow()
        {
            var window = GetWindow<HandlerInfoWindow>("Handler Info");
            window.minSize = new Vector2(300, 400);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // コマンド（演出）一覧
            EditorGUILayout.LabelField("【 演出コマンド 】", EditorStyles.boldLabel);
            DrawHandlerList<IDialogueCommandHandler>();

            EditorGUILayout.Space(15);

            // 分岐一覧
            EditorGUILayout.LabelField("【 分岐ハンドラー 】", EditorStyles.boldLabel);
            DrawHandlerList<IDialogueBranchHandler>();

            EditorGUILayout.EndScrollView();
        }

        // 指定したインターフェースを継承するクラスをリスト化して描画
        private void DrawHandlerList<T>()
        {
            var types = TypeCache.GetTypesDerivedFrom<T>();
            bool hasAny = false;

            foreach (var t in types)
            {
                if (t.IsAbstract || t.IsInterface) continue;
                hasAny = true;

                // [HandlerInfo] 属性を取得
                var attr = t.GetCustomAttribute<HandlerInfoAttribute>();

                EditorGUILayout.BeginHorizontal();

                // Handlerの名前（クラス名）を表示
                EditorGUILayout.LabelField($"・ {t.Name}", GUILayout.Width(200));

                // 説明(attr)が存在する場合のみ「詳細」ボタンを表示する
                if (attr != null)
                {
                    if (GUILayout.Button("詳細", GUILayout.Width(50)))
                    {
                        // 詳細ボタンが押されたらポップアップで使い方を表示
                        ShowDetailDialog(t.Name, attr);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (!hasAny)
            {
                EditorGUILayout.LabelField("  （登録されていません）");
            }
        }

        private void ShowDetailDialog(string className, HandlerInfoAttribute attr)
        {
            string msg = $"【説明】\n{attr.Description}\n\n";
            if (!string.IsNullOrEmpty(attr.Usage))
            {
                msg += $"【使い方】\n{attr.Usage}";
            }

            EditorUtility.DisplayDialog($"{className} の詳細", msg, "閉じる");
        }
    }
}