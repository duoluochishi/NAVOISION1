using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Models.ScanReconModels;
using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Utilities;

namespace NV.CT.Service.QualityTest.Initializer
{
    internal static class ItemParamInit
    {
        public static void Init(ItemModel[] items)
        {
            if (!Directory.Exists(Global.ItemParamPath))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(Global.ItemParamPath))
            {
                var str = File.ReadAllText(file);
                var param = SerializeUtility.JsonDeserialize<ItemParamConfigModel>(str, new ItemParamConfigModelConverter());

                if (param == null || param.ParamList.Length == 0)
                {
                    continue;
                }

                var item = items.FirstOrDefault(i => i.QTType == param.QTType);

                if (item == null)
                {
                    continue;
                }

                item.Entries ??= new ObservableCollection<ItemEntryModel>();

                for (var i = 0; i < param.ParamList.Length; i++)
                {
                    var paramItem = param.ParamList[i];
                    paramItem.ScanParam.AmendScanLength();
                    var study = GetDefaultStudy();
                    var patient = GetDefaultPatient();
                    item.Entries.Add(new()
                    {
                        ID = i + 1,
                        IsChecked = true,
                        Parent = item,
                        Param = paramItem,
                        ScanReconParamDto = new()
                        {
                            Study = study,
                            Patient = patient,
                            ReconSeriesParams = [],
                        },
                        OfflineReconParamDto = new()
                        {
                            Study = study,
                            Patient = patient,
                            ReconSeriesParams = [],
                        },
                    });
                }

                item.IsAllChecked = true;
                item.SelectedEntry = item.Entries.FirstOrDefault();
            }
        }

        private static StudyModel GetDefaultStudy()
        {
            return new()
            {
                //StudyInstanceUID和StudyID先随便填一个，真正需要扫描时再更新
                StudyInstanceUID = "2024010100000001",
                StudyID = "QT",
                AccessionNumber = string.Empty,
                AdmissionID = string.Empty,
                StudyDescription = string.Empty,
                StudyDate = DateTime.Now,
                StudyTime = DateTime.Now,
                ReferringPhysiciansName = "Referring Physician Name",
                RequestingPhysician = "Requesting Physician",
                RequestingService = "Requesting Service",
                RequestedProcedurePriority = RequestedProcedurePriority.HIGH,
                SpecialNeeds = "Special Needs",
                PhysiciansOfRecord = "PhysicianOfRecord",
                NameOfPhysiciansReadingStudy = "Physician Reading Study Name",
                RequestedProcedureID = "ReqProcedureID",
                RequestedProcedureDescription = "Request Procedure Description",
                ScheduledProcedureStepID = string.Empty,
                ScheduledProcedureStepDescription = string.Empty,
                OperatorsName = "OperatorName",
            };
        }

        private static PatientModel GetDefaultPatient()
        {
            var birthTime = new DateTime(2000, 1, 1, 12, 30, 30);
            return new()
            {
                Name = "QT",
                ID = "PID-0001",
                Sex = PatientSex.F,
                BirthDate = birthTime,
                BirthTime = birthTime,
                Age = $"{DateAndTime.DateDiff(DateInterval.Year, birthTime, DateTime.Now):000}Y",
                Size = 175,
                Weight = 60,
                AdditionalHistory = "Additional Patient History",
                MedicalAlerts = "Medical Alerts",
                ContrastAller = "Contrast Aller Comments",
                OtherPatientIDs = "Other ID",
                EthnicGroup = "汉",
                Comment = "Patient Comments",
                PregnancyStatus = PregnantStatus.None,
                PatientStatus = "Sober",
            };
        }
    }
}