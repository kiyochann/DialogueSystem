using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace UnityEditor.Dialogue
{
    using Runtime.Dialogue;

    /// <summary>
    /// DialogueGraphViewのデータ入出力（セーブ・ロード）を専門に行う静的クラス（バグ完全修正版）
    /// </summary>
    public static class DialogueGraphViewIO
    {
        public static void Save(DialogueGraphView graphView, DialogueContainer container)
        {
            container.allNodes.Clear();

            List<Edge> edges = graphView.graphElements.ToList().OfType<Edge>().ToList();
            List<Node> nodes = graphView.graphElements.ToList().OfType<Node>().ToList();

            foreach (Node nodeView in nodes)
            {
                if (nodeView.userData is DialogueNode nodeData)
                {
                    nodeData.graphPosition = nodeView.GetPosition().position;
                    nodeData.nextNodeID = string.Empty;

                    // 選択肢のtargetNodeIDを一度すべて綺麗にリセット
                    for (int i = 0; i < nodeData.choices.Count; i++)
                    {
                        var c = nodeData.choices[i];
                        c.targetNodeID = string.Empty;
                        nodeData.choices[i] = c;
                    }

                    // 右側の出力ポート（一本道用、および選択肢用）を全スキャン
                    var outputPorts = nodeView.outputContainer.Query<Port>().ToList();
                    foreach (var port in outputPorts)
                    {
                        Edge connectedEdge = edges.FirstOrDefault(edge => edge.output == port);
                        if (connectedEdge != null && connectedEdge.input != null)
                        {
                            Node targetNodeView = connectedEdge.input.node as Node;
                            DialogueNode targetData = targetNodeView?.userData as DialogueNode;

                            if (targetData != null)
                            {
                                if (port.userData is int choiceIndex && choiceIndex < nodeData.choices.Count)
                                {
                                    // 【修正】画面上の並び順ではなく、ポートが保持している正確なインデックスに遷移先IDを書き込む
                                    var choice = nodeData.choices[choiceIndex];
                                    choice.targetNodeID = targetData.nodeID;
                                    nodeData.choices[choiceIndex] = choice;
                                }
                                else if (port.userData == null)
                                {
                                    nodeData.nextNodeID = targetData.nodeID;
                                }
                            }
                        }
                    }

                    container.allNodes.Add(nodeData);
                }
            }

            if (container.allNodes.Count > 0)
            {
                var startNode = container.allNodes.OrderBy(n => n.graphPosition.x).First();
                container.startNodeID = startNode.nodeID;
            }
        }

        public static void Load(DialogueGraphView graphView, DialogueContainer container)
        {
            if (container.allNodes == null || container.allNodes.Count == 0) return;

            Dictionary<string, Node> viewCache = new Dictionary<string, Node>();

            // 1. まずすべてのノードを生成
            foreach (DialogueNode nodeData in container.allNodes)
            {
                Node nodeView = graphView.CreateNodeView(nodeData);
                viewCache.Add(nodeData.nodeID, nodeView);

                if (nodeData.choices != null && nodeData.choices.Count > 0)
                {
                    for (int i = 0; i < nodeData.choices.Count; i++)
                    {
                        // 【超重要バグ修正】ループインデックス「i」をポートに正確に渡して復元する
                        graphView.AddChoicePort(nodeView, nodeData.choices[i], i);
                    }
                }
            }

            // 2. 引かれていた線を完全復元
            foreach (DialogueNode nodeData in container.allNodes)
            {
                if (!viewCache.ContainsKey(nodeData.nodeID)) continue;
                Node sourceView = viewCache[nodeData.nodeID];

                // 一本道ルートの復元
                if (!string.IsNullOrEmpty(nodeData.nextNodeID) && viewCache.TryGetValue(nodeData.nextNodeID, out Node targetView))
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
                        if (!string.IsNullOrEmpty(targetID) && viewCache.TryGetValue(targetID, out Node branchTargetView))
                        {
                            // ポートのuserDataに記録されているインデックスと合致するポッチを探して線を結ぶ
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
