using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Runtime.Dialogue;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Logic;

namespace Runtime.Dialogue.Branching
{
    [HandlerInfo(description: "プレイヤーが画面上のボタンをクリックして選択する、標準的な選択肢を表示します。", usage: "ノードエディタ上で、選択肢のBranchTypeに「DefaultChoiceHandler」を指定してください。")]
    public class DefaultChoiceHandler : MonoBehaviour, IDialogueBranchHandler
    {
        public int Priority => 0; // 最低優先度（フォールバック用）

        private void Start()
        {
            if (DialogueBranchDispatcher.Instance != null)
                DialogueBranchDispatcher.Instance.RegisterHandler(this);
        }

        public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
        {
            var view = DialogueManager.Instance.CurrentView;
            if (view != null)
            {
                // 1. まず branchType で絞り込む
                var targetChoices = choices.Where(c =>
                    c.branchType == nameof(DefaultChoiceHandler) ||
                    c.branchType == "DefaultChoice").ToList();

                if (targetChoices.Count == 0) return false;

                // 2. 追加：FlagManager による条件評価でさらにフィルタリングする
                var displayChoices = new List<ChoiceData>();
                foreach (var choice in targetChoices)
                {
                    // 条件が設定されていない、または条件を満たしている場合のみ有効とする
                    if (string.IsNullOrEmpty(choice.conditionKey) ||
                        (FlagManager.Instance != null && FlagManager.Instance.EvaluateCondition(choice.conditionKey, choice.conditionOperator, choice.conditionValue)))
                    {
                        displayChoices.Add(choice);
                    }
                }

                // もし画面に出せる選択肢が1つもない場合は、このハンドラーでは処理できないとして false を返す
                if (displayChoices.Count == 0) return false;

                // 抽出したリストだけをUIに渡してボタンを作る
                view.ShowChoices(displayChoices, (selectedIndex) =>
                {
                    view.HideChoices();
                    // 選ばれたボタンの遷移先IDを取得して進行
                    string nextID = displayChoices[selectedIndex].targetNodeID;
                    onBranchDecided?.Invoke(nextID);
                });
                return true;
            }
            return false;
        }
    }
}