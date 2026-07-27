using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue
{
    /// <summary>
    /// 単一の選択肢データを表す構造体
    /// </summary>
    [System.Serializable]
    public struct ChoiceData
    {
        [Tooltip("選択肢ボタンに表示されるテキスト")]
        public string choiceText;

        [Tooltip("この選択肢を選んだ時に遷移する先のノードID")]
        public string targetNodeID;
    }

    /// <summary>
    /// 会話または分岐の最小単位(ノード)を表すデータ構造
    /// </summary>
    [System.Serializable]
    public class DialogueNode
    {
        [Tooltip("ノードを一意に識別するID。ノードエディタが自動生成するGUID等を想定")]
        public string nodeID;

        [Tooltip("話者の識別キー。名前の表示や、立ち絵、吹き出し位置の特定に使用")]
        public string speakerID;

        [Tooltip("テキスト内コマンド(タグ)を含む日本語の会話テキスト")]
        [TextArea(3, 5)]
        public string rawText;

        [Tooltip("選択肢がない(一本道)場合の、次に遷移するノードのID")]
        public string nextNodeID;

        [Tooltip("このノードが保持する選択肢のリスト(からなら一本道の会話)")]
        public List<ChoiceData> choices = new List<ChoiceData>();

        #region エディタツール用の拡張パラメータ
        [HideInInspector]
        [Tooltip("将来ノードエディタで作った際の、グラフ上の配置座標を保存するよう")]
        public Vector2 graphPosition;
        #endregion
    }
}
