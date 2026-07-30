using UnityEngine;
using UnityEditor;
using System.IO;

namespace DialogueSystem.Editor
{
    public static class DialogueTemplateCreator
    {
        [MenuItem("Assets/Create/Dialogue System/Custom Command Handler", false, 80)]
        public static void CreateCommandHandlerTemplate()
        {
            string template = @"using System;
using System.Collections;
using UnityEngine;
using Runtime.Dialogue;

namespace Runtime.Dialogue.Commands
{
    /// <summary>
    /// カスタム演出コマンドのテンプレート ([my_command:target=value])
    /// </summary>
    public class CustomCommandHandler : MonoBehaviour, IDialogueCommandHandler
    {
        public string TargetCommandName => ""my_command"";

        private void Start()
        {
            if (DialogueEventDispatcher.Instance != null)
            {
                DialogueEventDispatcher.Instance.RegisterHandler(this);
            }
        }

        public void Execute(DialogueCommand command, Action onComplete)
        {
            string targetValue = command.GetString(""target"", ""default_value"");
            float timeValue = command.GetFloat(""time"", 1.0f);

            StartCoroutine(CommandRoutine(timeValue, targetValue, onComplete));
        }

        public void ForceComplete(DialogueCommand command)
        {
            StopAllCoroutines();
        }

        private IEnumerator CommandRoutine(float time, string target, Action onComplete)
        {
            Debug.Log($""[CustomCommand] 開始: target={target}, time={time}"");
            yield return new WaitForSeconds(time);
            Debug.Log(""[CustomCommand] 完了"");
            onComplete?.Invoke();
        }
    }
}";
            CreateScriptAsset("CustomCommandHandler.cs", template);
        }

        [MenuItem("Assets/Create/Dialogue System/Custom Branch Handler", false, 81)]
        public static void CreateBranchHandlerTemplate()
        {
            string template = @"using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;

namespace Runtime.Dialogue.Branching
{
    /// <summary>
    /// カスタム分岐ハンドラーのテンプレート
    /// </summary>
    public class CustomBranchHandler : MonoBehaviour, IDialogueBranchHandler
    {
        public int Priority => 50;

        private void Start()
        {
            if (DialogueBranchDispatcher.Instance != null)
            {
                DialogueBranchDispatcher.Instance.RegisterHandler(this);
            }
        }

        public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
        {
            foreach (var choice in choices)
            {
                if (choice.branchType == BranchType.AutoBranch && choice.conditionKey == ""MyCustomKey"")
                {
                    int playerValue = GetPlayerValueFromGameManager(choice.conditionKey);

                    if (playerValue >= choice.conditionValue)
                    {
                        Debug.Log($""[CustomBranch] 条件達成！ノード '{choice.targetNodeID}' へ遷移します。"");
                        onBranchDecided?.Invoke(choice.targetNodeID);
                        return true;
                    }
                }
            }
            return false;
        }

        private int GetPlayerValueFromGameManager(string key)
        {
            // TODO: ゲーム側のセーブデータやマネージャーから数値を取得する処理を記述
            return 100;
        }
    }
}";
            CreateScriptAsset("CustomBranchHandler.cs", template);
        }

        private static void CreateScriptAsset(string defaultName, string content)
        {
            // 現在選択しているフォルダのパスを取得（未選択ならAssets直下）
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (Directory.Exists(path) == false)
            {
                path = Path.GetDirectoryName(path);
            }

            string fullPath = Path.Combine(path, defaultName);
            fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath); // 同名ファイルがある場合は番号を付与

            File.WriteAllText(fullPath, content, System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();

            // 生成したファイルを選択状態にしてリネームしやすくする
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
            Selection.activeObject = obj;
            EditorUtility.FocusProjectWindow();
        }
    }
}