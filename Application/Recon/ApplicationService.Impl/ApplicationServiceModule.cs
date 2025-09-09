//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.Examination.ApplicationService.Contract.ScanControl;
using NV.CT.Examination.ApplicationService.Contract.ScanControl.Rule;
using NV.CT.Examination.ApplicationService.Impl;
using NV.CT.Examination.ApplicationService.Impl.ProtocolExtension.ModificationRule;
using NV.CT.Examination.ApplicationService.Impl.ProtocolExtensions.ModificationRule;
using NV.CT.Examination.ApplicationService.Impl.Recon;
using NV.CT.Examination.ApplicationService.Impl.ScanControl;
using NV.CT.Protocol.Services;
using NV.CT.SmartPositioning.Contract;

namespace NV.CT.Recon.ApplicationService.Impl;

public class ApplicationServiceModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<ProtocolStructureService>().As<IProtocolStructureService>().SingleInstance();
		builder.Register(c =>
		{
			var pss = new ProtocolModificationService();
			pss.AddLinkModificationRules(new TopoTubePositionTypeLinkedRule());
			pss.AddLinkModificationRules(new ScanLengthLinkedRule());
			pss.AddLinkModificationRules(new ScanStartPositionLinkedRule());
			pss.AddLinkModificationRules(new TableDirectionLinkedRule());
			pss.AddLinkModificationRules(new ImageOrderLinkedRule());
			return pss;
		}).As<IProtocolModificationService>().SingleInstance();

		builder.RegisterType<ReconProtocolHostService>().As<IProtocolHostService>().SingleInstance();

		builder.RegisterType<ProtocolPerformStatusService>().As<IProtocolPerformStatusService>().SingleInstance();
		builder.RegisterType<ReconStudyHostService>().As<IStudyHostService>().SingleInstance();
		builder.RegisterType<ReconRTDReconService>().As<IRTDReconService>().SingleInstance();
		builder.RegisterType<ReconScanStatusService>().As<IScanStatusService>().SingleInstance();
		builder.RegisterType<ReconMeasurementStatusService>().As<IMeasurementStatusService>().SingleInstance();
		builder.RegisterType<ReconScanControlService>().As<IScanControlService>().SingleInstance();
		builder.RegisterType<ReconSelectionManager>().As<ISelectionManager>().SingleInstance();
		builder.RegisterType<ReconUIRelatedStatusService>().As<IUIRelatedStatusService>().SingleInstance();
		builder.RegisterType<ReconImageOperationService>().As<IImageOperationService>().SingleInstance();
		builder.RegisterType<ReconSmartPositioningService>().As<ISmartPositioningService>().SingleInstance();
		builder.RegisterType<ReconSystemReadyService>().As<ISystemReadyService>().SingleInstance();

		builder.RegisterType<SmartPositioningServiceImpl>().SingleInstance();
		builder.RegisterType<SmartPositionService>().As<ISmartPositionContract>().SingleInstance();
		builder.RegisterType<ReconPageService>().As<IPageService>().SingleInstance();

		builder.RegisterType<GoValidateDialogService>().As<IGoValidateDialogService>().SingleInstance();
		builder.RegisterType<GoService>().As<IGoService>().SingleInstance();

		#region ScanControl related

		builder.RegisterType<GoButtonControlEnableService>();
		builder.RegisterType<CancelButtonControlEnableService>();
		builder.RegisterType<ReconAllButtonControlEnableService>();
		builder.RegisterType<ConfirmButtonControlEnableService>();
		builder.RegisterType<LayoutRule>().SingleInstance();
		builder.RegisterType<SystemReadyRule>().SingleInstance();
		builder.RegisterType<HasUnperformedScanRule>().SingleInstance();
		builder.RegisterType<OfflineReadyRule>().SingleInstance();

		builder.RegisterType<HasUnFinishedRTDReconRule>().SingleInstance();

		builder.RegisterType<ReconUIControlStatusService>().As<IUIControlStatusService>();

		#endregion
	}
}