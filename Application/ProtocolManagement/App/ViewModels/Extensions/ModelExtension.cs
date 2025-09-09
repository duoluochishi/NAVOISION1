using NV.CT.Protocol.Models;
using NV.CT.ProtocolManagement.ViewModels.Models;
using NV.CT.ProtocolManagement.ViewModels.Enums;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;

namespace NV.CT.ProtocolManagement.ViewModels.Extensions
{
    public static class ModelExtension
    {
        public static Node? ModelToNode<T>(this T model, string templateID) where T : BaseModel
        {
            return model.GetType().Name switch
            {
                ProtocolLayeredName.PROTOCOL_MODEL => ProtocolModelToNode(model, templateID),
                ProtocolLayeredName.MEASUREMENT_MODEL => MeasurementModelToNode(model, templateID),
                ProtocolLayeredName.SCAN_MODEL => ScanModelToNode(model, templateID),
                ProtocolLayeredName.RECON_MODEL => ReconModelToNode(model, templateID),
                _ => null,
            };
        }

        private static void ModelToNodeCommonPara(BaseModel model, string templateID, ref Node node)
        {
            node.NodeId = model.Descriptor.Id;
            node.NodeName = model.Descriptor.Name;
            node.ProtocolTemplateId = templateID;
        }

        private static Node ProtocolModelToNode<T>(T model, string templateID) where T : class
        {
            Node protocolNode = new(NodeType.ProtocolNode);
            var protocolModel = model as ProtocolModel;
            if (protocolModel is not null)
            {
                if (protocolModel.Descriptor.Type != "Measurement")
                {
                    ModelToNodeCommonPara(protocolModel, templateID, ref protocolNode);
                    protocolNode.ParentID = templateID;
                }

                protocolModel.Children.ForEach(FOR =>
                {
                    FOR.Children.ForEach(measurement =>
                    {
                        var measurementNode = measurement.ModelToNode(templateID);
                        if (measurementNode is not null)
                        {
                            protocolNode.Children.Add(measurementNode);
                        }
                    });
                });
            }
            return protocolNode;
        }

        private static Node MeasurementModelToNode<T>(T model, string templateID)
        {
            Node measurementNode = new(NodeType.MeasurementNode);
            var measurementModel = model as MeasurementModel;
            if (measurementModel is not null)
            {
                ModelToNodeCommonPara(measurementModel, templateID, ref measurementNode);
                measurementNode.ParentID = measurementModel.Parent.Parent.Descriptor.Id;//this is Protocol Level
                measurementModel.Children.ForEach(scan =>
                {
                    Node? scanNode = scan.ModelToNode(templateID);
                    if (scanNode is not null)
                    {
                        measurementNode.Children.Add(scanNode);
                    }
                });
            }
            return measurementNode;
        }

        private static Node ScanModelToNode<T>(T model, string templateID)
        {
            Node scanNode = new(NodeType.ScanNode);
            var scanModel = model as ScanModel;
            if (scanModel is not null)
            {
                ModelToNodeCommonPara(scanModel, templateID, ref scanNode);
                scanNode.ParentID = scanModel.Parent.Descriptor.Id;
                scanModel.Children.ForEach(recon =>
                {
                    Node? reconNode = recon.ModelToNode(templateID);
                    if (reconNode is not null)
                    {
                        scanNode.Children.Add(reconNode);
                        reconNode.Parent = scanNode;
                    }
                });
            }
            return scanNode;
        }

        private static Node ReconModelToNode<T>(T model, string templateID)
        {
            Node reconNode = new(NodeType.ReconNode);
            var reconModel = model as ReconModel;
            if (reconModel is not null)
            {
                ModelToNodeCommonPara(reconModel, templateID, ref reconNode);
                reconNode.ParentID = reconModel.Parent.Descriptor.Id;
            }
            return reconNode;
        }
    }
}
