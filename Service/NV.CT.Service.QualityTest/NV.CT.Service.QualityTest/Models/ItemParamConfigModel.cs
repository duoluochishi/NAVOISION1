using System;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Models.ItemEntryParam;

namespace NV.CT.Service.QualityTest.Models
{
    public class ItemParamConfigModel
    {
        public QTType QTType { get; init; }
        public ItemEntryParamBaseModel[] ParamList { get; init; } = Array.Empty<ItemEntryParamBaseModel>();
    }
}