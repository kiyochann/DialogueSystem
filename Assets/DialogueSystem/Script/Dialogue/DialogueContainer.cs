using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Dialogue
{
    /// <summary>
    /// 1つの会話イベント(グラフ全体)を保持するScriptableObjectアセット
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogueContainer", menuName = "Dialogue/Dialogue Container")]
    public class DialogueContainer : ScriptableObject
    {
        [Tooltip("この会話イベントの開始地点(エントリーポイント)となるノードのID")]
        public string startNodeID;

        [Tooltip("この会話イベントに含まれるすべてのノードのリスト")]
        public List<DialogueNode> allNodes = new List<DialogueNode>();

        // 進行マネージャーがIDから高速にノードを検索できるようにするための内部辞書(キャッシュ)
        private Dictionary<string, DialogueNode> nodeCache = new Dictionary<string, DialogueNode>();

        /// <summary>
        /// IDから該当するノードを高速に検索・取得する
        /// </summary>
        public DialogueNode GetNodeByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            // すでに辞書が構築されている場合はキャッシュから高速復帰
            if(nodeCache.Count > 0 && nodeCache.ContainsKey(id))
            {
                return nodeCache[id];
            }

            // 初回アクセス時、またはキャッシュクリア時に辞書を再構築
            nodeCache.Clear();
            foreach(var node in allNodes)
            {
                if(node != null && !nodeCache.ContainsKey(node.nodeID))
                {
                    nodeCache.Add(node.nodeID, node);
                }
            }

            return nodeCache.ContainsKey(id) ? nodeCache[id] : null;
        }

        // Unity上でデータが変更されたり、プロジェクトのロードが走った際にキャッシュを安全にクリアする
        private void OnEnable()
        {
            nodeCache.Clear();
        }
    }
}
