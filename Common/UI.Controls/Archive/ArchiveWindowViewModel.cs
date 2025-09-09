//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/5 11:01:27      V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.DicomUtility.Transfer;
using NV.CT.Language;
using NV.CT.UI.ViewModel;
using NV.CT.WorkflowService.Contract;
using NV.MPS.Configuration;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;

namespace NV.CT.UI.Controls.Archive
{
    public class ArchiveWindowViewModel : BaseViewModel
    {
        #region Members
        private readonly IMapper _mapper;
        private readonly ILogger<ArchiveWindowViewModel> _logger;
        private Action<ArchiveModel,bool,bool>? _callback;

        private const string COMMAND_OK = "OKCommand";
        private const string COMMAND_CLOSE = "CloseCommand";

        #endregion

        #region Properties

        private ObservableCollection<ArchiveModel>? _archiveModels;
        public ObservableCollection<ArchiveModel>? ArchiveModels
        {
            get => _archiveModels;
            set
            {
                SetProperty(ref _archiveModels, value);
            }
        }

        private ArchiveModel? _selectedArchiveModel;
        public ArchiveModel? SelectedArchiveModel
        {
            get => _selectedArchiveModel;
            set
            {
                SetProperty(ref _selectedArchiveModel, value);
            }
        }

        private SupportedTransferSyntax[] _dicomTransferSyntaxTypes;
        public SupportedTransferSyntax[] DicomTransferSyntaxTypes
        {
            get => _dicomTransferSyntaxTypes;
            set
            {
                SetProperty(ref _dicomTransferSyntaxTypes, value);
            }
        }

        private SupportedTransferSyntax _selectedDicomTransferSyntax;
        public SupportedTransferSyntax SelectedDicomTransferSyntax
        {
            get => _selectedDicomTransferSyntax;
            set
            {
                SetProperty(ref _selectedDicomTransferSyntax, value);
            }
        }

        private string _message = string.Empty;
        public string Message
        {
            get => this._message;
            set => this.SetProperty(ref this._message, value);
        }


        private bool _useTls = false;
        public bool UseTls
        {
            get
            {
                return _useTls;
            }
            set
            {
                SetProperty(ref _useTls, value);
            }
        }

        private bool _anonymouse = false;
        public bool Anonymous
        {
            get
            {
                return _anonymouse;
            }
            set
            {
                SetProperty(ref _anonymouse, value);
            }
        }

        #endregion

        #region Constructor

        public ArchiveWindowViewModel(IDialogService dialogService,
                                      ILogger<ArchiveWindowViewModel> logger,
                                      IAuthorization authorizationService,
                                      IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;

            this._dicomTransferSyntaxTypes = EnumHelper.GetAllItems<SupportedTransferSyntax>().ToArray();
            this._selectedDicomTransferSyntax = SupportedTransferSyntax.JPEGLSLossless;

            Commands.Add(COMMAND_OK, new DelegateCommand<object>(OnOK));
            Commands.Add(COMMAND_CLOSE, new DelegateCommand<object>(OnClose, _ => true));
        }

        #endregion

        #region Public methods
        public void Show(Action<ArchiveModel,bool,bool>? callback)
        {
            this.Message = string.Empty;

            this._callback = callback;
            this.InitArchiveList();
        }

        #endregion

        #region Private Methods

        private void OnOK(object paramWindow)
        {
            if (this._callback is null)
            {
                this._logger.LogWarning("callback is null in OnSelectArchiveConfig.");
                return;
            }

            if (this.SelectedArchiveModel is null)
            {
                this._logger.LogWarning("No SelectedArchiveModel in OnSelectArchiveConfig.");
                return;
            }

            this.Message = string.Empty;

            this.SelectedArchiveModel.TransferSyntax = this.SelectedDicomTransferSyntax.ToString();
            HideWindow(paramWindow as Window);
            this._callback.Invoke(SelectedArchiveModel,UseTls,Anonymous);
        }

        private void InitArchiveList()
        {
            ArchiveModels = GetAllArchives().ToObservableCollection();
            SelectedArchiveModel = ArchiveModels.FirstOrDefault();
        }

        private List<ArchiveModel> GetAllArchives()
        {
            var archiveConfigs = UserConfig.ArchiveConfig.Archives;
            return _mapper.Map<List<ArchiveInfo>, List<ArchiveModel>>(archiveConfigs);
        }

        private void OnClose(object paramWindow)
        {
            if (paramWindow is Window window)
            {
                window.Hide();
            }
        }

        /// <summary>
        /// 测试网络连通性
        /// TODO: 将会用Echo服务替代
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private bool CheckConnectivityOfServer(string ip)
        {
            var ping = new Ping();
            try
            {
                PingReply pr = ping.Send(ip, 500);
                if (pr.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect Server IP {ip} with Status Error,{ex.Message},{ex.StackTrace}");
                return false;
            }
        }

        private void HideWindow(Window window)
        {
            if (window is not null)
            {
                window.Hide();
            }

        }

        #endregion
    }
}
