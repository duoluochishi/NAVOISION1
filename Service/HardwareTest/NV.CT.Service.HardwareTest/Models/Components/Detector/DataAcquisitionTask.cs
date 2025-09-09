using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition;
using NV.CT.Service.HardwareTest.Share.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace NV.CT.Service.HardwareTest.Models.Components.Detector
{
    public class DataAcquisitionTask
    {
        public DataAcquisitionTask(string name, DataAcquisitionParameters parameters)
        {
            Name = name;
            Parameters = parameters;
        }

        public string Name { get; set; } = string.Empty;
        public DataAcquisitionParameters Parameters { get; set; }
        public AcquisitionType AcquisitionType =>
            (Parameters.ExposureParameters.KVs.All(t => t == 0) || Parameters.ExposureParameters.MAs.All(t => t == 0)) ? AcquisitionType.DarkField : AcquisitionType.BrightField;

        public async Task<GenericResponse<bool>> ExecuteAsync() 
        {
            //配置采集参数
            var configureResponse = await Task.Run(() => DataAcquisitionProxy.Instance.ConfigureDataAcquisition(Parameters.ToProxyParam()));
            //若配置失败，直接返回
            if (!configureResponse.Status)
            {
                return configureResponse;
            }
            //开始数据采集
            var startResponse = await Task.Run(() => DataAcquisitionProxy.Instance.StartDataAcquisition(Parameters.ToProxyParam()));

            return startResponse;
        }
    }
}
