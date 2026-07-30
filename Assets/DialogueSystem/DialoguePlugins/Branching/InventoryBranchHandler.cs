using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Dialogue.Core;
using Runtime.Dialogue.Branching;

public class InventoryBranchHandler : MonoBehaviour, IDialogueBranchHandler
{
    // DefaultChoiceHandler(0)より高く、緊急割り込みより低い優先度
    public int Priority => 50;

    private void Start()
    {
        // ディスパッチャへ自身を登録
        if (DialogueBranchDispatcher.Instance != null)
            DialogueBranchDispatcher.Instance.RegisterHandler(this);
    }

    public bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided)
    {
        foreach (var choice in choices)
        {
            // 例: 「鍵を持っていれば特定のノードへ進む」などの条件判定
            if (choice.choiceText.StartsWith("[ItemCheck]") && HasRequiredItem(choice))
            {
                onBranchDecided?.Invoke(choice.targetNodeID);
                return true; // 分岐を処理したため true を返す
            }
        }
        return false; // 該当しない場合は次のハンドラー（通常選択肢など）へ譲る
    }

    private bool HasRequiredItem(ChoiceData choice)
    {
        // ここにゲーム固有の所持品チェック処理などを記述
        return true;
    }
}