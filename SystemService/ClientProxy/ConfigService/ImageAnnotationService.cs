//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/8 12:36:50     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;

namespace NV.CT.ClientProxy.ConfigService;

public class ImageAnnotationService : IImageAnnotationService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public ImageAnnotationService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public event EventHandler ConfigRefreshed;

    public ImageAnnotationConfig GetConfigs()
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IImageAnnotationService).Namespace,//ImageAnnotation
            SourceType = nameof(IImageAnnotationService),
            ActionName = nameof(IImageAnnotationService.GetConfigs),
            Data = string.Empty
        });
        if (response is not null && response.Success)
        {
            return JsonConvert.DeserializeObject<ImageAnnotationConfig>(response.Data);
        }
        return default;
    }

    public void Save(ImageAnnotationConfig imageAnnotationConfig)
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IImageAnnotationService).Namespace,
            SourceType = nameof(IImageAnnotationService),
            ActionName = nameof(IImageAnnotationService.Save),
            Data = JsonConvert.SerializeObject(imageAnnotationConfig)
        });
    }
}
