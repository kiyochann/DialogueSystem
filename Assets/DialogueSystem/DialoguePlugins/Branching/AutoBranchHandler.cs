using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;

namespace DialoguePlugins.Branching
{
    [HandlerInfo(description: "プレイヤーの入力を待たずに、自動的に指定されたノードへ分岐します。", usage: "ノードエディタ上で、選択肢のBranchTypeに「AutoBranch」を指定してください。")]
    public class AutoBranchHandler : MonoBehaviour, Runtime.Dialogue.Branching.IDialogueBranchHandler
    {
        public int Priority => 100; // 優先度を高く設定（通常ボタンより先に判定させる）

        private void Start()
        {
            if (Runtime.Dialogue.Branching.DialogueBranchDispatcher.Instance != null)
                Runtime.Dialogue.Branching.DialogueBranchDispatcher.Instance.RegisterHandler(this);
        }

        public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
        {
            foreach (var choice in choices)
            {
                // エディタ側で設定された BranchType が AutoBranch のものを探す
                if (choice.branchType == "AutoBranch")
                {
                    // 必要に応じてここで条件判定（例: 所持金チェックなど）を入れる
                    Debug.Log($"[AutoBranchHandler] 自動分岐を実行: {choice.choiceText}");

                    onBranchDecided?.Invoke(choice.targetNodeID);
                    return true;
                }
            }
            return false;
        }
    }
}