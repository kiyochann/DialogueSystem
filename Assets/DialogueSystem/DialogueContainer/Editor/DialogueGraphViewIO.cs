using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace UnityEditor.Dialogue
{
    using Runtime.Dialogue;

    /// <summary>
    /// DialogueGraphView と DialogueContainer（アセット）間のデータ入出力（セーブ・ロード）を専門に行う静的クラス
    /// </summary>
    public static class DialogueGraphViewIO
    {
        /// <summary>
        /// キャンバス上のノード配置と配線情報をアセットへ保存します。
        /// </summary>
        public static void Save(DialogueGraphView graphView, DialogueContainer container)
        {
            container.allNodes.Clear();

            List<Edge> edges = graphView.graphElements.ToList().OfType<Edge>().ToList();
            List<DialogueNodeView> nodeViews = graphView.graphElements.ToList().OfType<DialogueNodeView>().ToList();

            foreach (DialogueNodeView nodeView in nodeViews)
            {
                DialogueNode nodeData = nodeView.NodeData;
                nodeData.graphPosition = nodeView.GetPosition().position;
                nodeData.nextNodeID = string.Empty;

                // 選択肢の遷移先IDを初期化
                for (int i = 0; i < nodeData.choices.Count; i++)
                {
                    var choice = nodeData.choices[i];
                    choice.targetNodeID = string.Empty;
                    nodeData.choices[i] = choice;
                }

                // 出力ポート（一本道用・選択肢用）の配線を解析
                var outputPorts = nodeView.outputContainer.Query<Port>().ToList();
                foreach (var port in outputPorts)
                {
                    Edge connectedEdge = edges.FirstOrDefault(edge => edge.output == port);
                    if (connectedEdge != null && connectedEdge.input != null)
                    {
                        if (connectedEdge.input.node is DialogueNodeView targetNodeView)
                        {
                            DialogueNode targetData = targetNodeView.NodeData;

                            // 選択肢ポートの場合
                            if (port.userData is int choiceIndex && choiceIndex < nodeData.choices.Count)
                            {
                                var choice = nodeData.choices[choiceIndex];
                                choice.targetNodeID = targetData.nodeID;
                                nodeData.choices[choiceIndex] = choice;
                            }
                            // 通常のOutポート（一本道）の場合
                            else if (port.userData == null)
                            {
                                nodeData.nextNodeID = targetData.nodeID;
                            }
                        }
                    }
                }

                container.allNodes.Add(nodeData);
            }

            // X座標が最も左にあるノードを自動的にスタートノードに指定
            if (container.allNodes.Count > 0)
            {
                var startNode = container.allNodes.OrderBy(n => n.graphPosition.x).First();
                container.startNodeID = startNode.nodeID;
            }
        }

        /// <summary>
        /// アセットからノード情報と配線情報をキャンバスへ読み込み（復元）します。
        /// </summary>
        public static void Load(DialogueGraphView graphView, DialogueContainer container)
        {
            if (container.allNodes == null || container.allNodes.Count == 0) return;

            var viewCache = new Dictionary<string, DialogueNodeView>();

            // 1. すべてのノードオブジェクトを生成
            foreach (DialogueNode nodeData in container.allNodes)
            {
                var nodeView = graphView.CreateNodeView(nodeData);
                viewCache.Add(nodeData.nodeID, nodeView);

                // 選択肢ポートの復元
                if (nodeData.choices != null && nodeData.choices.Count > 0)
                {
                    for (int i = 0; i < nodeData.choices.Count; i++)
                    {
                        nodeView.AddChoicePort(nodeData.choices[i], i);
                    }
                }
            }

            // 2. ノード間の配線（Edge）を復元
            foreach (DialogueNode nodeData in container.allNodes)
            {
                if (!viewCache.TryGetValue(nodeData.nodeID, out DialogueNodeView sourceView)) continue;

                // 一本道ルートの復元
                if (!string.IsNullOrEmpty(nodeData.nextNodeID) && viewCache.TryGetValue(nodeData.nextNodeID, out DialogueNodeView targetView))
                {
                    Port outputPort = sourceView.outputContainer.Children().OfType<Port>().FirstOrDefault(p => p.userData == null);
                    Port inputPort = targetView.inputContainer.Q<Port>();

                    if (outputPort != null && inputPort != null)
                    {
                        Edge edge = outputPort.ConnectTo(inputPort);
                        graphView.AddElement(edge);
                    }
                }

                // 選択肢分岐ルートの復元
                if (nodeData.choices != null && nodeData.choices.Count > 0)
                {
                    var choicePorts = sourceView.outputContainer.Query<Port>().ToList();
                    for (int i = 0; i < nodeData.choices.Count; i++)
                    {
                        string targetID = nodeData.choices[i].targetNodeID;
                        if (!string.IsNullOrEmpty(targetID) && viewCache.TryGetValue(targetID, out DialogueNodeView branchTargetView))
                        {
                            Port outputPort = choicePorts.FirstOrDefault(p => p.userData is int idx && idx == i);
                            Port inputPort = branchTargetView.inputContainer.Q<Port>();

                            if (outputPort != null && inputPort != null)
                            {
                                Edge edge = outputPort.ConnectTo(inputPort);
                                graphView.AddElement(edge);
                            }
                        }
                    }
                }
            }
        }
    }
}