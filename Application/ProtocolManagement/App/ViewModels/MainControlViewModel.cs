using NV.CT.CTS;
using NV.CT.CTS.Helpers;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;
using NV.CT.UI.ViewModel;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Diagnostics;
using System.Windows;

namespace NV.CT.ProtocolManagement.ViewModels
{
	public class MainControlViewModel : BaseViewModel
	{
		private bool _reconControlIsEnable = false;
		private readonly IDialogService _dialogService;
		public bool ReconControlIsEnable
		{
			get => _reconControlIsEnable;
			set => SetProperty(ref _reconControlIsEnable, value);
		}

		private bool _scanControlIsEnable = false;
		public bool ScanControlIsEnable
		{
			get => _scanControlIsEnable;
			set => SetProperty(ref _scanControlIsEnable, value);
		}

		private bool _protocolControlIsEnable = false;
		public bool ProtocolControlIsEnable
		{
			get => _protocolControlIsEnable;
			set => SetProperty(ref _protocolControlIsEnable, value);
		}

		private bool _measurementControlIsEnable = false;
		private readonly IProtocolApplicationService _protocolApplicationService;

		public bool MeasurementControlIsEnable
		{
			get => _measurementControlIsEnable;
			set => SetProperty(ref _measurementControlIsEnable, value);
		}

		public MainControlViewModel(IProtocolApplicationService protocolApplicationService, IDialogService dialogService)
		{
			_dialogService = dialogService;
			_protocolApplicationService = protocolApplicationService;
			_protocolApplicationService.ProtocolTreeSelectNodeChanged += TreeSelectChanged;
			_protocolApplicationService.SelectBodyPartForProtocolChanged += SelectBodyPartForProtocolChanged;
			_protocolApplicationService.ApplicationClosing += _protocolApplicationService_ApplicationClosing;
		}

		private void _protocolApplicationService_ApplicationClosing(object? sender, AppService.Contract.ApplicationResponse e)
		{
			if (e.ApplicationName != ApplicationParameterNames.APPLICATIONNAME_PROTOCOLMANAGEMENT)
				return;

			if (e.NeedConfirm)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					_dialogService.ShowDialog(true, MessageLeveles.Info,
						Language.LanguageResource.Message_Info_CloseConfirmTitle
						, string.Format(Language.LanguageResource.Message_Confirm_CloseApplication, e.ApplicationName),
						arg =>
						{
							if (arg.Result == ButtonResult.OK)
							{
								Process.GetProcessById(e.ProcessId).Kill();
							}
						}, ConsoleSystemHelper.WindowHwnd);
				});
			}
			else
			{
				Process.GetProcessById(e.ProcessId).Kill();
			}
		}

		private void SelectBodyPartForProtocolChanged(object? sender, EventArgs<string> bodyPartName)
		{
			ProtocolControlIsEnable = true;
			ScanControlIsEnable = false;
			ReconControlIsEnable = false;
			MeasurementControlIsEnable = false;
		}

		private void TreeSelectChanged(object? sender, EventArgs<(string NodeType, string NodeId, string TemplateId)> e)
		{
			if (e.Data.NodeType == ProtocolLayeredName.PROTOCOL_NODE)
			{
				ProtocolControlIsEnable = true;
			}
			else
			{
				ProtocolControlIsEnable = false;
			}
			if (e.Data.NodeType == ProtocolLayeredName.SCAN_NODE)
			{
				ScanControlIsEnable = true;
			}
			else
			{
				ScanControlIsEnable = false;
			}
			if (e.Data.NodeType == ProtocolLayeredName.RECON_NODE)
			{
				ReconControlIsEnable = true;
			}
			else
			{
				ReconControlIsEnable = false;
			}
			if (e.Data.NodeType == ProtocolLayeredName.MEASUREMENT_NODE)
			{
				MeasurementControlIsEnable = true;
			}
			else
			{
				MeasurementControlIsEnable = false;
			}
		}

	}
}
