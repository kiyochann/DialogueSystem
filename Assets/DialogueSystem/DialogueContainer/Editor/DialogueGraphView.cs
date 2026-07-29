using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Dialogue
{
    using Runtime.Dialogue;

    /// <summary>
    /// ノードを配置・視覚化する無限キャンバス
    /// ズーム、ドラッグ移動、右クリックでのノード生成メニューを制御します。
    /// </summary>
    public class DialogueGraphView : GraphView
    {
        public DialogueGraphView()
        {
            // ズーム・ドラッグ操作の有効化
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // 背景グリッドの追加
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            ConstructContextMenu();
        }

        /// <summary>
        /// 右クリックメニューの構築
        /// </summary>
        private void ConstructContextMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvent =>
            {
                menuEvent.menu.AppendAction("Create Dialogue Node", actionEvent =>
                {
                    Vector2 mousePos = actionEvent.eventInfo.localMousePosition;
                    Vector2 graphPos = viewTransform.matrix.inverse.MultiplyPoint(mousePos);

                    CreateNodeView(new DialogueNode()
                    {
                        nodeID = Guid.NewGuid().ToString(),
                        speakerID = "New Speaker",
                        rawText = "セリフをここに入力してください",
                        graphPosition = graphPos
                    });
                });
            }));
        }

        /// <summary>
        /// ノードデータをもとに DialogueNodeView を生成してキャンバスに配置します。
        /// </summary>
        public DialogueNodeView CreateNodeView(DialogueNode nodeData)
        {
            var nodeView = new DialogueNodeView(nodeData);
            AddElement(nodeView);
            return nodeView;
        }

        /// <summary>
        /// ポート同士の接続判定（同じノード同士や同方向のポート間接続を拒否）
        /// </summary>
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

        /// <summary>
        /// キャンバス上の全ノードと線を消去します。
        /// </summary>
        public void ClearGraph()
        {
            var elements = graphElements.ToList();
            foreach (var element in elements)
            {
                RemoveElement(element);
            }
        }

        public void SaveGraph(DialogueContainer container) => DialogueGraphViewIO.Save(this, container);
        public void LoadGraph(DialogueContainer container) => DialogueGraphViewIO.Load(this, container);
    }
}