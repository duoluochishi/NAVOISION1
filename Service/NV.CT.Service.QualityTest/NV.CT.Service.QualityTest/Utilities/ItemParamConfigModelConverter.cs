using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Models.ItemEntryParam;

namespace NV.CT.Service.QualityTest.Utilities
{
    /*
     * .NET 7 以后可以直接使用JsonDerivedType特性进行多态序列化
     * .NET 6 及之前需要自己实现Converter进行多态序列化，详见微软文档 https://learn.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-6-0#support-polymorphic-deserialization
     */
    public class ItemParamConfigModelConverter : JsonConverter<ItemParamConfigModel>
    {
        public override bool CanConvert(Type type)
        {
            return typeof(ItemParamConfigModel).IsAssignableFrom(type);
        }

        public override ItemParamConfigModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                // throw new JsonException();
                return null;
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != nameof(ItemParamConfigModel.QTType))
            {
                // throw new JsonException();
                return null;
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            {
                // throw new JsonException();
                return null;
            }

            var typeStr = reader.GetString();
            var qtType = !string.IsNullOrWhiteSpace(typeStr) && Enum.TryParse(typeStr, true, out QTType e) ? e : QTType.None;

            if (!reader.Read() || reader.GetString() != nameof(ItemParamConfigModel.ParamList))
            {
                // throw new JsonException();
                return null;
            }

            var jsonOptions = SerializeUtility.GetJsonOptions();
            var paramList = qtType switch
            {
                QTType.IntegrationPhantom => JsonSerializer.Deserialize(ref reader, typeof(IntegrationPhantomParamModel[]), jsonOptions) as ItemEntryParamBaseModel[],
                QTType.SliceThicknessAxial or 
                QTType.SliceThicknessHelical or 
                QTType.CTOfWater or 
                QTType.NoiseOfWater or 
                QTType.ContrastScale => JsonSerializer.Deserialize(ref reader, typeof(SingleValueParamModel[]), jsonOptions) as ItemEntryParamBaseModel[],
                QTType.Homogeneity => JsonSerializer.Deserialize(ref reader, typeof(HomogeneityParamModel[]), jsonOptions) as ItemEntryParamBaseModel[],
                QTType.MTF_XY or QTType.MTF_Z => JsonSerializer.Deserialize(ref reader, typeof(MTFParamModel[]), jsonOptions) as ItemEntryParamBaseModel[],
                _ => null,
                // _ => JsonSerializer.Deserialize(ref reader, typeof(ItemEntryParamBaseModel[]), jsonOptions) as ItemEntryParamBaseModel[],
            };

            var ret = new ItemParamConfigModel
            {
                QTType = qtType,
                ParamList = paramList ?? Array.Empty<ItemEntryParamBaseModel>(),
            };

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                // throw new JsonException();
                return null;
            }

            return ret;
        }

        public override void Write(Utf8JsonWriter writer, ItemParamConfigModel value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            var jsonOptions = SerializeUtility.GetJsonOptions();
            writer.WriteString(nameof(ItemParamConfigModel.QTType), value.QTType.ToString());
            writer.WritePropertyName(nameof(ItemParamConfigModel.ParamList));

            switch (value.ParamList)
            {
                case IntegrationPhantomParamModel[] paramList:
                {
                    JsonSerializer.Serialize(writer, paramList, jsonOptions);
                    break;
                }
                case SingleValueParamModel[] paramList:
                {
                    JsonSerializer.Serialize(writer, paramList, jsonOptions);
                    break;
                }
                case HomogeneityParamModel[] paramList:
                {
                    JsonSerializer.Serialize(writer, paramList, jsonOptions);
                    break;
                }
                case MTFParamModel[] paramList:
                {
                    JsonSerializer.Serialize(writer, paramList, jsonOptions);
                    break;
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }

            writer.WriteEndObject();
        }
    }
}