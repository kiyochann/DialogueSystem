using System;
using System.Collections.Generic;

namespace Runtime.Dialogue.Branching
{
    /// <summary>
    /// 分岐処理を判定・実行するプラグインのルール
    /// </summary>
    public interface IDialogueBranchHandler
    {
        /// <summary>評価の優先度（数字が大きいほど先に判定される。自動分岐は高く、通常ボタンは低くする）</summary>
        int Priority { get; }

        /// <summary>
        /// このハンドラーが選択肢リストを処理できるか判定し、実行する
        /// </summary>
        /// <param name="choices">現在のノードが持っている選択肢データ</param>
        /// <param name="onBranchDecided">遷移先のノードIDが決まったら呼ぶコールバック</param>
        /// <returns>自分が処理を引き受けた場合は true、他のハンドラーに任せる場合は false</returns>
        bool TryHandleBranch(List<ChoiceData> choices, Action<string> onBranchDecided);
    }
}