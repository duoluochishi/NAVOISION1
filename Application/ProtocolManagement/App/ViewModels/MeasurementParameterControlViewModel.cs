using NV.CT.CTS;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;
using NV.CT.UI.ViewModel;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class MeasurementParameterControlViewModel : BaseViewModel
    {
        private readonly IProtocolApplicationService _protocolApplicationService;


        public MeasurementParameterControlViewModel(IProtocolApplicationService protocolApplicationService)
        {
            _protocolApplicationService = protocolApplicationService;
            _protocolApplicationService.ProtocolTreeSelectNodeChanged += TreeSelectChanged;
        }

        private void TreeSelectChanged(object? sender, EventArgs<(string NodeType, string NodeId, string TemplateId)> nodeTypeNodeIdAndTemplateID)
        {
            if (nodeTypeNodeIdAndTemplateID.Data.NodeType == ProtocolLayeredName.MEASUREMENT_NODE)
            {
                InitGetMeasurementParameter();
            }
            else
            {

            }

        }

        private void InitGetMeasurementParameter()
        {
            //TODO:目前没有Measurement参数
        }
    }
}
