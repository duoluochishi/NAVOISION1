using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NV.CT.ProtocolManagement.ViewModels.Common;
using NV.CT.ProtocolManagement.ViewModels.Models;

namespace NV.CT.ProtocolManagement.ViewModels.Extensions
{
    public static class NodeExtensions
    {
        public static void Sort<TNode>(this PeremptoryObservableCollection<TNode> collection) where TNode : Node
        {
            List<TNode> sortedList = collection.OrderBy(x => x.NodeName).ToList();//这里用升序
            for (int i = 0; i < sortedList.Count(); i++)
            {
                collection.Move(collection.IndexOf(sortedList[i]), i);
            }
        }

        public static void AddNode(this PeremptoryObservableCollection<Node> nodes, Node node)
        {
            foreach (var n in nodes)
            {
                if (n.NodeId == node.ParentID)
                {
                    n.Children.Add(node);
                    return;
                }
                n.Children.AddNode(node);
            }
        }

        public static Node? FindNode(this PeremptoryObservableCollection<Node> nodes, string nodeId)
        {
            foreach (var n in nodes)
            {
                if (n.NodeId == nodeId)
                {
                    return n;
                }
                var node = n.Children.FindNode(nodeId);
                if (node is not null)
                {
                    return node;
                }
            }
            return null;
        }

        //TODO:待优化
        public static void RemoveNode(this PeremptoryObservableCollection<Node> nodes, string nodeId)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.NodeId == nodeId)
                {
                    nodes.RemoveAt(i);
                    return;
                }
                n.Children.RemoveNode(nodeId);
            }
        }
    }
}
