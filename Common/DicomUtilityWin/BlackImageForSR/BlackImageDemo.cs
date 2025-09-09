//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using DoseReport.DicomModule;
using NV.CT.DicomUtility.BlackImage;
using NV.CT.DicomUtility.DicomCodeStringLib;
using NV.CT.DicomUtility.DicomIOD;
using NV.CT.DicomUtility.DicomModule;
using System;

namespace NV.CT.DicomUtility.BlackImageForSR
{
    public class BlackImageDemo
    {
        public void TestBlackImage()
        {
            var iod = CreateSecondaryCaptureImageIOD();
            var testAcqData = CreateACQTestDataForBlackImage();
            var path = @"d:\test_blackimage.dcm";
            BlackImageGenerator.Instance.GenerateBlackImage(iod, testAcqData, path);
        }

        private BlackImageConentInfo CreateACQTestDataForBlackImage()
        {

            ACQDoseInfoForBlackImage acq1 = new ACQDoseInfoForBlackImage()
            {
                Index = 1,
                SeriesDescription = "This is a test",
                ScanMode = "Preview",
                MAs = 1.01010101,
                KV = 1.0101010101,
                Cycles = 1,
                RotateTime = 1,
                CTDIvol = 1.0101010101,
                DLP = 101.0101,
                PhantomType = "Preview"
            };

            ACQDoseInfoForBlackImage acq2 = new ACQDoseInfoForBlackImage()
            {
                Index = 2,
                SeriesDescription = "MMMMMMMMMMMMM",
                ScanMode = "Spiral",
                MAs = 2.01010101,
                KV = 2.0101010101,
                Cycles = 2,
                RotateTime = 2,
                CTDIvol = 2.0101010101,
                DLP = 222.0101,
                PhantomType = "Spiral"
            };

            ACQDoseInfoForBlackImage acq3 = new ACQDoseInfoForBlackImage()
            {
                Index = 3,
                SeriesDescription = "This is a long sentence for test",
                ScanMode = "Sequence",
                MAs = 3.01010101,
                KV = 3.0101010101,
                Cycles = 3,
                RotateTime = 3,
                CTDIvol = 3.0101010101,
                DLP = 333.0101,
                PhantomType = "Sequence"
            };
            BlackImageConentInfo info = new BlackImageConentInfo()
            {
                StudyID = "123123123123",
                TotalDLP = 12312.3321,
                StudyTime = DateTime.Now,
                StudyInstanceUID = "123123123123"
            };
            info.AcqDoseInfos.Add(acq1);
            info.AcqDoseInfos.Add(acq2);
            info.AcqDoseInfos.Add(acq3);

            return info;
        }
        private SecondaryCaptureImageIOD CreateSecondaryCaptureImageIOD()
        {
            SecondaryCaptureImageIOD iod = new SecondaryCaptureImageIOD();
            FillPatientModule(iod.PatientModule);
            FillStudyModule(iod.GeneralStudyModule);
            FillGeneralSeriesModule(iod.GeneralSeriesModule);
            FillGeneralEquipment(iod.GeneralEquipmentModule);
            FillGeneralImageModule(iod.GeneralImageModule);
            FillImagePixelModule(iod.ImagePixelModule);
            FillSOPCommonForSC(iod.SOPCommonModule);
            FillCTImageForSC(iod.CTImageModule);
            FillImagePlaneForSC(iod.ImagePlaneModule);
            FillVOILUTForSC(iod.VOILUTModule);
            return iod;
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
        private void FillGeneralSeriesModule(GeneralSeriesModule seriesModule)
        {
            seriesModule.Modality = ModalityCS.CT;
            seriesModule.SeriesInstanceUID = "1.2.1.23..12.3.124.1.23..123";
        }
        private void FillGeneralImageModule(GeneralImageModule module)
        {
            module.InstanceNumber = 1;
            module.ImageType = @"ORIGINAL\PRIMARY\OTHER";
        }

        private void FillImagePixelModule(ImagePixelModule module)
        {
            module.SamplesPerPixel = 1;
            module.PhotometricInterpretation = PhotometricInterpretationCS.MONOCHROME2;
            module.Rows = 512;
            module.Columns = 512;
            module.BitsAllocated = 16;
            module.BitsStored = 12;
            module.HighBit = 11;
            module.PixelRepresentation = 0;
        }
        private void FillGeneralEquipment(GeneralEquipmentModule module)
        {
            module.Manufacturer = "NanoVision";
        }
        private void FillSOPCommonForSC(SOPCommonModule module)
        {
            module.SOPClassUID = "1.3.12.2.1107.5.1.7";
            module.SpecificCharacterSet = "GB18030";
            module.SOPInstanceUID = "1.3.12.2.1107.5.1.7.999999.30000022122010271742000000998";
        }
        private void FillCTImageForSC(CTImageModule module)
        {
            module.ImageType = @"ORIGINAL\PRIMARY\OTHER";
            module.SamplesPerPixel = 1;
            module.PhotometricInterpretation = PhotometricInterpretationCS.MONOCHROME2;
            module.BitsAllocated = 16;
            module.BitsStored = 12;
            module.HighBit = 11;
            module.RescaleIntercept = 0;
            module.RescaleSlope = 1;
        }

        private void FillImagePlaneForSC(ImagePlaneModule module)
        {
            module.PixelSpacing[0] = 1;
            module.PixelSpacing[1] = 1;
            module.ImagePosition[0] = 0;
            module.ImagePosition[1] = 0;
            module.ImagePosition[2] = 0;
        }

        private void FillVOILUTForSC(VOILUTModule module)
        {
            module.WindowCenter[0] = 128;
            module.WindowWidth[1] = 256;
        }
    }
}
