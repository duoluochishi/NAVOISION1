//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.DicomUtility.DicomCodeStringLib;
using NV.CT.DicomUtility.DicomIOD;
using NV.CT.DicomUtility.DicomModule;
using NV.CT.FacadeProxy.Common.Enums;
using System;

namespace NV.CT.DicomUtility.DoseReportSR
{
    public class DoseReportSRDemo
    {

        public void TestDoseReport()
        {
            var iod = CreateTestDoseSRIOD();
            var test = CreateDoseReportSRDataForTest();
            var path = @"d:\test_structurereport.dcm";
            StructureReportGenerator.Instance.GenerateDoseReportSR(iod, test, path);
        }

        private XRayRadiationDoseSRIOD CreateTestDoseSRIOD()
        {
            var result = new XRayRadiationDoseSRIOD();

            FillPatientModule(result.PatientModule);
            FillStudyModule(result.GeneralStudyModule);
            FillSRDocumentSeriesModule(result.SRDocumentSeriesModule);
            FillGeneralEquipment(result.GeneralEquipmentModule);
            FillEnhancedEquipment(result.EnhancedGeneralEquipmentModule);
            FillSRDocumentGeneral(result.SRDocumentGeneralModule);
            FillSRDocumentContent(result.SRDocumentContentModule);
            FillSOPCommon(result.SOPCommonModule);
            return result;
        }


        private void FillPatientModule(PatientModule module)
        {
            module.PatientName = "liyong";
            module.PatientBirthDate = DateTime.Now;
            module.PatientID = "123123123";
            module.PatientSex = PatientSexCS.M;
        }

        private void FillStudyModule(GeneralStudyModule module)
        {
            module.StudyInstanceUID = "1.3.12.2.1107.5.1.7.999999.30000022122010261026900000003";
            module.StudyDate = DateTime.Now;
            module.StudyTime = DateTime.Now;
            module.StudyID = "192837465";
            module.AccessionNumber = "1111111";
            module.ReferringPhysicianName = "Operator";
        }

        private void FillSRDocumentSeriesModule(SRDocumentSeriesModule module)
        {
            module.Modality = ModalityCS.SR;
            module.SeriesNumber = 101;
            module.SeriesInstanceUID = "1.3.12.2.1107.5.1.7.999999.30000022122010271742000000999";
        }

        private void FillGeneralEquipment(GeneralEquipmentModule module)
        {
            module.Manufacturer = "NanoVision";
        }

        private void FillEnhancedEquipment(EnhancedGeneralEquipmentModule module)
        {
            module.Manufacturer = "NanoVision";
            module.SoftwareVersions = "Nano1.0";
            module.ManufacturerModelName = "Eye24";
            module.DeviceSerialNumber = "11000011";
        }

        private void FillSRDocumentGeneral(SRDocumentGeneralModule module)
        {
            module.ContentDate = DateTime.Now;
            module.ContentTime = DateTime.Now;
            module.VerificationFlag = VerificationFlagCS.UNVERIFIED;
            module.CompletionFlag = CompletionFlagCS.COMPLETE;
        }
        private void FillSRDocumentContent(SRDocumentContentModule module)
        {
            module.ValueType = ValueTypeCS.CONTAINER;
            module.ContinuityOfContent = ContinuityOfContentCS.SEPARATE;
        }

        private void FillSOPCommon(SOPCommonModule module)
        {
            module.SpecificCharacterSet = "GB18030";
            module.SOPInstanceUID = "1.3.12.2.1107.5.1.7.999999.30000022122010271742000001000";
        }


        private DoseReportSRData CreateDoseReportSRDataForTest()
        {
            DoseReportSRData data = new DoseReportSRData
            {
                DeviceObserverUid = "1.2.156.14702.1.1005.0",
                StartDateTime = DateTime.Now,
                EndDateTime = DateTime.Now,
                StudyInstanceUID = "1.3.12.2.1107.5.1.7.999999.30000022122010261026900000003",
                TotalNumberOfIrrEvent = 3,
                DoseLengthProductTotal = 45.02,
                Comment = ""
            };

            CTDeviceRoleParticipant deviceRole = new CTDeviceRoleParticipant()
            {
                DeviceName = "1111111",
                ModelName = "2222222222",
                Manufacturer = "Nano",
                SerialNumber = "123123123123",
                ObserverUID = "1.3.12.2.1107.5.1.7.999999"
            };


            CTDoseInfo notConfiguredDose = new CTDoseInfo()
            {
                DLP = 1.1111,
                CTDIwPhantomType = 0,
                MeanCTDIvol = 1.1111,
                DoseCheckAlert = new DoseCheckAlert()
                {
                    CTDIvolAlertConfigured = false,
                    DLPAlertConfigured = false,
                },
                DoseCheckNotification = new DoseCheckNotification()
                {
                    DLPNotificationConfigured = false,
                    CTDIvolNotificationConfigured = false,
                }
            };

            CTDoseInfo notExceedDoseInfo = new CTDoseInfo()
            {
                DLP = 1.1111,
                CTDIwPhantomType = 0,
                MeanCTDIvol = 1.1111,
                DoseCheckAlert = new DoseCheckAlert()
                {
                    CTDIvolAlertConfigured = true,
                    DLPAlertConfigured = true,
                    DLPAlertValue = 100,
                    AccumulatedDLPForwardEstimate = 90,
                    CTDIvolAlertValue = 1000,
                    AccumulatedCTDIvolForwardEstimate = 900
                },
                DoseCheckNotification = new DoseCheckNotification()
                {
                    DLPNotificationConfigured = true,
                    CTDIvolNotificationConfigured = true,
                    DLPNotificationValue = 10,
                    DLPForwardEstimate = 9,
                    CTDIolNotificationValue = 100,
                    CTDIvolForwardEstimate = 90
                }
            };

            CTDoseInfo allDoseInfo = new CTDoseInfo()
            {
                DLP = 1.1111,
                CTDIwPhantomType = 0,
                MeanCTDIvol = 1.1111,
                DoseCheckAlert = new DoseCheckAlert()
                {
                    CTDIvolAlertConfigured = true,
                    DLPAlertConfigured = true,
                    DLPAlertValue = 90,
                    AccumulatedDLPForwardEstimate = 100,
                    CTDIvolAlertValue = 900,
                    AccumulatedCTDIvolForwardEstimate = 1000,
                    ReasonForProceeding = "kljhaljhsdf",
                    PersonName = "person1",
                    PersonRole = 0,
                },
                DoseCheckNotification = new DoseCheckNotification()
                {
                    DLPNotificationConfigured = true,
                    CTDIvolNotificationConfigured = true,
                    DLPNotificationValue = 9,
                    DLPForwardEstimate = 10,
                    CTDIolNotificationValue = 90,
                    CTDIvolForwardEstimate = 100,
                    ReasonForProceeding = "ttttttttttttt",
                    PersonName = "person2",
                    PersonRole = 0,
                }
            };



            XRaySourceParam p1 = new XRaySourceParam()
            {
                IdentificationXRaySource = "AAAA",
                KVP = 1.11,
                MaxTubeCurrent = 1.1,
                TubeCurrent = 1.11,
                ExposureTimePerRotate = 1.1
            };
            XRaySourceParam p2 = new XRaySourceParam()
            {
                IdentificationXRaySource = "BBBB",
                KVP = 2.22,
                MaxTubeCurrent = 2.22,
                TubeCurrent = 2.22,
                ExposureTimePerRotate = 2.22,
            };
            XRaySourceParam p3 = new XRaySourceParam()
            {
                IdentificationXRaySource = "CCCC",
                KVP = 3.33,
                MaxTubeCurrent = 3.33,
                TubeCurrent = 3.33,
                ExposureTimePerRotate = 3.33,
            };

            CTAcquisitionData topoAcq = new CTAcquisitionData()
            {
                AquisitionProtocol = "TOPO",
                BodyPart = BodyPart.HEAD,
                AcquisitionType = ScanOption.Surview,
                IrradiationEventUID = "1.2.156.14702.1.1005.64.1.20170109154412422100000000",
                IsContrast = false,
                ExposureTime = 1.11,
                ScanningLength = 1.11,
                NominalSingleCollimationWidth = 1.11,
                TotalSingleCollimationWidth = 1.11,
                NumberOfXRaySources = 1,
                CTDoseInfo = notConfiguredDose,
                CTDeviceRoleParticipant = deviceRole

            };
            topoAcq.XRaySourceParams.Add(p1);


            CTAcquisitionData SequenceAcq = new CTAcquisitionData()
            {
                AquisitionProtocol = "Sequence",
                BodyPart = BodyPart.HEAD,
                AcquisitionType = ScanOption.Axial,
                IrradiationEventUID = "1.2.156.14702.1.1005.64.1.20170109154412422100000001",
                IsContrast = false,
                ExposureTime = 2.22,
                ScanningLength = 2.22,
                NominalSingleCollimationWidth = 2.22,
                TotalSingleCollimationWidth = 2.22,
                PitchFactor = 2.22,
                NumberOfXRaySources = 2,
                CTDoseInfo = notExceedDoseInfo,
                CTDeviceRoleParticipant = deviceRole
            };
            SequenceAcq.XRaySourceParams.Add(p1);
            SequenceAcq.XRaySourceParams.Add(p2);


            CTAcquisitionData spiralAcq = new CTAcquisitionData()
            {
                AquisitionProtocol = "Spiral",
                BodyPart = BodyPart.HEAD,
                AcquisitionType = ScanOption.Helical,
                IrradiationEventUID = "1.2.156.14702.1.1005.64.1.20170109154412422100000002",
                IsContrast = true,
                ExposureTime = 3.33,
                ScanningLength = 3.33,
                NominalSingleCollimationWidth = 3.33,
                TotalSingleCollimationWidth = 3.33,
                PitchFactor = 3.33,
                NumberOfXRaySources = 3,
                CTDoseInfo = allDoseInfo,
                CTDeviceRoleParticipant = deviceRole
            };
            spiralAcq.XRaySourceParams.Add(p1);
            spiralAcq.XRaySourceParams.Add(p2);
            spiralAcq.XRaySourceParams.Add(p3);


            data.CTAcquisitionDatas.Add(topoAcq);
            data.CTAcquisitionDatas.Add(SequenceAcq);
            data.CTAcquisitionDatas.Add(spiralAcq);
            return data;
        }
    }
}
