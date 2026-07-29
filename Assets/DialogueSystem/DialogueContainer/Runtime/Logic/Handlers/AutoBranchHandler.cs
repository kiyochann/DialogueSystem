using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue.Branching.Handlers
{
    /// <summary>
    /// [auto:条件] という選択肢を見つけたら、ボタンを出さずに裏で条件判定して自動遷移するハンドラー
    /// </summary>
    public class AutoBranchHandler : MonoBehaviour, IDialogueBranchHandler
    {
        // 普通のボタン表示より先に判定したいので、優先度を高くする
        public int Priority => 100;

        private void Start()
        {
            if (DialogueBranchDispatcher.Instance != null)
                DialogueBranchDispatcher.Instance.RegisterHandler(this);
        }

        public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
        {
            foreach (var choice in choices)
            {
                // エディタ上で "[auto:hp>50]" と書かれているか？
                if (choice.choiceText.StartsWith("[auto:"))
                {
                    bool conditionMet = EvaluateCondition(choice.choiceText);

                    if (conditionMet)
                    {
                        Debug.Log($"[AutoBranch] 条件を満たしたため、自動で分岐します: {choice.choiceText}");
                        onBranchDecided?.Invoke(choice.targetNodeID);
                        return true; // 自動遷移を実行したので、これ以上他の分岐処理（ボタン表示等）はしない
                    }
                }
            }

            // 条件を満たすautoタグが無ければ false を返し、次のハンドラー（普通のボタン等）に任せる
            return false;
        }

        private bool EvaluateCondition(string conditionText)
        {
            // TODO: ここに実際のゲームの変数をチェックする処理を書く
            // (例: PlayerStats.HP >= 50 かどうかをパースして判定する)

            // 今回はテスト用として、"[auto:true]" なら条件クリアとする
            return conditionText.Contains("true");
        }
    }
}