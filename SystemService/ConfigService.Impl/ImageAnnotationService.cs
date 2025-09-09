//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/6/28 13:39:36     V1.0.0       张震
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Impl;
using NV.CT.ConfigService.Models.UserConfig;
using NV.MPS.Environment;

namespace NV.CT.ConfigService.Server.Services;

public class ImageAnnotationService : IImageAnnotationService
{
    private readonly ConfigRepository<ImageAnnotationConfig> _repository;

    public event EventHandler ConfigRefreshed;

    public ImageAnnotationService()
    {
        _repository = new ConfigRepository<ImageAnnotationConfig>(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ConfigService", "FourCornersConfig.xml"), false);
        _repository.ConfigRefreshed += ImageAnnotationConfig_ConfigRefreshed;
    }

    private void ImageAnnotationConfig_ConfigRefreshed(object? sender, EventArgs e)
    {
        ConfigRefreshed?.Invoke(sender, e);
    }

    public void Save(ImageAnnotationConfig imageAnnotationConfig)
    {
        _repository.Save(imageAnnotationConfig);
    }
    
    public ImageAnnotationConfig GetConfigs()
    {
        return _repository.GetConfigs();
    }
}
