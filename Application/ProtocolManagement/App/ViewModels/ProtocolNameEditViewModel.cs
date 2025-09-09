using Microsoft.Extensions.Logging;
using NV.CT.CTS.Helpers;
using NV.CT.Language;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;
using NV.CT.ProtocolManagement.ViewModels.Enums;
using NV.CT.ProtocolManagement.ViewModels.Models;
using NV.CT.UI.ViewModel;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using System;
using System.Linq;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class ProtocolNameEditViewModel : BaseViewModel
    {
        private readonly ILogger<ProtocolNameEditViewModel>? _logger;
        private readonly IProtocolApplicationService _protocolApplicationService;
        private readonly IDialogService _dialogService;
        public ProtocolTreeControlViewModel _protocolTreeControlViewModel;
        public event EventHandler<bool> NameFocusSetting;
        public event EventHandler<bool> WindowClosing;

        private string _nodeTypeDescription = LanguageResource.Text_ProtocolName;
        public string NodeTypeDescription
        {
            get
            {
                return this._nodeTypeDescription;
            }
            set 
            {
                this.SetProperty(ref this._nodeTypeDescription, value);
            }
        }

        public ProtocolNameEditViewModel(ILogger<ProtocolNameEditViewModel> logger, IProtocolApplicationService protocolApplicationService, IDialogService dialogService, ProtocolTreeControlViewModel protocolTreeControlViewModel)
        {
            _logger= logger;
            _dialogService= dialogService;
            _protocolApplicationService = protocolApplicationService;
            _protocolTreeControlViewModel = protocolTreeControlViewModel;
            //Commands.Add("LostFocusCommand", new DelegateCommand(LostFocusCommand));
            Commands.Add("CloseCommand", new DelegateCommand(CloseCommand));
            Commands.Add("ConfirmCommand", new DelegateCommand(ConfirmCommand));
        }

        private void CloseCommand()
        {
            WindowClosing.Invoke(this, true);
        }

        /// <summary>
        /// 初始化加载数据
        /// </summary>
        /// <param name="node"></param>
        public void LoadData(Node node)
        {
            CurrentNode = node;
            NewNodeName = node.NodeName + " Copy";
            this.NodeTypeDescription = this.GetNodeTypeDescription(node);
        }

        private string GetNodeTypeDescription(Node node)
        {
            string nodeTypeDescription = string.Empty;
            switch (node.NodeType)
            {
                case ProtocolLayeredName.PROTOCOL_NODE:
                    nodeTypeDescription = LanguageResource.Content_ProtocolName;
                    break;
                case ProtocolLayeredName.SCAN_NODE:
                    nodeTypeDescription = LanguageResource.Content_ScanName;
                    break;
                case ProtocolLayeredName.RECON_NODE:
                    nodeTypeDescription = LanguageResource.Content_ReconName;
                    break;
                default:
                    nodeTypeDescription = LanguageResource.Content_ProtocolName;
                    break;
            }

            return nodeTypeDescription;

        }


        private void LostFocusCommand()
        {
            //失去焦点时，检查节点名称和Id是否重复
            if (CheckNameIsDuplicate())
            {
                //如果重复，则弹窗提示重复，焦点转移到当前文本框
                _dialogService.ShowDialog(true, MessageLeveles.Warning, "Confirm", "The name is duplicated!", arg =>
                {
                    if (arg.Result != ButtonResult.OK) return;
                    NameFocusSetting.Invoke(this,true);
                }, ConsoleSystemHelper.WindowHwnd);
            }
        }
        
        private void ConfirmCommand()
        {
            //通过在ProtocolTreeViewModel中定义的事件 传递新的名称，回到主窗口ViewModel执行Repeat操作
            if (!CheckNameIsDuplicate())
            {
                _protocolTreeControlViewModel.IsConfirmed = true;
                _protocolTreeControlViewModel.NewNodeName = NewNodeName;

                //_protocolApplicationService.SendNewNodeName(NewNodeName);
                WindowClosing.Invoke(this, true);
            }
            else
            {
                _dialogService.ShowDialog(true, MessageLeveles.Warning, "Confirm", "The name is duplicated!", arg =>
                {
                    if (arg.Result != ButtonResult.OK) return;
                    NameFocusSetting.Invoke(this, true);
                }, ConsoleSystemHelper.WindowHwnd);
            }
        }

        /// <summary>
        /// 检查名称是否重复
        /// </summary>
        /// <returns></returns>
        private bool CheckNameIsDuplicate()
        {
            bool flag = false;
            //查询当前部位下的所有协议，
            var allProtocols = _protocolApplicationService.GetAllProtocolTemplates();
            var currentBodyProtocols = allProtocols.FindAll(protocolTemplate =>
                    protocolTemplate.Protocol.BodyPart.ToString().ToUpper() == allProtocols.FirstOrDefault(t=>t.Descriptor.Id == CurrentNode.ProtocolTemplateId).BodyPart.ToString().ToUpper());
            var protocolTemplateModel = _protocolApplicationService.GetProtocolTemplate(CurrentNode.ProtocolTemplateId);
            switch (CurrentNode.NodeType)
            {
                case ProtocolLayeredName.PROTOCOL_NODE://比较协议名称
                    flag = currentBodyProtocols.Any(p => p.Descriptor.Name == NewNodeName);
                    break;
                case ProtocolLayeredName.SCAN_NODE:
                    int scanNum = 0;
                    currentBodyProtocols.FirstOrDefault(p => p.Descriptor.Id == CurrentNode.ProtocolTemplateId).Protocol
                        .Children[0].Children.ForEach(measurement =>
                        {
                            measurement.Children.ForEach(scanItem =>
                            {
                                if (scanItem.Descriptor.Name == NewNodeName)
                                {
                                    scanNum++;
                                }
                            });
                        });
                    flag = (scanNum > 0);
                    break;
                case ProtocolLayeredName.RECON_NODE:
                    int reconNum = 0;
                    currentBodyProtocols.FirstOrDefault(p => p.Descriptor.Id == CurrentNode.ProtocolTemplateId).Protocol
                        .Children[0].Children.ForEach(measurement =>
                        {
                            var scanModel = measurement.Children.Find(r => r.Descriptor.Id == CurrentNode.ParentID);
                            scanModel?.Children.ForEach(reconItem =>
                            {
                                if (reconItem.Descriptor.Name == NewNodeName)
                                {
                                    reconNum++;
                                }
                            });
                        });
                    flag = (reconNum > 0);
                    break;
            }
            return flag;
        }



        private string protocolName = string.Empty;
        public string ProtocolName
        {
            get => protocolName;
            set => SetProperty(ref protocolName, value);
        }

        private Node currentNode = new Node();

        public Node CurrentNode
        {
            get => currentNode;
            set => SetProperty(ref currentNode, value);
        }

        private string nodeName = string.Empty;

        public string NewNodeName
        {
            get => nodeName;
            set => SetProperty(ref nodeName, value);
        }

    }
}
