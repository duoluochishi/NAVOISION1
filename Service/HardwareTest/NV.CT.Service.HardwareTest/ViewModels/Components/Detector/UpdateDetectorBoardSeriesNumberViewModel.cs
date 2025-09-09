using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.FacadeProxy.Common.Models.Generic;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Helpers;
using NV.CT.Service.HardwareTest.Attachments.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using NV.MPS.Configuration;
using System;
using System.Linq;
using System.Windows;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.Detector
{
    public partial class UpdateDetectorBoardSeriesNumberViewModel : ObservableObject, IDialogAware
    {
        private readonly ILogService logService;

        public UpdateDetectorBoardSeriesNumberViewModel(ILogService logService)
        {
            //Get From DI
            this.logService = logService;
        }

        #region Properties

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UpdateDetectorInfo))]
        private uint detectorModuleID;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UpdateSeriesNumber))]
        private string firstPartOfSeriesNumber = string.Empty;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UpdateSeriesNumber))]
        private string secondPartOfSeriesNumber = string.Empty;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UpdateSeriesNumber))]
        private ProductCategory thirdPartOfSeriesNumber = ProductCategory.M;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UpdateSeriesNumber))]
        private FactoryCode forthPartOfSeriesNumber = FactoryCode.NBJ;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UpdateSeriesNumber))]
        private string fifthPartOfSeriesNumber = string.Empty;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UpdateSeriesNumber))]
        private string sixthPartOfSeriesNumber = string.Empty;
        [ObservableProperty]
        private string errorMessage = string.Empty;

        public string LastSeriesNumber { get; set; } = string.Empty;
        public string UpdateSeriesNumber =>
            $"{FirstPartOfSeriesNumber}#{SecondPartOfSeriesNumber}" +
            $"#{Enum.GetName(ThirdPartOfSeriesNumber)}#{Enum.GetName(ForthPartOfSeriesNumber)}" +
            $"#{FifthPartOfSeriesNumber}#{SixthPartOfSeriesNumber}";

        public string UpdateDetectorInfo => $"[DetectBoardSlot-{DetectorModuleID.ToString("00")}] Series Number : ";

        #endregion

        #region Commands

        [RelayCommand]
        private void Update()
        {
            //清空ErrorMessage
            ErrorMessage = string.Empty;
            //校验参数
            var formatResponse = ValidateSeriesNumberFormat();
            //判定
            if (!formatResponse.Status)
            {
                ErrorMessage = formatResponse.Message; return;
            }
            //校验序列号是否变化且唯一
            var uniqueResponse = ValidateUniqueSeriesNumber();
            //判定
            if (!uniqueResponse.Status) 
            {
                ErrorMessage = uniqueResponse.Message; return;
            }
            //生成新的DetectBoardSource
            var detectorBoardSource = new DetectBoardDto() 
            {
                DetectorModuleID = DetectorModuleID,
                InstallTime = DateTime.Now,
                SeriesNumber = UpdateSeriesNumber,
                Using = true
            };
            //发送消息
            WeakReferenceMessenger.Default.Send(new UpdateDetectorBoardSlotSourceMessage(detectorBoardSource));
            //关闭窗口
            DialogHelper.Close();
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogHelper.Close();
        }

        #endregion

        #region Dialog Aware

        public void EnterDialog(object[] parameters)
        {
            //解析detectorIndex和boardIndex
            _ = uint.TryParse(parameters[0].ToString(), out uint detectorModuleID);
            //更新
            DetectorModuleID = detectorModuleID;
            //解析当前序列号
            LastSeriesNumber = parameters[1].ToString()!;
            //更新显示
            if (parameters[1] == DependencyProperty.UnsetValue || string.IsNullOrWhiteSpace(LastSeriesNumber)) 
            {
                ResetSeriesNumber();
                return;
            }
            ParseSeriesNumber(LastSeriesNumber);
        }

        #endregion

        #region Utils

        /// <summary>
        /// 验证序列号格式
        /// </summary>
        /// <param name="seriesNumber"></param>
        /// <returns></returns>
        private GenericResponse<bool> ValidateSeriesNumberFormat() 
        {
            //空校验
            if (string.IsNullOrWhiteSpace(FirstPartOfSeriesNumber) || string.IsNullOrWhiteSpace(SecondPartOfSeriesNumber) ||
                string.IsNullOrWhiteSpace(FifthPartOfSeriesNumber) || string.IsNullOrWhiteSpace(SixthPartOfSeriesNumber)) 
            {
                return new(false, "Detected null parts.");
            }
            //格式校验
            if (FirstPartOfSeriesNumber.Length != 8) 
            {
                return new(false, "First part length should be 8.");
            }
            if (SecondPartOfSeriesNumber.Length != 2)
            {
                return new(false, "Second part length should be 2.");
            }
            if (FifthPartOfSeriesNumber.Length != 8)
            {
                return new(false, "Fifth part length should be 8.");
            }
            if (SixthPartOfSeriesNumber.Length != 4)
            {
                return new(false, "Sixth part length should be 4.");
            }

            return new(true, "The format of series number is correct.");
        }

        /// <summary>
        /// 验证唯一序列号
        /// </summary>
        /// <returns></returns>
        private GenericResponse<bool> ValidateUniqueSeriesNumber() 
        {
            //如果前后无变化，false
            if (LastSeriesNumber == UpdateSeriesNumber)
            {
                return new(false, "Detected no changes with series number.");
            }
            //获取检出板更新历史
            var records = DetectorModuleConfig.DetectBoardsExchangeHistoryConfig.DetectBoardExchangeRecords;
            //如果当前输入的序列号在历史记录中已安装在其他槽位
            var matchedRecords = records.Where(t => t.SeriesNumber.Equals(UpdateSeriesNumber) && t.Using == true).ToList();
            //校验
            if (matchedRecords is not null && matchedRecords.Count() > 0) 
            {
                return new(false, $"Input serise number: [{UpdateSeriesNumber}] has been installed on detector module {matchedRecords.First().DetectorModuleID}");
            }

            return new(true, "The series number is unique.");
        }

        /// <summary>
        /// 拆分序列号
        /// </summary>
        private void ParseSeriesNumber(string seriesNumber) 
        {
            //以#为分割进行拆解
            string[] parts = seriesNumber.Split('#');
            //更新显示
            FirstPartOfSeriesNumber = parts[0];
            SecondPartOfSeriesNumber = parts[1];
            ThirdPartOfSeriesNumber = Enum.Parse<ProductCategory>(parts[2]);
            ForthPartOfSeriesNumber = Enum.Parse<FactoryCode>(parts[3]);
            FifthPartOfSeriesNumber = parts[4];
            SixthPartOfSeriesNumber = parts[5];
        }

        /// <summary>
        /// 重置序列号
        /// </summary>
        private void ResetSeriesNumber() 
        {
            FirstPartOfSeriesNumber = string.Empty;
            SecondPartOfSeriesNumber = string.Empty;
            ThirdPartOfSeriesNumber = ProductCategory.M;
            ForthPartOfSeriesNumber = FactoryCode.NBJ;
            FifthPartOfSeriesNumber = string.Empty;
            SixthPartOfSeriesNumber = string.Empty;
        }

        #endregion

    }
}
