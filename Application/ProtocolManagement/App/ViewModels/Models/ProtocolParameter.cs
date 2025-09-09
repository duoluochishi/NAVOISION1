using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.ProtocolManagement.ViewModels.Common;
using NV.CT.UI.ViewModel;
using System.Collections.Generic;

namespace NV.CT.ProtocolManagement.ViewModels.Models
{
    public class ProtocolParameter : BaseViewModel
    {
        //private Dictionary<string, EnumViewModel<ScanOption>> _scanOption;
        private string _scanOption;
        private string _examPartID;
        private string _bodySize;
        private string _bodyPart;
        private string _description;
        private string _isTabletSuitable = false.ToString();
        private string _isEnhanced = false.ToString();
        private string _isEmergency = false.ToString();
        private string _isFactory = false.ToString();
        private string _isValid = false.ToString();
        private string _protocolFamily;
        private string _sortID;
        private string _protocolName;


        public string ScanOption
        {
            get => _scanOption;
            set => SetProperty(ref _scanOption, value);
        }
        public string ProtocolName
        {
            get => _protocolName;
            set => SetProperty(ref _protocolName, value);
        }
        public string IsTabletSuitable
        {
            get => _isTabletSuitable;
            set => SetProperty(ref _isTabletSuitable, value);
        }
        public string ProtocolFamily
        {
            get => _protocolFamily;
            set => SetProperty(ref _protocolFamily, value);
        }
        public string IsEmergency
        {
            get => _isEmergency;
            set => SetProperty(ref _isEmergency, value);
        }
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        public string ExamPartID
        {
            get => _examPartID;
            set => SetProperty(ref _examPartID, value);
        }
        public string IsEnhanced
        {
            get => _isEnhanced;
            set => SetProperty(ref _isEnhanced, value);
        }
        public string IsFactory
        {
            get => _isFactory;
            set => SetProperty(ref _isFactory, value);
        }
        public string BodyPart
        {
            get => _bodyPart;
            set => SetProperty(ref _bodyPart, value);
        }
        public string BodySize
        {
            get => _bodySize;
            set => SetProperty(ref _bodySize, value);
        }
        public string IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }
        public string SortID
        {
            get => _sortID;
            set => SetProperty(ref _sortID, value);
        }


        public ProtocolParameter()
        {
            //ScanOption = EnumConverter.ToDic<ScanOption>();
        }
    }
}
