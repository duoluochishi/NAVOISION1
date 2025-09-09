//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
//  2024/8/16 13:45:36    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Impl.Repository;
using NV.MPS.Environment;

namespace NV.CT.DatabaseService.Impl;

public class PrintConfigManager : IPrintConfigManager
{
    private readonly string _printConfigRootPath = RuntimeConfig.Console.PrintConfig.Path;
    private readonly string _configFileExtension = ".json";
    private readonly string _latest = "Latest";
    //private readonly string _history = "History";

    private ILogger<PrintConfigManager> _logger;
    private readonly StudyRepository _studyRepository;
    private string _currentStudyId = string.Empty;
    private (List<PrintingImageProperty> ImageList, List<ItemRect> LayoutItemList, ItemRect selectedLayoutItem) _printConfig;

    public (List<PrintingImageProperty> ImageList, List<ItemRect> LayoutItemList, ItemRect selectedLayoutItem) PrintConfig
    {
        get => this._printConfig;
    }

    public PrintConfigManager(ILogger<PrintConfigManager> logger, StudyRepository studyRepository)
    {
        _logger = logger;
        _studyRepository = studyRepository;
    }

    public (List<PrintingImageProperty> ImageList, List<ItemRect> LayoutItemList, ItemRect SelectedLayoutItem) LoadConfig(string studyId)
    {
        if (this._currentStudyId != studyId)
        {
            this._currentStudyId = studyId;
            this._printConfig = this.LoadConfigByStudyId(studyId);
        }

        return this._printConfig;
    }

    /// <summary>
    /// 添加打印图像信息
    /// </summary>
    /// <param name="studyId"></param>
    /// <param name="imagePropertyList"></param>
    /// <returns></returns>
    public bool AppendImagesToPrint(string studyId, List<PrintingImageProperty> imagePropertyList)
    {
        if (imagePropertyList is null || imagePropertyList.Count == 0)
        {
            this._logger.LogInformation("Invalid parameter imagePropertyList of AppendImagesToPrint.");
            return false;
        }

        if (this._currentStudyId != studyId)
        {   
            var config = this.LoadConfigByStudyId(studyId);
            config.ImageList.AddRange(imagePropertyList);
            return this.SavePrintConfig(studyId, config.ImageList, config.LayoutItemList, config.SelectedLayoutItem);
        }
        else
        {
            this._printConfig.ImageList.AddRange(imagePropertyList); 
            return true;
        }
    }

    /// <summary>
    /// 由打印模块更新内存对象的图像信息
    /// </summary>
    /// <param name="studyId"></param>
    /// <param name="imagePropertyList"></param>
    /// <returns></returns>
    public bool UpdateImageInformation(string studyId, List<PrintingImageProperty> imagePropertyList)
    {
        if (this._currentStudyId != studyId)
        {
            var config = this.LoadConfigByStudyId(studyId);
            config.ImageList = imagePropertyList is null ? new List<PrintingImageProperty>() : imagePropertyList;
            return this.SavePrintConfig(studyId, config.ImageList, config.LayoutItemList, config.SelectedLayoutItem);
        }
        else
        {
            this._printConfig.ImageList = imagePropertyList is null ? new List<PrintingImageProperty>() : imagePropertyList;
            return true;
        }
    }

    /// <summary>
    /// 由打印模块更新内存对象的布局信息
    /// </summary>
    /// <param name="studyId"></param>
    /// <param name="layoutItems"></param>
    /// <returns></returns>
    public bool UpdateLayOutInformation(string studyId, List<ItemRect> layoutItemList)
    {
        if (this._currentStudyId != studyId)
        {
            var config = this.LoadConfigByStudyId(studyId);
            config.LayoutItemList = layoutItemList is null ? new List<ItemRect>() : layoutItemList;
            return this.SavePrintConfig(studyId, config.ImageList, config.LayoutItemList, config.SelectedLayoutItem);
        }
        else
        {
            this._printConfig.LayoutItemList = layoutItemList is null ? new List<ItemRect>() : layoutItemList;
            return true;
        }
    }

    /// <summary>
    /// 由打印模块更新内存对象的选中的布局信息
    /// </summary>
    /// <param name="studyId"></param>
    /// <param name="selectedLayoutItem">选中的布局</param>
    /// <returns></returns>
    public bool UpdateSelectedLayOutInformation(string studyId, ItemRect selectedLayoutItem)
    {
        if (this._currentStudyId != studyId)
        {
            var config = this.LoadConfigByStudyId(studyId);
            config.SelectedLayoutItem = selectedLayoutItem;
            return this.SavePrintConfig(studyId, config.ImageList, config.LayoutItemList, config.SelectedLayoutItem);
        }
        else
        {
            this._printConfig.selectedLayoutItem = selectedLayoutItem;
            return true;
        }
    }

    /// <summary>
    /// 持久化内存对象
    /// </summary>
    /// <param name="studyId"></param>
    /// <returns></returns>
    public bool Save(string studyId)
    {
        if (this._currentStudyId != studyId)
        {
            this._logger.LogInformation("Invalid parameter of Save in PrintConfigManager:different studyId!");
            return false;
        }

        return this.SavePrintConfig(this._currentStudyId, this._printConfig.ImageList, this._printConfig.LayoutItemList, this._printConfig.selectedLayoutItem);
    }

    private (List<PrintingImageProperty> ImageList, List<ItemRect> LayoutItemList, ItemRect SelectedLayoutItem) LoadConfigByStudyId(string studyId)
    {
        string filePath = this.GetConfigFilePathByStudyId(studyId);
        if (!File.Exists(filePath))
        {
            return new(new List<PrintingImageProperty>(), new List<ItemRect>(), new ItemRect());
        }
        else
        {
            string fileContent = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<(List<PrintingImageProperty>, List<ItemRect>, ItemRect)>(fileContent);
        }
    }

    private bool SavePrintConfig(string studyId, List<PrintingImageProperty> imagePropertyList, List<ItemRect> layoutItemList, ItemRect selectedLayoutItem)
    {
        try
        {
            string filePath = this.GetConfigFilePathByStudyId(studyId);

            File.WriteAllText(filePath, (imagePropertyList, layoutItemList, selectedLayoutItem).ToJson()); //save to file
            return this._studyRepository.UpdatePrintConfigPath(studyId, filePath); //save to database
        }
        catch (Exception ex)
        {
            this._logger.LogError($"Failed to save PrintConfig in PrintConfigManager for studyId:{studyId}, the exception is:{ex.Message}");
            return false;
        }
    }

    private string GetConfigFilePathByStudyId(string studyId)
    {
        var directory = Path.Combine(this._printConfigRootPath, this._latest, studyId);
        if (!Directory.Exists(directory))
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Failed to create folder:{directory} with exception:{ex.Message}");
            }
        }

        return Path.Combine(directory, studyId + _configFileExtension);
    }
}