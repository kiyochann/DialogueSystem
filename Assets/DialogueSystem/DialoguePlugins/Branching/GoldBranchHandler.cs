using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;

public class GoldBranchHandler : MonoBehaviour, IDialogueBranchHandler
{
    public int Priority => 80;

    [SerializeField] private int playerGold = 1000; // 現在の所持金

    private void Start()
    {
        if (DialogueBranchDispatcher.Instance != null)
            DialogueBranchDispatcher.Instance.RegisterHandler(this);
    }

    public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
    {
        foreach (var choice in choices)
        {
            // エディタで「Gold」と設定された選択肢の判定処理
            if (choice.conditionKey == "Gold")
            {
                // エディタで入力した数値(conditionValue)を直接チェック
                if (playerGold >= choice.conditionValue)
                {
                    Debug.Log($"[GoldBranch] 所持金クリア ({playerGold} >= {choice.conditionValue})");
                    onBranchDecided?.Invoke(choice.targetNodeID);
                    return true; // 分岐実行
                }
            }
        }

        return false; // 条件を満たさない場合は次のハンドラー（通常選択肢表示など）へ
    }
}