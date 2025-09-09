using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using System.Xml.Serialization;

namespace NV.CT.Protocol.Models
{
    [Serializable]
    public class BaseModel<TParent, TChild> : BaseModel<TParent>
    {
        [XmlIgnore]
        public virtual List<TChild> Children { get; set; }
    }

    [Serializable]
    public class BaseModel<TParent> : BaseModel
    {
        [XmlIgnore, JsonIgnore]
        public virtual TParent Parent { get; set; }
    }

    [Serializable]
    public class BaseModel
    {
        [XmlElement("Descriptor")]
        public DescriptorModel Descriptor { get; set; }

        [XmlArray("Parameters")]
        [XmlArrayItem("Parameter")]
        public List<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();

        public ParameterModel this[string parameterName] => Parameters.FirstOrDefault(p => p.Name == parameterName);

        [XmlIgnore, JsonIgnore]
        private PerformStatus _status;

        [XmlAttribute]
        public PerformStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                }
            }
        }

        [XmlAttribute]
        public FailureReasonType FailureReason { get; set; }

        private bool CheckParameter(string parameterName)
        {
            return Parameters.Any(p => p.Name == parameterName);
        }

        protected string GetParameterValue(string parameterName, string defaultValue = null, bool ignoreLogging = false)
        {
            if (!CheckParameter(parameterName))
            {
                if (!ignoreLogging)
                {
                    CTS.Global.Logger?.LogDebug($"Missing \"{parameterName}\" parameter in protocol xml.");
                }
                return !string.IsNullOrEmpty(defaultValue) ? defaultValue : string.Empty;
            }
            return this[parameterName].Value;
        }

        protected T GetParameterValue<T>(string parameterName, bool ignoreLogging = false)
        {
            if (!CheckParameter(parameterName))
            {
                if (!ignoreLogging)
                {
                    CTS.Global.Logger?.LogDebug($"Missing \"{parameterName}\" parameter in protocol xml.");
                }
                return default;
            }

            var tempValue = this[parameterName].Value;

            if (string.IsNullOrEmpty(tempValue) || tempValue.Equals("null") || tempValue.Equals("NaN") || tempValue.Equals("∞") || tempValue.Equals("inifity")) return default;

            return ParameterConverter.Convert<T>(tempValue);
        }
    }
}
