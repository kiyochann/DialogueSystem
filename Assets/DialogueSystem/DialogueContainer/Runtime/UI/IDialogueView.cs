using System;
using System.Collections.Generic;

namespace Runtime.Dialogue
{
    /// <summary>
    /// ダイアログUI(表現層)が必ず実装すべき共通窓口(インタフェース)
    /// </summary>
    public interface IDialogueView
    {
        /// <summary>UI画面を開き、初期化する</summary>
        void InitializeView();

        /// <summary>UI画面を完全に閉じる</summary>
        void CloseView();

        /// <summary>
        /// 新しいセリフを表示する(１文字ずつのタイピング演出を開始する)
        /// </summary>
        /// <param name="speakerID">話者のID。View側での名前の切り替えや立ち絵、吹き出し位置の特定に使用</param>
        /// <param name="cleanText">演出タグが取り除かれた、純粋に画面へ表示するテキスト</param>
        /// <param name="onTypingComplete">文字表示がすべて完了した瞬間に、View側からマネージャーへ完了を伝えるコールバック</param>
        void DisplaySentence(string speakerID, string cleanText, Action onTypingComplete);

        /// <summary>現在行っているタイピング演出を強制ストップし、全文を一瞬で表示する</summary>
        void ForceCompleteTyping();

        /// <summary>
        /// 選択肢を画面に生成してプレイヤーに提示する
        /// </summary>
        /// <param name="choices">選択肢データのリスト</param>
        /// <param name="onChoiceSelected">プレイヤーがボタンを押したときに、その要素番号(index)をマネージャーに返すコールバック</param>
        void ShowChoices(List<ChoiceData> choices, Action<int> onChoiceSelected);

        /// <summary>表示されている選択肢UIを画面から消去する</summary>
        void HideChoices();
    }
}