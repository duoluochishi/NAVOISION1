//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/16 13:45:36     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.MPS.Communication;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;

namespace NV.CT.ClientProxy.DataService;

public class PrintConfigManager : IPrintConfigManager
{
    private readonly MCSServiceClientProxy _clientProxy;

    public PrintConfigManager(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public (List<PrintingImageProperty> ImageList, List<ItemRect> LayoutItemList, ItemRect SelectedLayoutItem) LoadConfig(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPrintConfigManager).Namespace,
            SourceType = nameof(IPrintConfigManager),
            ActionName = nameof(IPrintConfigManager.LoadConfig),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<(List<PrintingImageProperty> ImageList, List<ItemRect> LayoutItemList, ItemRect SelectedLayoutItem)>(commandResponse.Data);
            return res;
        }

        return new(new List<PrintingImageProperty>(), new List<ItemRect>(), new ItemRect());
    }

    public bool AppendImagesToPrint(string studyId, List<PrintingImageProperty> imagePropertyList)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPrintConfigManager).Namespace,
            SourceType = nameof(IPrintConfigManager),
            ActionName = nameof(IPrintConfigManager.AppendImagesToPrint),
            Data = Tuple.Create(studyId, imagePropertyList).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return default;
    }

    public bool UpdateImageInformation(string studyId, List<PrintingImageProperty> imagePropertyList)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPrintConfigManager).Namespace,
            SourceType = nameof(IPrintConfigManager),
            ActionName = nameof(IPrintConfigManager.UpdateImageInformation),
            Data = Tuple.Create(studyId, imagePropertyList).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return default;
    }

    public bool UpdateLayOutInformation(string studyId, List<ItemRect> layoutItemList)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPrintConfigManager).Namespace,
            SourceType = nameof(IPrintConfigManager),
            ActionName = nameof(IPrintConfigManager.UpdateLayOutInformation),
            Data = Tuple.Create(studyId, layoutItemList).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return default;
    }

    public bool UpdateSelectedLayOutInformation(string studyId, ItemRect selectedLayoutItem)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPrintConfigManager).Namespace,
            SourceType = nameof(IPrintConfigManager),
            ActionName = nameof(IPrintConfigManager.UpdateSelectedLayOutInformation),
            Data = Tuple.Create(studyId, selectedLayoutItem).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return default;
    }

    public bool Save(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IPrintConfigManager).Namespace,
            SourceType = nameof(IPrintConfigManager),
            ActionName = nameof(IPrintConfigManager.Save),
            Data = studyId,
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return default;
    }

}
