using Microsoft.Extensions.DependencyInjection;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.Language;
using NV.CT.Protocol;
using NV.CT.Protocol.Models;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.ProtocolManagement.ApplicationService.Impl;
using NV.CT.ProtocolManagement.ViewModels.Common;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;
using NV.CT.ProtocolManagement.ViewModels.Enums;
using NV.CT.ProtocolManagement.ViewModels.Extensions;
using NV.CT.ProtocolManagement.ViewModels.Models;
using NV.CT.ProtocolManagement.Views.English;
using NV.CT.UI.ViewModel;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using static System.Formats.Asn1.AsnWriter;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class ProtocolTreeControlViewModel : BaseViewModel
    {
        private readonly IProtocolApplicationService _protocolApplicationService;
        private readonly IDialogService _dialogService;
        private PeremptoryObservableCollection<Node> _nodeList;
        private PeremptoryObservableCollection<Node> _protocols = new();
        private string _nowSelectedProtocolTemplateId;
        private string _nowSelectedNodeType;
        private string _nowSelectedNodeId;
        private Node? _selectedNode;
        private Dictionary<string, object> _tempCopyNodeDictionary = new();
        public Action<string> NodeSelecting;
        public Action Expanding;
        public Action Collapsing;
        public bool IsConfirmed { get; set; } = false;
        public string NewNodeName { get; set; } = string.Empty;
        public PrepareExpandNode TempPrepareExpandNode { get; set; } = new PrepareExpandNode();
        public PeremptoryObservableCollection<Node> NodeList
        {
            get => _nodeList;
            set => SetProperty(ref _nodeList, value);
        }

        public Node? SelectedNode
        {
            get => _selectedNode;
            set => SetProperty(ref _selectedNode, value);
        }

        public Dictionary<string, object> TempCopyNodeDictionary
        {
            get => _tempCopyNodeDictionary;
            set => SetProperty(ref _tempCopyNodeDictionary, value);
        }
        public string LastNodeId { get; set; } = string.Empty;
        public ProtocolTreeControlViewModel(IProtocolApplicationService protocolApplicationService, IDialogService dialogService)
        {
            _protocolApplicationService = protocolApplicationService;
            _dialogService = dialogService;
            TempCopyNodeDictionary.Add(ProtocolLayeredName.SCAN_NODE, null);
            TempCopyNodeDictionary.Add(ProtocolLayeredName.RECON_NODE, null);

            Commands.Add(Constants.COMMAND_PROTOCOL_TREE_SELECTED_CHANGED, new DelegateCommand<object[]>(ProtocolTreeSelecteNodeChanged));
            Commands.Add(Constants.COMMAND_RIGHT_MENU_CLICK, new DelegateCommand<object[]>(RightMenuClick));

            _protocolApplicationService.SelectBodyPartForProtocolChanged += FilterBodyPartForProtocolChanged;
            _protocolApplicationService.ProtocolConditionFilterChanging += OnProtocolConditionFilterChanging;
            _protocolApplicationService.ProtocolOperationClicked += ProtocolOperationClicked;
            _protocolApplicationService.SearchButtonClicked += SearchButtonClicked;
            _protocolApplicationService.InputClick += InputProtocol;
            _protocolApplicationService.NodeMoving += _protocolApplicationService_NodeMoving;
            _protocolApplicationService.NodeRepeating += _protocolApplicationService_NodeRepeating;
            _protocolApplicationService.NodeDeleting += _protocolApplicationService_NodeDeleting;
            _protocolApplicationService.NodeExpanding += _protocolApplicationService_NodeExpanding;
            _protocolApplicationService.NodeCollapsing += _protocolApplicationService_NodeCollapsing;

            _protocolApplicationService.ProtocolChanged += ProtocolApplicationServiceOnProtocolChanged;
            _protocolApplicationService.EmergencyProtocolSwitching += ProtocolApplicationServiceOnEmergencyProtocolSwitching;
            _protocolApplicationService.NewNodeNameSended += OnNewNodeNameSended;

            _protocolApplicationService.ChangeBodyPartForProtocol(CTS.Enums.BodyPart.Head.ToString());
        }

        private void OnProtocolConditionFilterChanging(object? sender, EventArgs<List<ProtocolTemplateModel>> e)
        {
            //将数据加载到Tree中
            LoadAllProtocolToNodeList(e.Data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">新的节点名称</param>
        private void OnNewNodeNameSended(object? sender, EventArgs<string> e)
        {
            //Repeat
            RepeatNode(e.Data);
        }

        private void RepeatNode(string newNodeName)
        {
            ProtocolTemplateModel protocolTemplate = null;
            switch (_nowSelectedNodeType)
            {
                case ProtocolLayeredName.PROTOCOL_NODE:
                    var protocolTemplateID = _protocolApplicationService.Repeat(_nowSelectedProtocolTemplateId, newNodeName);
                    protocolTemplate = _protocolApplicationService.GetProtocolTemplate(protocolTemplateID);
                    //复制新的协议节点
                    ProtocolToUINode(protocolTemplate);
                    RefreshProtocolTree();
                    break;
                case ProtocolLayeredName.SCAN_NODE:
                    string scanID = _protocolApplicationService.RepeatScanTask(_nowSelectedProtocolTemplateId, _nowSelectedNodeId, newNodeName);
                    protocolTemplate = GetNowSeleteProtocolTemplate();
                    ProtocolHelper.ResetParent(protocolTemplate.Protocol);
                    Node? scanNode = new(NodeType.ScanNode);
                    var scanModel = (from f in protocolTemplate.Protocol.Children
                                     from m in f.Children
                                     from s in m.Children
                                     where s.Descriptor.Id == scanID
                                     select s).First();
                    scanNode = scanModel.ModelToNode(protocolTemplate.Descriptor.Id);
                    if (scanNode is not null)
                    {
                        scanNode.ProtocolTemplateId = protocolTemplate.Descriptor.Id;
                        NodeList.AddNode(scanNode);
                    }
                    break;
                case ProtocolLayeredName.RECON_NODE:
                    string reconID = _protocolApplicationService.RepeatReconTask(_nowSelectedProtocolTemplateId, _nowSelectedNodeId, newNodeName);
                    protocolTemplate = GetNowSeleteProtocolTemplate();
                    ProtocolHelper.ResetParent(protocolTemplate.Protocol);
                    Node? reconNode = new(NodeType.ReconNode);
                    var reconModel = (from f in protocolTemplate.Protocol.Children
                                      from m in f.Children
                                      from s in m.Children
                                      from r in s.Children
                                      where r.Descriptor.Id == reconID
                                      select r).First();
                    //var reconModel = GetReconModel(protocolTemplate, reconID);
                    reconNode = reconModel.ModelToNode(protocolTemplate.Descriptor.Id);
                    if (reconNode is not null)
                        NodeList.AddNode(reconNode);
                    break;
            }
        }



        private ScanModel GetScanModel(ProtocolTemplateModel protocolTemplateModel, string scanId)
        {
            return (from f in protocolTemplateModel.Protocol.Children
                from m in f.Children
                from s in m.Children
                where s.Descriptor.Id == scanId
                select s).First();
        }

        private ReconModel GetReconModel(ProtocolTemplateModel protocolTemplateModel, string reconId)
        {
            return  (from f in protocolTemplateModel.Protocol.Children
                from m in f.Children
                from s in m.Children
                from r in s.Children
                where r.Descriptor.Id == reconId
                select r).First();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="protocolTemplateId"></param>
        [UIRoute]
        private void ProtocolApplicationServiceOnProtocolChanged(object? sender, string protocolTemplateId)
        {
            if (string.IsNullOrEmpty(protocolTemplateId)) return;

            LastNodeId = Global.Instance.SelectNodeID;
            //刷新协议
            using (var scope = Global.Instance.ServiceProvider.CreateScope())
            {
                var protocolFilterControlViewModel = scope.ServiceProvider.GetRequiredService<ProtocolFilterControlViewModel>();
                protocolFilterControlViewModel.FilterProtocol();
            }

            //Todo:展开节点，选中节点,待完善
            InitExpandNode(protocolTemplateId, LastNodeId);
        }
        private void InitExpandNode(string protocolTemplateId,string lastNodeId)
        {//System.Windows.Application.Current.Dispatcher.Invoke
            Dispatcher.CurrentDispatcher.Invoke((Action)(()=>{
                //var template = _protocolApplicationService.GetProtocolTemplate(protocolTemplateId);
                //ProtocolToUINode(template);
                //RefreshProtocolTree();
                TempPrepareExpandNode.ProtocolTemplateId = protocolTemplateId;
                if (!TempPrepareExpandNode.ProtocolTemplateId.IsNullOrEmpty())
                {
                    //根据协议Id找到协议节点，展开协议节点
                    //if (TempPrepareExpandNode.SelectedNode.NodeType == ProtocolLayeredName.PROTOCOL_NODE) return;
                    var rootNode = NodeList.FirstOrDefault(t => t.ProtocolTemplateId == protocolTemplateId); 
                    if (rootNode != null) 
                    {
                        TempPrepareExpandNode.SelectedNode= rootNode;
                        NodeSelecting.Invoke(protocolTemplateId);
                    }                    
                    var node = NodeList.FindNode(lastNodeId);
                    if (node != null) 
                    {
     
                        if (node.NodeType == ProtocolLayeredName.PROTOCOL_NODE)
                        {
                            node.IsSelected = true;
                            Expanding.Invoke();
                        }
                        if (node.NodeType == ProtocolLayeredName.SCAN_NODE)
                        {
                            //找到父节点，先选中协议节点并展开，再选中扫描节点
                            var parent = NodeList.FirstOrDefault(t => t.ProtocolTemplateId == node.ProtocolTemplateId);
                            if (parent != null) 
                            {
                                parent.IsSelected = true;
                                Expanding.Invoke();
                                parent.IsSelected = false;
                                node.IsSelected = true;
                            }
                        }
                        if (node.NodeType == ProtocolLayeredName.RECON_NODE)
                        {
                            //找到父节点，先选中协议节点并展开，再选中扫描节点
                            var protocolNode = NodeList.FirstOrDefault(t => t.ProtocolTemplateId == node.ProtocolTemplateId);
                            if (protocolNode != null) 
                            {
                                protocolNode.IsSelected = true;
                                Expanding.Invoke();
                                protocolNode.IsSelected = false;
                                var scanNode = NodeList.FindNode(node.ParentID);
                                if (scanNode != null)
                                {                               
                                    scanNode.IsSelected = true;
                                    scanNode.IsSelected = false;
                                    node.IsSelected = true;
                                }
                            }
                        }
                    }
                    TempPrepareExpandNode = new();
                }
            }));

        }

        [UIRoute]
        private void ProtocolApplicationServiceOnEmergencyProtocolSwitching(object? sender, ProtocolTemplateModel e)
        {
            NodeList.Clear();
            _protocols.Clear();
            ProtocolToUINode(e);
            RefreshProtocolTree();
        }
        /// <summary>
        /// 节点上移或下移
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _protocolApplicationService_NodeMoving(object? sender, EventArgs<string> e)
        {
            switch (e.Data)
            {
                case ProtocolOperation.BTN_UP:
                    MoveUpNowSelectedFromNodeType();
                    break;
                case ProtocolOperation.BTN_DOWN:
                    MoveDownNowSelectedFromNodeType();
                    break;
            }
        }
        private void _protocolApplicationService_NodeRepeating(object? sender, EventArgs e)
        {
            RepeatNowSelectedFromNodeType();
        }
        private void _protocolApplicationService_NodeDeleting(object? sender, EventArgs e)
        {
            DeleteNowSelectedFromNodeType();
        }
        private void _protocolApplicationService_NodeExpanding(object? sender, EventArgs e)
        {
            Expanding.Invoke();
        }

        private void _protocolApplicationService_NodeCollapsing(object? sender, EventArgs e)
        {
            Collapsing.Invoke();
        }

        private void InputProtocol(object? sender, EventArgs<ProtocolTemplateModel> e)
        {
            if (e.Data is null)
            {
                return;
            }
            string protocolName = string.Empty;
            _dialogService.ShowDialog(false, MessageLeveles.Info, "Tip", "Input Successful", arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    arg.Parameters.TryGetValue(DialogParameter.PROTOCOL_NAME, out protocolName);
                    var protocolTemplate = e.Data;
                    ProtocolManagerHelper.ResetId(protocolTemplate);
                    protocolTemplate.Descriptor.Name += "Input";//TODO:等对话窗口创建
                    protocolTemplate.Protocol.Descriptor.Name = protocolName;
                    //导入新的协议
                    _protocolApplicationService.Save(protocolTemplate);
                    //导入协议
                    ProtocolToUINode(protocolTemplate);
                    RefreshProtocolTree();
                }
            }, ConsoleSystemHelper.WindowHwnd);
        }

        [UIRoute]
        /// <summary>
        /// Filter protocols based on the selected body part and display the matching protocols on the tree.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="bodyPartName"></param>
        private void FilterBodyPartForProtocolChanged(object? sender, EventArgs<string> bodyPartName)
        {
            if (bodyPartName.Data is not null)
            {
                var fileModels = GetNowBodyPartProtocol(bodyPartName.Data);
                if (fileModels is null)
                {
                    NodeList = new PeremptoryObservableCollection<Node>();
                    ProtocolTreeSelecteNodeChanged(new object[] { _nowSelectedNodeType, _nowSelectedProtocolTemplateId, _nowSelectedNodeId });
                }
                else
                {
                    LoadAllProtocolToNodeList(fileModels);
                }
            }
        }

        /// <summary>
        /// 根据身体部位 获取所有协议数据
        /// </summary>
        /// <param name="bodyPartName"></param>
        /// <returns></returns>
        private List<ProtocolTemplateModel> GetNowBodyPartProtocol(string bodyPartName)
        {
            return _protocolApplicationService.GetAllProtocolTemplates()
                    .FindAll(protocolTemplate =>
                    protocolTemplate.Protocol.BodyPart.ToString().ToUpper() == bodyPartName.ToUpper() && !protocolTemplate.Protocol.IsEmergency);
        }

        /// <summary>
        /// Conduct operations pertaining to protocols 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProtocolOperationClicked(object? sender, EventArgs<string> e)
        {
            string message = string.Empty;
            switch (e.Data)
            {
                case ProtocolOperation.BTN_EXPORT:
                    var exportFlag = ExportProtocol();
                    message = exportFlag ? "Export protocol successful!" : "Export protocol failed";
                    _dialogService.ShowDialog(false, MessageLeveles.Info, "Tip", message, callback => { }, ConsoleSystemHelper.WindowHwnd);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchButtonClicked(object? sender, EventArgs<string> e)
        {
            if (e.Data is not null)
            {
                RefreshProtocolTree();
                //删除不包含关键字的节点
                for (int index = 0; index < NodeList.Count; index++)
                {
                    if (!NodeList[index].NodeName.Contains(e.Data))
                    {
                        NodeList.Remove(NodeList[index]);
                        index--;
                    }
                }
            }
            else
            {
                //TODO
            }
        }

        /// <summary>
        /// 导出协议
        /// </summary>
        private bool ExportProtocol()
        {
            bool flag = false;
            FolderBrowserDialog dialog = new();
            DialogResult result = dialog.ShowDialog();
            if (result != DialogResult.Cancel)
            {
                var ProtocolTemplateID = _protocolApplicationService.GetProtocolTemplate(_nowSelectedProtocolTemplateId);
                _protocolApplicationService.Export(ProtocolTemplateID, dialog.SelectedPath);
                flag = true;
            }
            return flag;
        }

        /// <summary>
        /// 选中项 右键菜单，Repeat,Delete
        /// </summary>
        /// <param name="SelectedNodeNameAndNodeItself"></param>
        private void RightMenuClick(object[] SelectedNodeNameAndNodeItself)
        {
            string? nodeName = SelectedNodeNameAndNodeItself[0].ToString();
            SelectedNode = SelectedNodeNameAndNodeItself[1] as Node;
            try
            {
                switch (nodeName)
                {
                    case ProtocolOperation.COPY:
                        //TODO
                        if (SelectedNode.NodeType != ProtocolLayeredName.PROTOCOL_NODE) CopyNode();
                        break;
                    case ProtocolOperation.PASTE:
                        //TODO
                        if (SelectedNode.NodeType != ProtocolLayeredName.PROTOCOL_NODE) PasteNode();
                        break;
                    case ProtocolOperation.CUT:
                        //TODO
                        break;
                    case ProtocolOperation.REPEAT:
                        RepeatNowSelectedFromNodeType();
                        break;
                    case ProtocolOperation.DELETE:
                        DeleteNowSelectedFromNodeType();
                        break;
                    case ProtocolOperation.AUTO_SCAN:
                        AutoScanNowSelectedScanTask();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                //TODO
            }
        }

        private void CopyNode()
        {
            switch (SelectedNode.NodeType)
            {
                case ProtocolLayeredName.SCAN_NODE:
                    //将当前ScanModel放在临时字典中
                    TempCopyNodeDictionary[ProtocolLayeredName.SCAN_NODE] = SelectedNode;
                    break;
                case ProtocolLayeredName.RECON_NODE:
                    //将当前ReconModel放在临时字典中
                    TempCopyNodeDictionary[ProtocolLayeredName.RECON_NODE] = SelectedNode;
                    break;
            }
        }

        private void PasteScanNode()
        {
            if (TempCopyNodeDictionary[ProtocolLayeredName.SCAN_NODE] is null) return;
            if (SelectedNode.NodeType != ProtocolLayeredName.SCAN_NODE) return;
            //从临时字典中找到复制的ScanModel
            var copyScanNode = TempCopyNodeDictionary[ProtocolLayeredName.SCAN_NODE] as Node;
            var template1 = GetProtocolTemplateById(copyScanNode.ProtocolTemplateId);
            ProtocolHelper.ResetParent(template1.Protocol);
            var copyScanModel = GetScanModel(template1, copyScanNode.NodeId);

            var nowScanNode = SelectedNode;
            var template2 = GetProtocolTemplateById(nowScanNode.ProtocolTemplateId);
            if (template2.IsFactory) return;

            ProtocolHelper.ResetParent(template2.Protocol);
            var selectedScanModel = GetScanModel(template2, nowScanNode.NodeId);
            //判断身体部位是否相同
            if (copyScanModel.BodyPart != selectedScanModel.BodyPart) return;

            //检查是否重名
            CheckNodeName(nowScanNode);
            if (string.IsNullOrEmpty(NewNodeName)) return;
            //添加节点
            var scanPos = NodeList.FindNode(selectedScanModel.Parent.Parent.Parent.Descriptor.Id).Children.IndexOf(nowScanNode);
            var pasteNode = copyScanNode.Clone();
            pasteNode.NodeId = Guid.NewGuid().ToString().ToLower();
            pasteNode.NodeName = NewNodeName;
            pasteNode.ProtocolTemplateId = nowScanNode.ProtocolTemplateId;
            pasteNode.ParentID = nowScanNode.ParentID;
            //复制粘贴扫描任务时，子节点重建任务的Id也要更新
            pasteNode.Children.ForEach(r =>
            {
                r.NodeId = Guid.NewGuid().ToString().ToLower();
            });
            //pasteNode.Children[0].NodeId = Guid.NewGuid().ToString().ToLower();

            if (NodeList.FindNode(selectedScanModel.Parent.Parent.Parent.Descriptor.Id).Children.Count > scanPos + 1)
            {
                NodeList.FindNode(selectedScanModel.Parent.Parent.Parent.Descriptor.Id).Children.Insert(scanPos + 1, pasteNode);
            }
            else
            {
                NodeList.FindNode(selectedScanModel.Parent.Parent.Parent.Descriptor.Id).Children.Add(pasteNode);
            }

            //更新协议内容
            var pasteScanModel = copyScanModel.Clone();
            pasteScanModel.Descriptor.Id = pasteNode.NodeId;
            pasteScanModel.Descriptor.Name = NewNodeName;
            for (int i = 0; i < pasteNode.Children.Count; i++)
            {
                pasteScanModel.Children[i].Descriptor.Id = pasteNode.Children[i].NodeId;
            }

            var m = selectedScanModel.Parent.Children.IndexOf(selectedScanModel);
            if (m < selectedScanModel.Parent.Children.Count - 1)
            {
                selectedScanModel.Parent.Children.Insert(m + 1, pasteScanModel);
            }
            else
            {
                selectedScanModel.Parent.Children.Add(pasteScanModel);
            }

            _protocolApplicationService.Save(template2);

            TempCopyNodeDictionary[ProtocolLayeredName.SCAN_NODE] = null;
        }

        private void PasteReconNode()
        {
            if (TempCopyNodeDictionary[ProtocolLayeredName.RECON_NODE] is null) return;
            if (SelectedNode.NodeType != ProtocolLayeredName.RECON_NODE) return;
            //从临时字典中找到复制的ScanModel
            var copyReconNode = TempCopyNodeDictionary[ProtocolLayeredName.RECON_NODE] as Node;
            var templateProtocol1 = GetProtocolTemplateById(copyReconNode.ProtocolTemplateId);
            ProtocolHelper.ResetParent(templateProtocol1.Protocol);
            var copyReconModel = GetReconModel(templateProtocol1, copyReconNode.NodeId);

            var nowReconNode = SelectedNode;
            var templateProtocol2 = GetProtocolTemplateById(nowReconNode.ProtocolTemplateId);
            ProtocolHelper.ResetParent(templateProtocol2.Protocol);
            var selectedReconModel = GetReconModel(templateProtocol2, nowReconNode.NodeId);
            //判断身体部位是否相同
            if (copyReconModel.Parent.BodyPart != selectedReconModel.Parent.BodyPart) return;
            //检查是否重名
            CheckNodeName(nowReconNode);
            if (string.IsNullOrEmpty(NewNodeName)) return;
            //添加节点
            var reconPos = NodeList.FindNode(selectedReconModel.Parent.Descriptor.Id).Children.IndexOf(nowReconNode);
            var pasteReconNode = copyReconNode.Clone();
            pasteReconNode.NodeId = Guid.NewGuid().ToString().ToLower();
            pasteReconNode.NodeName = NewNodeName;
            pasteReconNode.ProtocolTemplateId = nowReconNode.ProtocolTemplateId;
            pasteReconNode.ParentID = nowReconNode.ParentID;

            if (NodeList.FindNode(selectedReconModel.Parent.Descriptor.Id).Children.Count > reconPos + 1)
            {
                NodeList.FindNode(selectedReconModel.Parent.Descriptor.Id).Children.Insert(reconPos + 1, pasteReconNode);
            }
            else
            {
                NodeList.FindNode(selectedReconModel.Parent.Descriptor.Id).Children.Add(pasteReconNode);
            }

            //更新协议内容
            var pasteReconModel = copyReconModel.Clone();
            pasteReconModel.Descriptor.Id = pasteReconNode.NodeId;
            pasteReconModel.Descriptor.Name = NewNodeName;
            pasteReconModel.Parent = selectedReconModel.Parent;

            var k = selectedReconModel.Parent.Children.IndexOf(selectedReconModel);
            if (k < selectedReconModel.Parent.Children.Count - 1)
            {
                selectedReconModel.Parent.Children.Insert(k + 1, pasteReconModel);
            }
            else
            {
                selectedReconModel.Parent.Children.Add(pasteReconModel);
            }

            _protocolApplicationService.Save(templateProtocol2);

            TempCopyNodeDictionary[ProtocolLayeredName.RECON_NODE] = null;
        }

        private void PasteNode()
        {
            switch (SelectedNode.NodeType)
            {
                case ProtocolLayeredName.SCAN_NODE:
                    PasteScanNode();
                    break;
                case ProtocolLayeredName.RECON_NODE:
                    PasteReconNode();
                    break;
            }

            IsConfirmed = false;
            NewNodeName = string.Empty;
        }

        private void MoveUpNowSelectedFromNodeType()
        {
            //如果是Protocol节点，则不能移动
            if (SelectedNode.NodeType == ProtocolLayeredName.PROTOCOL_NODE) return;
            //如果是首节点，则不能移动
            var template = GetProtocolTemplateById(SelectedNode.ProtocolTemplateId);
            ProtocolHelper.ResetParent(template.Protocol);

            if (!CheckIsFactory(template, "Unable to move factory protocol!")) return;
            if (!CheckIsEmergency(template,"Unable to move emergency protocol!")) return;

            if (SelectedNode.NodeType == ProtocolLayeredName.SCAN_NODE)
            {
                var scanModel = GetScanModel(template, SelectedNode.NodeId);
                //扫描节点上移，判断扫描节点是否第一个，如果是第一个则不执行上移动作

                //Todo:后期会调整(待展开节点)
                //var parentNode = NodeList.FindNode(scanModel.Parent.Parent.Parent.Descriptor.Id);
                //int i = parentNode.Children.IndexOf(SelectedNode);
                //if (i == 0) return;
                //parentNode.Children.Insert(i - 1, SelectedNode.Clone());
                //parentNode.Children.RemoveAt(i + 1);
                var templateId = template.Protocol.Descriptor.Id;
                var nodeClone = SelectedNode.Clone();
                //更新Template
                var scanParent = scanModel.Parent;
                var j = scanParent.Children.IndexOf(scanModel);
                if (j > 0)
                {
                    scanParent.Children.Insert(j-1,scanModel.Clone());
                    scanParent.Children.RemoveAt(j + 1);
                    //保存Template
                    TempPrepareExpandNode.ProtocolTemplateId = templateId;
                    TempPrepareExpandNode.SelectedNode = nodeClone;
                    _protocolApplicationService.Save(template);
                }
            }

            //
            if (SelectedNode.NodeType == ProtocolLayeredName.RECON_NODE)
            {
                var reconModel = GetReconModel(template, SelectedNode.NodeId);
                if (!CheckIsRTDForMove(reconModel)) return;
                //Todo:后期会调整(待展开节点)
                //var reconParentNode = NodeList.FindNode(reconModel.Parent.Descriptor.Id);
                //int i = reconParentNode.Children.IndexOf(SelectedNode);
                //if (i <= 1) return;
                //reconParentNode.Children.Insert(i - 1, SelectedNode.Clone());
                //reconParentNode.Children.RemoveAt(i + 1);
                var templateId = template.Protocol.Descriptor.Id;
                var nodeClone = SelectedNode.Clone();

                //更新Template
                var reconParent = reconModel.Parent;
                var j = reconParent.Children.IndexOf(reconModel);
                if (j > 1)
                {
                    reconParent.Children.Insert(j - 1, reconModel.Clone());
                    reconParent.Children.RemoveAt(j + 1);
                    //保存Template
                    TempPrepareExpandNode.ProtocolTemplateId = templateId;
                    TempPrepareExpandNode.SelectedNode = nodeClone;
                    _protocolApplicationService.Save(template);
                }
            }
        }
        private void MoveDownNowSelectedFromNodeType()
        {
            //如果是Protocol节点，则不能移动
            if (SelectedNode.NodeType == ProtocolLayeredName.PROTOCOL_NODE) return;
            //如果是末节点，则不能移动
            var template = GetProtocolTemplateById(SelectedNode.ProtocolTemplateId);
            ProtocolHelper.ResetParent(template.Protocol);

            if (!CheckIsFactory(template, "Unable to move factory protocol!")) return;
            if (!CheckIsEmergency(template, "Unable to move emergency protocol!")) return;

            if (SelectedNode.NodeType == ProtocolLayeredName.SCAN_NODE)
            {
                var scanModel = GetScanModel(template, SelectedNode.NodeId);
                //扫描节点下移，判断扫描节点是否最后一个，如果是则不执行下移动作
                //Todo:后期会调整(待展开节点)
                //var scanParentNode = NodeList.FindNode(scanModel.Parent.Parent.Parent.Descriptor.Id);
                //int i = scanParentNode.Children.IndexOf(SelectedNode);
                //if (i == scanParentNode.Children.Count - 1) return;
                //scanParentNode.Children.Insert(i + 2, SelectedNode.Clone());
                //scanParentNode.Children.RemoveAt(i);
                var templateId = template.Protocol.Descriptor.Id;
                var nodeClone = SelectedNode.Clone();
                //更新Template
                var scanParent = scanModel.Parent;
                var j = scanParent.Children.IndexOf(scanModel);
                if (j < scanParent.Children.Count - 1)
                {
                    scanParent.Children.Insert(j + 2, scanModel.Clone());
                    scanParent.Children.RemoveAt(j);
                    //保存Template
                    TempPrepareExpandNode.ProtocolTemplateId = templateId;
                    TempPrepareExpandNode.SelectedNode = nodeClone;
                    _protocolApplicationService.Save(template);
                }
            }

            if (SelectedNode.NodeType == ProtocolLayeredName.RECON_NODE)
            {
                var reconModel = GetReconModel(template, SelectedNode.NodeId);
                if (!CheckIsRTDForMove(reconModel)) return;
                //Todo:后期会调整(待展开节点)
                //var reconParentNode = NodeList.FindNode(reconModel.Parent.Descriptor.Id);
                //int i = reconParentNode.Children.IndexOf(SelectedNode);
                //if (i == reconParentNode.Children.Count - 1) return;
                //reconParentNode.Children.Insert(i + 2, SelectedNode.Clone());
                //reconParentNode.Children.RemoveAt(i);
                var templateId = template.Protocol.Descriptor.Id;
                var nodeClone = SelectedNode.Clone();
                //更新Template
                var reconParent = reconModel.Parent;
                var j = reconParent.Children.IndexOf(reconModel);
                if (j > 0 && j < reconParent.Children.Count - 1)
                {
                    reconParent.Children.Insert(j + 2, reconModel.Clone());
                    reconParent.Children.RemoveAt(j);
                    //保存Template
                    TempPrepareExpandNode.ProtocolTemplateId = templateId;
                    TempPrepareExpandNode.SelectedNode = nodeClone;
                    _protocolApplicationService.Save(template);
                }

            }
        }

        private void CheckNodeName(Node node)
        {
            ProtocolNameEditWindow protocolNameEditWindow = new ProtocolNameEditWindow();
            protocolNameEditWindow.Topmost = true;
            var winHwnd = new WindowInteropHelper(protocolNameEditWindow);
            winHwnd.Owner = ConsoleSystemHelper.WindowHwnd;
            var vm = protocolNameEditWindow.DataContext as ProtocolNameEditViewModel;
            vm.LoadData(node);
            protocolNameEditWindow.ShowDialog();
        }

        private void RepeatNowSelectedFromNodeType()
        {
            var template = _protocolApplicationService.GetProtocolTemplate(Global.Instance.SelectTemplateID);
            if (!CheckIsEmergencyForRepeat(template)) return;

            CheckNodeName(SelectedNode);
            if (IsConfirmed)
            {
                RepeatNode(NewNodeName);
                IsConfirmed = false;
                NewNodeName = string.Empty;
            }
        }

        /// <summary>
        /// Delete node based on node type.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private bool DeleteNowSelectedFromNodeType()
        {
            switch(_nowSelectedNodeType )
            {
                case ProtocolLayeredName.PROTOCOL_NODE: 
                    DeleteProtocolTask();
                    break;
                case ProtocolLayeredName.MEASUREMENT_NODE:
                    DeleteMeasurementTask();
                    break;
                case ProtocolLayeredName.SCAN_NODE:
                    DeleteScanTask();
                    break;
                case ProtocolLayeredName.RECON_NODE:
                    DeleteReconTask();
                    break;
                //_ => throw new ArgumentOutOfRangeException(nameof(_nowSelectedNodeType),
                //    $"Not expected direction value: {_nowSelectedNodeType}"),
            };
            return true;
        }

        [UIRoute]
        public bool RepeatProtocolTask()
        {
            var template = _protocolApplicationService.GetProtocolTemplate(Global.Instance.SelectTemplateID);
            if (!CheckIsEmergencyForRepeat(template)) return false;

            //ProtocolNameEditWindow protocolNameEditWindow = new ProtocolNameEditWindow();
            //protocolNameEditWindow.Topmost = true;
            //var winHwnd = new WindowInteropHelper(protocolNameEditWindow);
            //winHwnd.Owner = ConsoleSystemHelper.WindowHwnd;
            //var vm = protocolNameEditWindow.DataContext as ProtocolNameEditViewModel;
            //vm.LoadData(SelectedNode);
            //protocolNameEditWindow.ShowDialog();
            CheckNodeName(SelectedNode);

            var protocolName = string.Empty;

            //IDialogParameters parameters = new DialogParameters();
            //parameters.Add(DialogParameter.Tag, DialogParameter.ProtocolName);
            //parameters.Add(DialogParameter.Title, ProtocolOperation.RepeatProtocol);
            //_dialogService.ShowDialog(DialogNameManager.InputDialog, parameters, callback =>
            //{
            //    if (callback.Result == ViewModel.Common.DialogEnum.ButtonResult.OK)
            //    {
            //        callback.Parameters.TryGetValue(DialogParameter.ProtocolName, out protocolName);
            //    }
            //});

            //var protocolTemplateID = _protocolApplicationService.Repeat(_nowSelectedProtocolTemplateId, protocolName);
            //var protocolTemplate = _protocolApplicationService.GetProtocolTemplate(protocolTemplateID);
            ////复制新的协议节点
            //ProtocolToUINode(protocolTemplate);
            //RefreshProtocolTree();
            return true;
        }

        private ProtocolTemplateModel GetNowSeleteProtocolTemplate()
        {
            return _protocolApplicationService.GetProtocolTemplate(_nowSelectedProtocolTemplateId);
        }

        private bool RepeatMeasurementTask()
        {
            var isComplateRepeat = false;
            var measurementModel = _protocolApplicationService.RepeatMeasurementTask(_nowSelectedProtocolTemplateId, _nowSelectedNodeId);
            var measurementID = measurementModel.Descriptor.Id;
            ProtocolTemplateModel protocolTemplate = GetNowSeleteProtocolTemplate();
            ProtocolHelper.ResetParent(protocolTemplate.Protocol);
            Node measurementNode = new(NodeType.MeasurementNode);
            var measurement = (from f in protocolTemplate.Protocol.Children
                               from m in f.Children
                               where m.Descriptor.Id == measurementID
                               select m).FirstOrDefault();
            Node? node = measurement.ModelToNode(protocolTemplate.Descriptor.Id);
            if (node is not null)
                measurementNode = node;
            NodeList.AddNode(measurementNode);
            isComplateRepeat = true;//TODO
            return isComplateRepeat;
        }

        [UIRoute]
        private bool DeleteProtocolTask()
        {
            var result = false;
            if (SelectedNode is not null)
            {
                var template = _protocolApplicationService.GetProtocolTemplate(Global.Instance.SelectTemplateID);
                if (!CheckIsFactoryForDelete(template)) return false;
                if (!CheckIsEmergencyForDelete(template)) return false;

                _dialogService.ShowDialog(true, MessageLeveles.Info, "Confirm", LanguageResource.Message_Confirm_Delete, arg =>
                {
                    if (arg.Result != ButtonResult.OK) return;
                    _protocolApplicationService.Delete(_nowSelectedProtocolTemplateId);
                    DeleteProtocolNode(_nowSelectedProtocolTemplateId);

                    result = true;
                }, ConsoleSystemHelper.WindowHwnd);
            }
            return result;
        }

        private bool DeleteMeasurementTask()
        {
            bool result = false;
            _protocolApplicationService.DeleteMeasurementTask(_nowSelectedProtocolTemplateId, _nowSelectedNodeId);
            if (SelectedNode is not null)
            {
                NodeList.RemoveNode(SelectedNode.NodeId);
                result = true;
            }
            return result;
        }

        private bool AutoScanNowSelectedScanTask()
        {
            if (SelectedNode is null) return false;
            if (SelectedNode.NodeType != ProtocolLayeredName.SCAN_NODE) return false;
            
            bool result = false;
            var template = _protocolApplicationService.GetProtocolTemplate(Global.Instance.SelectTemplateID);
            var scanModel = GetScanModel(template, SelectedNode.NodeId);
            if (scanModel.AutoScan) return false;
            MeasurementModel measurementModel = null;
            foreach (var f in template.Protocol.Children)
            {
                foreach (var m in f.Children)
                {
                    if (m.Children.Any(s => s == scanModel))
                    {
                        measurementModel = m;
                        break;
                    }
                }
            }
            var currentScanIndex = measurementModel.Children.IndexOf(scanModel);
            if (currentScanIndex > 0)
            {
                //检查在这个ScanTask前有没有AutoScan的任务
                var lastAutoScanIndex = measurementModel.Children.FindLastIndex(t => t.AutoScan);
                if (lastAutoScanIndex >= 0)
                {
                    for (int i = lastAutoScanIndex + 1; i < currentScanIndex + 1; i++)
                    {
                        measurementModel.Children[i].Parameters
                            .Find(p => p.Name == ProtocolParameterNames.SCAN_AUTO_SCAN).Value = true.ToString();
                    }
                }
                else
                {
                    measurementModel.Children[currentScanIndex].Parameters
                        .Find(p => p.Name == ProtocolParameterNames.SCAN_AUTO_SCAN).Value = true.ToString();
                }
            }
            else
            {
                measurementModel.Children[currentScanIndex].Parameters
                    .Find(p => p.Name == ProtocolParameterNames.SCAN_AUTO_SCAN).Value = true.ToString();
            }
            string message = string.Empty;
            try
            {
                _protocolApplicationService.Save(template);
                message = "Save Successful";
            }
            catch (Exception e)
            {
                message = $"Save failed: {e.Message}";
            }
            _dialogService.ShowDialog(false, MessageLeveles.Info, "Tip", message, a => {}, ConsoleSystemHelper.WindowHwnd);
            return result;
        }

        [UIRoute]
        private bool DeleteScanTask()
        {
            var result = false;
            if (SelectedNode is not null)
            {
                var template = _protocolApplicationService.GetProtocolTemplate(Global.Instance.SelectTemplateID);
                if (!CheckIsFactoryForDelete(template)) return false;
                if (!CheckIsEmergencyForDelete(template)) return false;

                _dialogService.ShowDialog(true, MessageLeveles.Info, LanguageResource.Message_Info_CloseConfirmTitle, LanguageResource.Message_Confirm_Delete, arg =>
                {
                    if (arg.Result != ButtonResult.OK) return;
                    Node parentNode = null;
                    NodeList.ForEach(p =>
                    {
                        if (p.Children.Any(s => s.NodeId == SelectedNode.NodeId))
                        {
                            parentNode = p;
                        }
                    });
                    //parentNode = NodeList.FindNode(SelectedNode.Parent.ParentID);
                    if (null != parentNode)
                    {
                        if (!RemoveScanNode(_nowSelectedProtocolTemplateId, _nowSelectedNodeId)) return;

                        var k = parentNode.Children.IndexOf(SelectedNode);
                        //从TreeView中删除Scan节点
                        NodeList.RemoveNode(SelectedNode.NodeId);
                        SelectedNode = k > 0 ? parentNode.Children[k - 1] : parentNode;
                        _nowSelectedNodeId = SelectedNode.NodeId;
                        result = true;
                    }
                }, ConsoleSystemHelper.WindowHwnd);
            }
            return result;
        }

        [UIRoute]
        private bool DeleteReconTask()
        {
            //Todo: 第一个Recon默认为RTD,不能删除
            var result = false;
            if (SelectedNode is not null)
            {
                var template = _protocolApplicationService.GetProtocolTemplate(Global.Instance.SelectTemplateID);
                var reconModel = GetReconModel(template,SelectedNode.NodeId);
                
                if (!CheckIsFactoryForDelete(template)) return false;
                if (!CheckIsEmergencyForDelete(template)) return false;
                if (!CheckIsRTDForDelete(reconModel)) return false;

                _dialogService.ShowDialog(true, MessageLeveles.Info, "Confirm", LanguageResource.Message_Confirm_Delete, arg =>
                {
                    if (arg.Result != ButtonResult.OK) return;
                    Node parentNode = NodeList.FindNode(SelectedNode.ParentID);
                    var k = parentNode.Children.IndexOf(SelectedNode);
                    
                    _protocolApplicationService.DeleteReconTask(_nowSelectedProtocolTemplateId, _nowSelectedNodeId);
                    //从TreeView中删除Recon节点
                    NodeList.RemoveNode(SelectedNode.NodeId);
                    
                    SelectedNode = k > 0 ? parentNode.Children[k - 1] : parentNode;
                    _nowSelectedNodeId = SelectedNode.NodeId;
                    
                    result = true;
                }, ConsoleSystemHelper.WindowHwnd);
            }
            return result;
        }
        private bool CheckIsFactoryForDelete(ProtocolTemplateModel template)
        {
            if (template.Protocol.IsFactory)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Warning,
                    LanguageResource.Message_Info_CloseWarningTitle, "Unable to delete factory protocol!", null, ConsoleSystemHelper.WindowHwnd);
                return false;
            }
            return true;
        }

        private bool CheckIsFactory(ProtocolTemplateModel template, string message)
        {
            if (template.Protocol.IsFactory)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Warning,
                    LanguageResource.Message_Info_CloseWarningTitle, message, null, ConsoleSystemHelper.WindowHwnd);
                return false;
            }
            return true;
        }

        private bool CheckIsEmergencyForDelete(ProtocolTemplateModel template)
        {
            if (template.Protocol.IsEmergency)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Warning,
                    LanguageResource.Message_Info_CloseWarningTitle, "Unable to delete emergency protocol!", null, ConsoleSystemHelper.WindowHwnd);
                return false;
            }
            return true;
        }
        private bool CheckIsRTDForDelete(ReconModel reconModel)
        {
            if (reconModel.IsRTD)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Warning,
                    LanguageResource.Message_Info_CloseWarningTitle, "Recon parameters belonging to RTD cannot be deleted!", null, ConsoleSystemHelper.WindowHwnd);
                return false;
            }
            return true;
        }
        private bool CheckIsRTDForMove(ReconModel reconModel)
        {
            if (reconModel.IsRTD)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Warning,
                    LanguageResource.Message_Info_CloseWarningTitle, "Recon node belonging to RTD cannot be moved!", null, ConsoleSystemHelper.WindowHwnd);
                return false;
            }
            return true;
        }

        private bool CheckIsEmergencyForRepeat(ProtocolTemplateModel template)
        {
            if (template.Protocol.IsEmergency)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Info,
                    LanguageResource.Message_Info_CloseInformationTitle, "Unable to repeat emergency protocol!", null, ConsoleSystemHelper.WindowHwnd);
                return false;
            }
            return true;
        }

        private bool CheckIsEmergency(ProtocolTemplateModel template, string message)
        {
            if (template.Protocol.IsEmergency)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Info,
                    LanguageResource.Message_Info_CloseInformationTitle, message, null, ConsoleSystemHelper.WindowHwnd);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocolTemplate"></param>
        /// <returns></returns>
        private static ProtocolTemplateModel ModifityProtocolID(ProtocolTemplateModel protocolTemplate)
        {
            //TODO
            protocolTemplate.Descriptor.Id = IdGenerator.Next(0);
            protocolTemplate.Protocol.Descriptor.Id = IdGenerator.Next(1);
            protocolTemplate.Protocol.Children.ForEach(forItem =>
            {
                forItem.Descriptor.Id = IdGenerator.Next(2);
                forItem.Children.ForEach(measurementItem =>
                {
                    measurementItem.Descriptor.Id = IdGenerator.Next(3);
                    measurementItem.Children.ForEach(scanItem =>
                    {
                        scanItem.Descriptor.Id = IdGenerator.Next(4);
                        scanItem.Children.ForEach(reconItem => reconItem.Descriptor.Id = IdGenerator.Next(5));
                    });
                });
            });
            return protocolTemplate;
        }
        [UIRoute]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeTypeAndId"></param>
        private void ProtocolTreeSelecteNodeChanged(object[] nodeTypeAndId)
        {
            _nowSelectedNodeType = nodeTypeAndId[0].ToString() ?? string.Empty;
            _nowSelectedProtocolTemplateId = nodeTypeAndId[1].ToString() ?? string.Empty;
            _nowSelectedNodeId = nodeTypeAndId[2].ToString() ?? string.Empty;
            //如果选择的是扫描或重建，是否也能找到相应节点？待调试
            SelectedNode = NodeList.FindNode(_nowSelectedNodeId);

            Global.Instance.SelectNodeID = _nowSelectedNodeId;
            Global.Instance.SelectTemplateID = _nowSelectedProtocolTemplateId;
            //Tuple<string, string, string> tuple = new(_nowSelectedNodeType,_nowSelectedNodeId,_nowSelectedProtocolTemplateId);
            //_ = ("NodeType", "NodeId", "TemplateId");
            (string NodeType, string NodeId, string TemplateId) nodeTypeAndID = new(_nowSelectedNodeType, _nowSelectedNodeId, _nowSelectedProtocolTemplateId);
            _protocolApplicationService.ChangeTreeNodeSelect(nodeTypeAndID);//TODO: para class
        }
        [UIRoute]
        /// <summary>
        /// When switching body part selection, by default, retrieve all protocols for the currently selected body part and display these protocols on the protocol tree.
        /// </summary>
        private void LoadAllProtocolToNodeList(List<ProtocolTemplateModel> protocolTemplates)
        {
            NodeList = new();
            _protocols.Clear();
            protocolTemplates.ForEach(protocol => ProtocolToUINode(protocol));
            RefreshProtocolTree();
            //NodeList.Sort();
        }
        [UIRoute]
        /// <summary>
        /// Delete protocolNode
        /// </summary>
        /// <param name="protocolTemplateID"></param>
        /// <returns></returns>
        private bool DeleteProtocolNode(string protocolTemplateID)
        {
            bool result = true;
            try
            {
                var protocolToRemove = _protocols.FirstOrDefault(p => p.ParentID == protocolTemplateID);
                if (protocolToRemove is not null)
                {
                    _protocols.Remove(protocolToRemove);
                    _protocols = new PeremptoryObservableCollection<Node>(_protocols.OrderBy(p => p.NodeName));
                    NodeList = _protocols;
                }
            }
            catch (Exception)
            {
                //TODO
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Remove the corresponding Scan Node from the protocol tree based on the currently selected template ID and Node ID.
        /// </summary>
        /// <param name="templateFileModel"></param>
        private bool RemoveScanNode(string nowSelectTemplateID, string nowSelectNodeID)
        {
            return _protocolApplicationService.DeleteScanTask(nowSelectTemplateID, nowSelectNodeID);
        }

        [UIRoute]
        /// <summary>
        /// Convert the protocol model in the Data layer to the view protocol model.
        /// </summary>
        /// <param name="protocol"></param>
        private void ProtocolToUINode(ProtocolTemplateModel protocolTemplate)
        {
            var protocol = protocolTemplate.Protocol;
            if (protocol is not null)
            {
                ProtocolHelper.ResetParent(protocol);
                var node = protocol.ModelToNode(protocolTemplate.Descriptor.Id);
                if (node is not null)
                    _protocols.Add(node);
            }
            //RefreshProtocolTree();
        }
        [UIRoute]
        /// <summary>
        /// Refresh tree
        /// </summary>
        private void RefreshProtocolTree()
        {
            //NodeList.Clear();
            PeremptoryObservableCollection<Node> nodes = _protocols.Clone();
            PeremptoryObservableCollection<Node> tempNodes = new PeremptoryObservableCollection<Node>();
            int last = nodes.Count - 1;
            int i = 0;
            foreach (var node in nodes)
            {
                tempNodes.Clear();
                TraverseTreeViewItems(node.Children, ref tempNodes);
                nodes[i].Children.Clear();
                foreach (Node item in tempNodes)
                {
                    nodes[i].Children.Add(item);
                }
                i++;
            }

            NodeList = new PeremptoryObservableCollection<Node>(nodes.OrderBy(x => x.NodeName).ToList());
        }


        [UIRoute]
        private void TraverseTreeViewItems(PeremptoryObservableCollection<Node> itemsControl, ref PeremptoryObservableCollection<Node> tempNodes)
        {
            foreach (Node item in itemsControl)
            {
                if (item.NodeType == ProtocolLayeredName.MEASUREMENT_NODE)
                {
                    TraverseTreeViewItems(item.Children, ref tempNodes);
                }
                if (item.NodeType == ProtocolLayeredName.SCAN_NODE)
                {
                    tempNodes.Add(item);
                }
            }
        }

        private ProtocolTemplateModel GetProtocolTemplateById(string templateId)
        {
            return _protocolApplicationService.GetProtocolTemplate(templateId);
        }
    }
}