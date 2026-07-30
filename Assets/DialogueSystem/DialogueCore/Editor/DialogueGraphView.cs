using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Runtime.Dialogue.Core;

namespace DialogueSystem.Editor
{
    public class DialogueGraphView : GraphView
    {
        public DialogueGraphView()
        {
            SetupGridBackground();

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            SetupContextMenu();
        }

        private void SetupGridBackground()
        {
            var gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void SetupContextMenu()
        {
            this.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                evt.menu.AppendAction("Create Node", (menuAction) =>
                {
                    Vector2 localPosition = this.contentViewContainer.WorldToLocal(evt.mousePosition);
                    CreateNode("New Dialogue Node", localPosition);
                });
            });
        }

        public void CreateNode(string nodeName, Vector2 position)
        {
            var nodeData = new DialogueNode
            {
                nodeID = Guid.NewGuid().ToString(),
                dialogueText = "ここにセリフを入力",
                graphPosition = position
            };

            var nodeView = new DialogueNodeView(nodeData);
            nodeView.SetPosition(new Rect(position, new Vector2(250, 200)));

            AddElement(nodeView);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        public DialogueNodeView CreateNodeView(DialogueNode nodeData)
        {
            var nodeView = new DialogueNodeView(nodeData);
            nodeView.SetPosition(new Rect(nodeData.graphPosition, new Vector2(250, 200)));
            AddElement(nodeView);
            return nodeView;
        }

        /// <summary>
        /// グラフ内の全要素（ノード・エッジ等）を安全に削除するユーティリティ
        /// </summary>
        public void ClearGraph()
        {
            var elements = this.graphElements.ToList();
            foreach (var e in elements)
            {
                try
                {
                    this.RemoveElement(e);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DialogueGraphView.ClearGraph] RemoveElement failed: {ex}");
                }
            }
        }

        /// <summary>
        /// ノードの表示更新やレイアウトを強制する補助（ロード後に呼ぶ）
        /// </summary>
        public void RefreshAll()
        {
            var nodeViews = this.graphElements.ToList().OfType<DialogueNodeView>().ToList();
            foreach (var nv in nodeViews)
            {
                try
                {
                    nv.RefreshPorts();
                    nv.RefreshExpandedState();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DialogueGraphView.RefreshAll] node refresh failed: {ex}");
                }
            }
            this.MarkDirtyRepaint();
        }
    }
}
