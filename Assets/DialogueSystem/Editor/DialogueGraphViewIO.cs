using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace UnityEditor.Dialogue
{
    using Runtime.Dialogue;

    /// <summary>
    /// DialogueGraphViewのデータ入出力（セーブ・ロード）を専門に行う静的クラス
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

                    for (int i = 0; i < nodeData.choices.Count; i++)
                    {
                        var c = nodeData.choices[i];
                        c.targetNodeID = string.Empty;
                        nodeData.choices[i] = c;
                    }

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
                                if (port.userData is int choiceIndex)
                                {
                                    var choice = nodeData.choices[choiceIndex];
                                    choice.targetNodeID = targetData.nodeID;
                                    nodeData.choices[choiceIndex] = choice;
                                }
                                else
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

            foreach (DialogueNode nodeData in container.allNodes)
            {
                Node nodeView = graphView.CreateNodeView(nodeData);
                viewCache.Add(nodeData.nodeID, nodeView);

                if (nodeData.choices != null && nodeData.choices.Count > 0)
                {
                    for (int i = 0; i < nodeData.choices.Count; i++)
                    {
                        graphView.AddChoicePort(nodeView, nodeData.choices[i], i);
                    }
                }
            }

            foreach (DialogueNode nodeData in container.allNodes)
            {
                if (!viewCache.ContainsKey(nodeData.nodeID)) continue;
                Node sourceView = viewCache[nodeData.nodeID];

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

                if (nodeData.choices != null && nodeData.choices.Count > 0)
                {
                    var choicePorts = sourceView.outputContainer.Query<Port>().ToList();
                    for (int i = 0; i < nodeData.choices.Count; i++)
                    {
                        string targetID = nodeData.choices[i].targetNodeID;
                        if (!string.IsNullOrEmpty(targetID) && viewCache.TryGetValue(targetID, out Node branchTargetView))
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
