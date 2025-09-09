//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.Examination.ApplicationService.Contract.ScanControl;
using NV.CT.Examination.ApplicationService.Contract.ScanControl.Rule;
using NV.CT.Examination.ApplicationService.Impl.ProtocolExtension.ModificationRule;
using NV.CT.Examination.ApplicationService.Impl.ProtocolExtensions.ModificationRule;
using NV.CT.Examination.ApplicationService.Impl.ScanControl;
using NV.CT.Protocol.Services;
using NV.CT.SmartPositioning.Contract;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Impl;

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

        builder.RegisterType<ProtocolPerformStatusService>().As<IProtocolPerformStatusService>().SingleInstance();
        builder.RegisterType<ProtocolHostService>().As<IProtocolHostService>().SingleInstance();
        builder.RegisterType<StudyHostService>().As<IStudyHostService>().SingleInstance();
        builder.RegisterType<RTDReconService>().As<IRTDReconService>().SingleInstance();
        builder.RegisterType<ScanStatusService>().As<IScanStatusService>().SingleInstance();
        builder.RegisterType<MeasurementStatusService>().As<IMeasurementStatusService>().SingleInstance();
        builder.RegisterType<ScanControlService>().As<IScanControlService>().SingleInstance();
        builder.RegisterType<SelectionManager>().As<ISelectionManager>().SingleInstance();
        builder.RegisterType<SystemReadyService>().As<ISystemReadyService>().SingleInstance();
        builder.RegisterType<UIRelatedStatusService>().As<IUIRelatedStatusService>().SingleInstance();
        builder.RegisterType<ImageOperationService>().As<IImageOperationService>().SingleInstance();
        builder.RegisterType<SmartPositioningService>().As<ISmartPositioningService>().SingleInstance();

        builder.RegisterType<SmartPositioningServiceImpl>().SingleInstance();
        builder.RegisterType<SmartPositionService>().As<ISmartPositionContract>().SingleInstance();
        builder.RegisterType<PageService>().As<IPageService>().SingleInstance();

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

        builder.RegisterType<UIControlStatusService>().As<IUIControlStatusService>();

        #endregion      
    }
}