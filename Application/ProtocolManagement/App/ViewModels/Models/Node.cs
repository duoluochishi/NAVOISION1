using NV.CT.ProtocolManagement.ViewModels.Common;
using NV.CT.ProtocolManagement.ViewModels.Enums;
using NV.CT.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.ProtocolManagement.ApplicationService.Impl;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;

namespace NV.CT.ProtocolManagement.ViewModels.Models
{
    [Serializable]
    /// <summary>
    /// Defines the <see cref="Node" />.
    /// </summary>
    public class Node : BaseViewModel
    {
        private PeremptoryObservableCollection<Node> _children;
        private string _protocolTemplateId;
        private string _nodeName;
        private string _nodeType;
        private string _parentID;
        private string _nodeId;

        private bool _isSelected = false;
        //TODO:
        private Node? _parent;

        public Dictionary<int, Node> nodesDictionary = new Dictionary<int, Node>();

        public Node(NodeType nodeType)
        {
            ProtocolTemplateId = Guid.NewGuid().ToString();
            Children = new PeremptoryObservableCollection<Node>();
            NodeId = Guid.NewGuid().ToString();
            NodeType = nodeType.ToString();
        }

        public Node? Parent
        {
            get => _parent;
            set => SetProperty(ref _parent, value);
        }
        /// <summary>
        /// 节点ID.
        /// </summary>
        public string NodeId
        {
            get => _nodeId;
            set => SetProperty(ref _nodeId, value);
        }
        /// <summary>
        /// 节点名称.
        /// </summary>
        public string NodeName
        {
            get => _nodeName;
            set => SetProperty(ref _nodeName, value);
        }
        /// <summary>
        /// 节点类型
        /// </summary>
        public string NodeType
        {
            get => _nodeType;
            set => _nodeType = value;
        }

        public bool IsFactory
        {
            get
            {
                if (NodeType == ProtocolLayeredName.PROTOCOL_NODE)
                {
                    var template = Global.Instance.ServiceProvider.GetRequiredService<IProtocolApplicationService>().GetProtocolTemplate(ProtocolTemplateId);
                    return template.Protocol.IsFactory;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsAudlt
        {
            get
            {
                if (NodeType == ProtocolLayeredName.PROTOCOL_NODE)
                {
                    var template = Global.Instance.ServiceProvider.GetRequiredService<IProtocolApplicationService>().GetProtocolTemplate(ProtocolTemplateId);
                    return template.Protocol.IsAdult;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsRTD
        {
            get
            {
                if (NodeType == ProtocolLayeredName.RECON_NODE)
                {
                    var template = Global.Instance.ServiceProvider.GetRequiredService<IProtocolApplicationService>().GetProtocolTemplate(ProtocolTemplateId);
                    var reconModel = (from f in template.Protocol.Children
                        from m in f.Children
                        from s in m.Children
                        from r in s.Children
                        where r.Descriptor.Id == NodeId
                        select r).First();
                    return reconModel.IsRTD;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        /// <summary>
        /// 对应ProtocolTemplate的ID.
        /// </summary>
        public string ProtocolTemplateId
        {
            get => _protocolTemplateId;
            set => SetProperty(ref _protocolTemplateId, value);
        }
        /// <summary>
        /// 父节点ID
        /// </summary>
        public string ParentID
        {
            get => _parentID;
            set => SetProperty(ref _parentID, value);
        }
        /// <summary>
        /// 子节点集合.
        /// </summary>
        public PeremptoryObservableCollection<Node> Children
        {
            get => _children;
            set => SetProperty(ref _children, value);
        }

        public Node()
        {

        }
    }
}