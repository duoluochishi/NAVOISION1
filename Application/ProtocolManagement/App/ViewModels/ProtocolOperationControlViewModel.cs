using Microsoft.Extensions.Logging;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS.Helpers;
using NV.CT.Protocol;
using NV.CT.Protocol.Models;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;
using NV.CT.UI.ViewModel;
using NV.MPS.Exception;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DialogResult = System.Windows.Forms.DialogResult;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class ProtocolOperationControlViewModel : BaseViewModel
    {
        private readonly IProtocolApplicationService _protocolApplicationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ProtocolOperationControlViewModel> _logger;

        private string _nowSelectedBodyPart;
        public string NowSelectedBodyPart
        {
            get => _nowSelectedBodyPart;
            set => SetProperty(ref _nowSelectedBodyPart, value);
        }

        public ProtocolOperationControlViewModel(ILogger<ProtocolOperationControlViewModel> logger, IProtocolApplicationService protocolApplicationService, IDialogService dialogService)
        {
            _logger = logger;
            _protocolApplicationService = protocolApplicationService;
            _dialogService = dialogService;
            Commands.Add("OperationCommand", new DelegateCommand<string>(Operation));
            _protocolApplicationService.SelectBodyPartForProtocolChanged += SelecctBodyPartForProtocolChanged;
            _protocolApplicationService.ProtocolChanged += ProtocolApplicationServiceOnProtocolChanged;
        }
        [UIRoute]
        private void ProtocolApplicationServiceOnProtocolChanged(object? sender, string e)
        {
            //string protocolTemplateId = e;
            ////刷新
            ////将协议加入到当前Tree
            ////根据体位查询所有协议
            //var allProtocols = _protocolApplicationService.GetAllProtocolTemplates();

            //if (allProtocols.Any(t => t.Descriptor.Id == protocolTemplateId))
            //{
            //    BodyPart bodyPart = allProtocols.FirstOrDefault(t => t.Descriptor.Id == protocolTemplateId).BodyPart;
            //    //刷新Tree
            //    _protocolApplicationService.ChangeBodyPartForProtocol(bodyPart.ToString());
            //}
        }

        private void Operation(string operationName)
        {
            switch (operationName)
            {
                case ProtocolOperation.BTN_EXPORT:
                    Export();
                    break;
                case ProtocolOperation.BTN_IMPORT:
                    Import();
                    break;
                case ProtocolOperation.BTN_REPEAT:
                    Repeat();
                    break;
                case ProtocolOperation.BTN_DELETE:
                    Delete();
                    break;
                case ProtocolOperation.BTN_EXPAND:
                    Expand();
                    break;
                case ProtocolOperation.BTN_COLLAPSED:
                    Collapsed();
                    break;
                case ProtocolOperation.BTN_UP:
                    MoveUp();
                    break;
                case ProtocolOperation.BTN_DOWN:
                    MoveDown();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Selection of the body part has undergone modification.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="bodyPartName"></param>
        private void SelecctBodyPartForProtocolChanged(object? sender, CTS.EventArgs<string> bodyPartName)
        {
            NowSelectedBodyPart = !string.IsNullOrEmpty(bodyPartName.Data) ? bodyPartName.Data : string.Empty;
        }

        /// <summary>
        /// Export Protocol
        /// </summary>
        /// <param name="operateName"></param>
        private void Export()
        {
            string message = string.Empty;
            try
            {
                _protocolApplicationService.ExcuteProtocolOperation(ProtocolOperation.BTN_EXPORT);
                message = "Export protocol successful!";
            }
            catch (NanoException ex)
            {
                //TODO
                message = "Export protocol failed";
                _logger.LogError(ex, $"Export protocol failed");
            }
            
        }

        private bool CheckProtocolDuplicate(string[] filePaths)
        {
            var protocolModels = ProtocolHelper.GetProtocolsByFiles(filePaths);
            List<ProtocolTemplateModel> protocolTemplate = _protocolApplicationService.GetAllProtocolTemplates()
                .FindAll(protocolTemplate => !protocolTemplate.Protocol.IsEmergency);
            bool flag = false;

            protocolModels.ForEach(p =>
            {
                if (protocolTemplate.Where(protocol => protocol.Protocol.BodyPart == p.Protocol.BodyPart).Any(t =>
                        t.Protocol.Descriptor.Name.ToUpper() == p.Protocol.Descriptor.Name.ToUpper()))
                {
                    flag = true;
                }
            });
            return flag;
        }

        /// <summary>
        /// 导入协议文件，Import Protocol(xml file)
        /// </summary>
        private void Import()
        {
            //TODO
            var message = string.Empty;
            try
            {
                OpenFileDialog dialog = new()
                {
                    Multiselect = true,
                    DefaultExt = ".xml",
                    Filter = "xml file|*.xml"
                };
                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.Cancel)
                {
                    //Todo:协议名称重复判断
                    //根据部位查询当前部位的所有协议，再判断协议名称与导入协议文件是否重名
                    string[] filePaths = dialog.FileNames;
                    if (CheckProtocolDuplicate(filePaths))
                    {
                        message = "Cannot be imported because duplicate protocol names exist!";
                        _dialogService.ShowDialog(false, MessageLeveles.Info, "Info", message, callback => { }, ConsoleSystemHelper.WindowHwnd);
                        return;
                    }

                    //执行导入协议文件
                    var importResult = _protocolApplicationService.Import(new List<string>(dialog.FileNames));
                    if (importResult.Item1)
                    {
                        message = "Import protocol successful!";
                    }
                    else
                    {
                        message = "Import protocol failed!";
                        _logger.LogWarning(importResult.Item2);
                    }
                }
            }
            catch (System.Exception ex)
            {
                message = "Import protocol failed!";
                _logger.LogError(ex, $"Export protocol failed");
            }
            if (!string.IsNullOrEmpty(message))
            {
                _dialogService.ShowDialog(false, MessageLeveles.Info, "Tip", message, callback =>{}, ConsoleSystemHelper.WindowHwnd);
            }
        }

        private void Repeat()
        {
            _protocolApplicationService.SetRepeatNode();
        }

        private void Delete()
        {
            _protocolApplicationService.SetDeleteNode();
        }

        private void Collapsed()
        {
            _protocolApplicationService.SetCollapseNode();
        }

        private void Expand()
        {
            _protocolApplicationService.SetExpandNode();
        }

        private void MoveUp()
        {
            _protocolApplicationService.SetMoveNode(ProtocolOperation.BTN_UP);
        }

        private void MoveDown()
        {
            _protocolApplicationService.SetMoveNode(ProtocolOperation.BTN_DOWN);
        }
    }
}

