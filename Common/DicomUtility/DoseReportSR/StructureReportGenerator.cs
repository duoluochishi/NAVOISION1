//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using FellowOakDicom;
using FellowOakDicom.StructuredReport;
using NV.CT.DicomUtility.DicomCodeItems;
using NV.CT.DicomUtility.DicomIOD;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.DicomUtility.DoseReportSR
{
    public class StructureReportGenerator
    {
        private static StructureReportGenerator _structureReportGenerator;

        public static StructureReportGenerator Instance
        {
            get
            {
                if (null == _structureReportGenerator)
                {
                    _structureReportGenerator = new StructureReportGenerator();
                 }
                 return _structureReportGenerator;                
            }
        }

        public void GenerateDoseReportSR(XRayRadiationDoseSRIOD iod, DoseReportSRData srContentDetails,string path)
        {
            DicomDataset ds = new DicomDataset();

            iod.Update(ds);
            FillContentSequenceForDoseReport(srContentDetails, ds);

            var df = new DicomFile(ds);
            df.Save(path);
        }


        private void FillContentSequenceForDoseReport(DoseReportSRData input, DicomDataset ds)
        {
            ds.Add(DicomTag.ContentSequence, GetContentSequenceItems(input));
        }

        private DicomDataset[] GetContentSequenceItems(DoseReportSRData input)
        {
            var itemList = new List<DicomDataset>()
            {
                GetProcedureReportedItem(),
                GetObserverTypeItem(),
                GetDeviceObserverUIDItem(input.DeviceObserverUid),
                GetStartDateItem(input.StartDateTime),
                GetEndDateItem(input.EndDateTime),
                GetScopeOfAccumulationItem(input.StudyInstanceUID),
                GetCTAccumulatedDoseDataItem(input.TotalNumberOfIrrEvent,input.DoseLengthProductTotal)
            };

            foreach(var acquisitionData in input.CTAcquisitionDatas)
            {
                itemList.Add(GetCTAcquisitionItem(acquisitionData).Dataset);
            }
            itemList.Add(GetCommentItem(input.Comment).Dataset);
            itemList.Add(GetSourceOfDoseInformation().Dataset);

            return itemList.ToArray();
        }

        private DicomDataset GetConceptNameCodeSequenceForDS()
        {
            return ConceptNameCodeItems.XRayRadiationDoseReport_DCM;
        }

        private DicomDataset GetContentTemplateSequence()
        {
            DicomDataset ds = new DicomDataset();
            ds.Add(DicomTag.MappingResource, "DCMR");
            ds.Add(DicomTag.TemplateIdentifier, "10011 ");
            return ds;
        }


        /// <summary>
        /// EV(121058, DCM, "Procedure reported")
        /// </summary>
        /// <returns></returns>
        private DicomDataset GetProcedureReportedItem()
        {
            DicomCodeItem code_ProcedureReported = ConceptNameCodeItems.ProcedureReported_DCM;
            DicomCodeItem value_ProcedureReported = new DicomCodeItem("P5-08000", "SRT", "Computed Tomography X-Ray");
            DicomContentItem dci_ProcedureReported = new DicomContentItem(code_ProcedureReported, DicomRelationship.HasConceptModifier, value_ProcedureReported);

            DicomCodeItem code_HasIntent = ConceptNameCodeItems.HasIntent_SCT;
            DicomCodeItem value_HasIntent = new DicomCodeItem("R-408C3", "SRT", "Diagnostic Intent");
            DicomContentItem dci_HasIntent = new DicomContentItem(code_HasIntent, DicomRelationship.HasConceptModifier, value_HasIntent);

            dci_ProcedureReported.Dataset.Add(DicomTag.ContentSequence, dci_HasIntent.Dataset);

            return dci_ProcedureReported.Dataset;
        }

        /// <summary>
        /// EV(121005, DCM, "Observer Type")
        /// </summary>
        /// <returns></returns>
        private DicomDataset GetObserverTypeItem()
        {
            DicomCodeItem code_ObserverType = ConceptNameCodeItems.ObserverType_DCM;
            DicomCodeItem value_ObserverType = new DicomCodeItem("121007", "DCM", "Device");
            DicomContentItem dci_ObserverType = new DicomContentItem(code_ObserverType, DicomRelationship.HasObservationContext, value_ObserverType);
            return dci_ObserverType.Dataset;
        }

        /// <summary>
        /// EV (121012, DCM, "Device Observer UID")
        /// </summary>
        /// <returns></returns>
        private DicomDataset GetDeviceObserverUIDItem(string deviceObserverUID)
        {
            DicomCodeItem code_DeviceObserverUID = ConceptNameCodeItems.DeviceObserverUID_SCT;
            DicomUID value_DeviceObserverUID = new DicomUID(deviceObserverUID, "UID", DicomUidType.ApplicationContextName);
            DicomContentItem dci_DeviceObserverUID = new DicomContentItem(code_DeviceObserverUID, DicomRelationship.HasObservationContext, value_DeviceObserverUID);
            return dci_DeviceObserverUID.Dataset;
        }

        /// <summary>
        /// EV (113809, DCM, "Start of X-Ray Irradiation")
        /// </summary>
        /// <returns></returns>
        private DicomDataset GetStartDateItem(DateTime startDateTime)
        {
            DicomCodeItem code_StartDate = ConceptNameCodeItems.StartOfXRayIrradiation_DCM;
            DicomContentItem dci_StartDate = new DicomContentItem(code_StartDate, DicomRelationship.HasObservationContext, DicomValueType.DateTime, startDateTime);
            return dci_StartDate.Dataset;
        }

        /// <summary>
        /// EV (113810, DCM, "End of X-Ray Irradiation")
        /// </summary>
        /// <returns></returns>
        private DicomDataset GetEndDateItem(DateTime endDateTime)
        {
            DicomCodeItem code_EndDate = ConceptNameCodeItems.EndOfXRayIrradiation_DCM;
            DicomContentItem dci_StartDate = new DicomContentItem(code_EndDate, DicomRelationship.HasObservationContext, DicomValueType.DateTime, endDateTime);
            return dci_StartDate.Dataset;
        }

        /// <summary>
        /// EV (113705, DCM, "Scope of Accumulation")
        /// </summary>
        /// <returns></returns>
        private DicomDataset GetScopeOfAccumulationItem(string studyInstanceUID)
        {
            DicomCodeItem code_ScopeOfAccumulation = ConceptNameCodeItems.ScopeOfAccumulation_DCM;
            DicomCodeItem value_ScopeOfAccumulation = new DicomCodeItem("113014", "DCM", "Study");
            DicomContentItem dci_ScopeOfAccumulation = new DicomContentItem(code_ScopeOfAccumulation, DicomRelationship.HasObservationContext, value_ScopeOfAccumulation);

            DicomCodeItem code_StudyInstanceUID = ConceptNameCodeItems.StudyInstanceUID_DCM;
            DicomUID value_StudyInstanceUID = new DicomUID(studyInstanceUID, "UID", DicomUidType.SOPInstance);
            DicomContentItem dci_HasIntent = new DicomContentItem(code_StudyInstanceUID, DicomRelationship.HasProperties, value_StudyInstanceUID);

            dci_ScopeOfAccumulation.Dataset.Add(DicomTag.ContentSequence, dci_HasIntent.Dataset);

            return dci_ScopeOfAccumulation.Dataset;
        }

        /// <summary>
        /// EV (113811, DCM, "CT Accumulated Dose Data")
        /// </summary>
        /// <returns></returns>
        private DicomDataset GetCTAccumulatedDoseDataItem(int totalNumberOfIrrEvent,double doseLengthProductTotal)
        {
            DicomCodeItem code_CTAccumulatedDoseData = ConceptNameCodeItems.CTAccumulatedDoseData_DCM;

            DicomContentItem dci_CTAccumulatedDoseData = new DicomContentItem(code_CTAccumulatedDoseData, DicomRelationship.Contains, DicomContinuity.Separate,
                GetTotalNumberOfIrrEventItem(totalNumberOfIrrEvent),
                GetCTDoseLengthProductTotal(doseLengthProductTotal)
                );
            return dci_CTAccumulatedDoseData.Dataset;
        }
        /// <summary>
        /// EV (113812, DCM, "Total Number of Irradiation Events")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetTotalNumberOfIrrEventItem(int totalNumberOfIrrEvent )
        {
            DicomCodeItem code_TotalNumberOfIrrEvent = ConceptNameCodeItems.TotalNumberOfIrradiationEvents_DCM;
            DicomCodeItem code_unit = UnitCodeItems.EventsUnit_DCM;
            DicomMeasuredValue value_TotalNumberOfIrrEvent = new DicomMeasuredValue(totalNumberOfIrrEvent, code_unit);
            DicomContentItem dci_TotalNumberOfIrrEventItem = new DicomContentItem(code_TotalNumberOfIrrEvent, DicomRelationship.Contains, value_TotalNumberOfIrrEvent);

            return dci_TotalNumberOfIrrEventItem;
        }
        /// <summary>
        /// EV (113813, DCM, "CT Dose Length Product Total")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetCTDoseLengthProductTotal(double doseLengthProductTotal)
        {
            DicomCodeItem code_CTDoseLengthProductTotal = ConceptNameCodeItems.CTDoseLengthProductTotal_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MgYCMUnit_DCM;
            DicomMeasuredValue value_CTDoseLengthProductTotal = new DicomMeasuredValue((decimal)doseLengthProductTotal, code_unit);
            DicomContentItem dci_CTDoseLengthProductTotal = new DicomContentItem(code_CTDoseLengthProductTotal, DicomRelationship.Contains, value_CTDoseLengthProductTotal);

            return dci_CTDoseLengthProductTotal;
        }


        /// <summary>
        /// EV (113819, DCM, "CT Acquisition")
        /// </summary>
        /// <param name="acquisitionData"></param>
        /// <returns></returns>
        private DicomContentItem GetCTAcquisitionItem(CTAcquisitionData acquisitionData)
        {
            DicomCodeItem code_CTAcquisition = ConceptNameCodeItems.CTAcquisition_DCM;

            DicomContentItem dci_CTAcquisition = new DicomContentItem(code_CTAcquisition, DicomRelationship.Contains, DicomContinuity.Separate,
                GetCTAcquisitionProtocolItem(acquisitionData.AquisitionProtocol),
                GetTargetRegionItem(acquisitionData.BodyPart),
                GetCTAcquisitionType(acquisitionData.AcquisitionType),
                GetProcedureContextItem(acquisitionData.IsContrast),
                GetIrradiationEventUID(acquisitionData.IrradiationEventUID),
                GetCTAcquisitionParameters(acquisitionData)
                );
            if(acquisitionData.AcquisitionType != 0)
            {
                dci_CTAcquisition.Add(GetCTDose(acquisitionData.CTDoseInfo));                    
            }
            dci_CTAcquisition.Add(GetCommentItem(acquisitionData.Comment));

            dci_CTAcquisition.Add(GetDeviceRoleItem(acquisitionData.CTDeviceRoleParticipant));
            return dci_CTAcquisition;
        }

        /// <summary>
        /// EV (125203, DCM, "Acquisition Protocol")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetCTAcquisitionProtocolItem(string acqProtocol)
        {
            DicomCodeItem code = ConceptNameCodeItems.AcquisitionProtocol_DCM;
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains,DicomValueType.Text, acqProtocol);

            return dci;
        }

        /// <summary>
        /// EV (123014, DCM, "Target Region")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetTargetRegionItem(BodyPart bodyPart)
        {
            DicomCodeItem code = ConceptNameCodeItems.TargetRegion_DCM;
            DicomCodeItem value = DoseReportSRHelper.GetTargetRegion(bodyPart);
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);

            return dci;
        }

        /// <summary>
        /// EV (113820, DCM, "CT Acquisition Type")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetCTAcquisitionType(ScanOption acqType)
        {
            DicomCodeItem code = ConceptNameCodeItems.CTAcquisitionType_DCM;
            DicomCodeItem value = DoseReportSRHelper.GetCTAcquisitionType(acqType);
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);

            return dci;
        }

        /// <summary>
        /// EV (113769, DCM, "Irradiation Event UID")
        /// </summary>
        /// <param name="acquisitionData"></param>
        /// <returns></returns>
        private DicomContentItem GetIrradiationEventUID(string irrEventUID)
        {
            DicomCodeItem code = ConceptNameCodeItems.IrradiationEventUID_DCM;
            DicomUID uid = new DicomUID(irrEventUID, "UID", DicomUidType.ApplicationContextName);
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, uid);

            return dci;
        }

        /// <summary>
        /// EV (408730004, SCT, "Procedure Context")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetProcedureContextItem(bool isContrast)
        {
            DicomCodeItem code = ConceptNameCodeItems.ProcedureContext_SCT;
            var contrastResult = DoseReportSRHelper.GetProcedureContext(isContrast);
            DicomCodeItem value = new DicomCodeItem(contrastResult.Item1, contrastResult.Item2, contrastResult.Item3);
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);

            return dci;
        }

        /// <summary>
        /// EV (113822, DCM, "CT Acquisition Parameters")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetCTAcquisitionParameters(CTAcquisitionData acqData)
        {
            DicomCodeItem code = ConceptNameCodeItems.CTAcquisitionParameters_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, DicomContinuity.Separate,
                GetExposureTimeItem(acqData.ExposureTime),
                GetScanningLength(acqData.ScanningLength),
                GetNominalSingleCollimationWidthItem(acqData.NominalSingleCollimationWidth),
                GetTotalSingleCollimationWidthItem(acqData.TotalSingleCollimationWidth));

            if(acqData.AcquisitionType is ScanOption.Helical)
            {
                dci.Add(GetPitchFactorItem(acqData.PitchFactor));
            }
            dci.Add(GetNumberOfXRaySourcesItem(acqData.NumberOfXRaySources));

            foreach(var param in acqData.XRaySourceParams)
            {
                dci.Add(GetXRaySourceParametersItem(param, acqData.AcquisitionType is ScanOption.Surview or ScanOption.DualScout));
            }
            return dci;

        }

        /// <summary>
        /// EV (113824, DCM, "Exposure Time")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetExposureTimeItem(double exposureTime)
        {
            DicomCodeItem code = ConceptNameCodeItems.ExposureTime_DCM;
            DicomCodeItem code_unit = UnitCodeItems.SecondUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)exposureTime, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);

            return dci;
        }

        /// <summary>
        /// EV(113825, DCM, "Scanning Length")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetScanningLength(double scanningLength)
        {
            DicomCodeItem code = ConceptNameCodeItems.ScanningLength_DCM;
            DicomCodeItem code_unit = UnitCodeItems.Millimetre_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)scanningLength, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);

            return dci;
        }

        /// <summary>
        /// EV(113826, DCM, "Nominal Single Collimation Width")
        /// </summary>
        /// <param name="scanningLength"></param>
        /// <returns></returns>
        private DicomContentItem GetNominalSingleCollimationWidthItem(double nominalSingleCollimationWidth)
        {
            DicomCodeItem code = ConceptNameCodeItems.NominalSingleCollimationWidth_DCM;
            DicomCodeItem code_unit = UnitCodeItems.Millimetre_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)nominalSingleCollimationWidth, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);

            return dci;
        }


        /// <summary>
        /// EV (113827, DCM, "Nominal Total Collimation Width")
        /// </summary>
        /// <param name="scanningLength"></param>
        /// <returns></returns>
        private DicomContentItem GetTotalSingleCollimationWidthItem(double nominalTotalCollimationWidth)
        {
            DicomCodeItem code = ConceptNameCodeItems.NominalTotalCollimationWidth_DCM;
            DicomCodeItem code_unit = new DicomCodeItem("mm", "UCUM", "mm", "1.4");
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)nominalTotalCollimationWidth, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);

            return dci;
        }
        /// <summary>
        /// EV(113828, DCM, "Pitch Factor")
        /// </summary>
        /// <param name="scanningLength"></param>
        /// <returns></returns>
        private DicomContentItem GetPitchFactorItem(double pitchFactor)
        {
            DicomCodeItem code = ConceptNameCodeItems.PitchFactor_DCM;
            DicomCodeItem code_unit = UnitCodeItems.RatioUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)pitchFactor, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);

            return dci;
        }
        /// <summary>
        /// EV(113823, DCM, "Number of X-Ray Sources")
        /// </summary>
        /// <param name="scanningLength"></param>
        /// <returns></returns>
        private DicomContentItem GetNumberOfXRaySourcesItem(int num)
        {
            DicomCodeItem code = ConceptNameCodeItems.NumberOfXRaySources_DCM;
            DicomCodeItem code_unit = UnitCodeItems.XRaysources_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)num, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);

            return dci;
        }

        /// <summary>
        /// EV (113831, DCM, "CT X-Ray Source Parameters")
        /// </summary>
        /// <param name="scanningLength"></param>
        /// <returns></returns>
        private DicomContentItem GetXRaySourceParametersItem(XRaySourceParam param,bool isTopo)
        {
            DicomCodeItem code = ConceptNameCodeItems.CTXRaySourceParameters_DCM;

            DicomContentItem dci = new DicomContentItem(code,DicomRelationship.Contains,DicomContinuity.Separate,
               GetXRaySourceIdentificatioItem(param.IdentificationXRaySource),
               GetKVPItem(param.KVP),
               GetMaxTubeCurrentItem(param.MaxTubeCurrent),
               GetTubeCurrentItem(param.TubeCurrent)
               );

            if(!isTopo)
            {
                dci.Add(GetExposureTimePerRotateItem(param.ExposureTimePerRotate));
            }

            return dci;
        }

        /// <summary>
        /// EV (113832, DCM, "Identification of the X-Ray Source")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetXRaySourceIdentificatioItem(string id)
        {
            DicomCodeItem code = ConceptNameCodeItems.IdentificationOfTheXRaySource_DCM;
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains,DicomValueType.Text, id);
            return dci;
        }


        /// <summary>
        /// EV (113733, DCM, "KVP")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetKVPItem(double kvp)
        {
            DicomCodeItem code = ConceptNameCodeItems.KVP_DCM;
            DicomCodeItem code_unit = UnitCodeItems.KVUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)kvp, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113833, DCM, "Maximum X-Ray Tube Current")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetMaxTubeCurrentItem(double maxTubeCurrent)
        {
            DicomCodeItem code = ConceptNameCodeItems.MaximumXRayTubeCurrent_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MAUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)maxTubeCurrent, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113734, DCM, "X-Ray Tube Current")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetTubeCurrentItem(double tubeCurrent)
        {
            DicomCodeItem code = ConceptNameCodeItems.XRayTubeCurrent_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MAUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)tubeCurrent, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV(113834, DCM, "Exposure Time per Rotation")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetExposureTimePerRotateItem(double time)
        {
            DicomCodeItem code = ConceptNameCodeItems.ExposureTimePerRotation_DCM;
            DicomCodeItem code_unit = UnitCodeItems.SecondUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)time, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113829, DCM, "CT Dose")
        /// </summary>
        /// <param name="doseInfo"></param>
        /// <returns></returns>
        private DicomContentItem GetCTDose(CTDoseInfo doseInfo)
        {
            DicomCodeItem code = ConceptNameCodeItems.CTDose_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, DicomContinuity.Separate,
                GetMeanCTDIvolItem(doseInfo.MeanCTDIvol),
                GetCTDIwPhantomTypeItem(doseInfo.CTDIwPhantomType),
                GetDLPItem(doseInfo.DLP),
                GetDoseAlertDetailsItem(doseInfo.DoseCheckAlert),
                GetDoseNotificationDetailsItem( doseInfo.DoseCheckNotification)
                );

            return dci;
        }

        /// <summary>
        /// EV (113830, DCM, "Mean CTDIvol")
        /// </summary>
        /// <param name="meanCTDIvolItem"></param>
        /// <returns></returns>
        private DicomContentItem GetMeanCTDIvolItem(double meanCTDIvolItem)
        {
            DicomCodeItem code = ConceptNameCodeItems.MeanCTDIvol_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MgYUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)meanCTDIvolItem, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113835, DCM, "CTDIw Phantom Type")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetCTDIwPhantomTypeItem(int phantomType)
        {
            DicomCodeItem code = ConceptNameCodeItems.CTDIwPhantomType_DCM;
            var phantomResult = DoseReportSRHelper.GetPhantomType(phantomType);
            DicomCodeItem value = new DicomCodeItem(phantomResult.Item1, phantomResult.Item2, phantomResult.Item3);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113838, DCM, "DLP")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetDLPItem(double dlp)
        {
            DicomCodeItem code = ConceptNameCodeItems.DLP_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MgYCMUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)dlp, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113900, DCM, "Dose Check Alert Details")
        /// </summary>
        /// <param name="alert"></param>
        /// <returns></returns>
        private DicomContentItem GetDoseAlertDetailsItem(DoseCheckAlert alert)
        {
            DicomCodeItem code = ConceptNameCodeItems.DoseCheckAlertDetails_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, DicomContinuity.Separate,
                GetDLPAlertConfiguredItem(alert.DLPAlertConfigured),
                GetCTDIvolAlertConfiguredItem(alert.CTDIvolAlertConfigured)
                );

            bool isExceed = false;
            if(alert.DLPAlertConfigured)
            {
                dci.Add(GetDLPAlertValueItem(alert.DLPAlertValue));
                if(alert.AccumulatedDLPForwardEstimate >= alert.DLPAlertValue)
                {
                    dci.Add(GetAccumulatedDLPForwardEstimateItem(alert.AccumulatedDLPForwardEstimate));
                    isExceed = true;
                }
            }
            if(alert.CTDIvolAlertConfigured)
            {
                dci.Add(GetCTDIvolAlertValueItem(alert.CTDIvolAlertValue));
                if(alert.AccumulatedCTDIvolForwardEstimate >= alert.CTDIvolAlertValue)
                {
                    dci.Add(GetAccumulatedCTDIvolForwardEstimateItem(alert.AccumulatedCTDIvolForwardEstimate));
                    isExceed = true;
                }
            }

            if (isExceed)
            {
                dci.Add(GetReasonForProceedingItem(alert.ReasonForProceeding));
                dci.Add(GetPersonNameItem(alert.PersonName,alert.PersonRole));
            }
            return dci;
        }

        /// <summary>
        /// EV(113901, DCM, "DLP Alert Value Configured")
        /// </summary>
        /// <param name="isConfigured"></param>
        /// <returns></returns>
        private DicomContentItem GetDLPAlertConfiguredItem(bool isConfigured)
        {
            DicomCodeItem code = ConceptNameCodeItems.DLPAlertValueConfigured_DCM;
            var ynResult = DoseReportSRHelper.GetYesNo(isConfigured);
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, ynResult);
            return dci;
        }

        /// <summary>
        /// EV (113902, DCM, "CTDIvol Alert Value Configured")
        /// </summary>
        /// <param name="isConfigured"></param>
        /// <returns></returns>
        private DicomContentItem GetCTDIvolAlertConfiguredItem(bool isConfigured)
        {
            DicomCodeItem code = ConceptNameCodeItems.CTDIvolAlertValueConfigured_DCM;
            var ynResult = DoseReportSRHelper.GetYesNo(isConfigured);
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, ynResult);
            return dci;
        }

        /// <summary>
        /// EV(113903, DCM, "DLP Alert Value")
        /// </summary>
        /// <param name="dlpAlertValue"></param>
        /// <returns></returns>
        private DicomContentItem GetDLPAlertValueItem(double dlpAlertValue)
        {
            DicomCodeItem code = ConceptNameCodeItems.DLPAlertValue_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MgYCMUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)dlpAlertValue, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113904, DCM, "CTDIvol Alert Value")
        /// </summary>
        /// <param name="ctDIvolAlertValue"></param>
        /// <returns></returns>
        private DicomContentItem GetCTDIvolAlertValueItem(double ctDIvolAlertValue )
        {
            DicomCodeItem code = ConceptNameCodeItems.CTDIvolAlertValue_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MgYUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)ctDIvolAlertValue, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113905, DCM, "Accumulated DLP Forward Estimate")
        /// </summary>
        /// <param name="ctDIvolAlertValue"></param>
        /// <returns></returns>
        private DicomContentItem GetAccumulatedDLPForwardEstimateItem(double accumulatedDLPForwardEstimate)
        {
            DicomCodeItem code = ConceptNameCodeItems.AccumulatedDLPForwardEstimate_DCM;
            DicomCodeItem code_unit = new DicomCodeItem("mGy.cm", "UCUM", "mGy.cm", "1.4");
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)accumulatedDLPForwardEstimate, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113906, DCM, "Accumulated CTDIvol Forward Estimate")
        /// </summary>
        /// <param name="accumulatedCTDIvolForwardEstimate"></param>
        /// <returns></returns>

        private DicomContentItem GetAccumulatedCTDIvolForwardEstimateItem(double accumulatedCTDIvolForwardEstimate)
        {
            DicomCodeItem code = ConceptNameCodeItems.AccumulatedCTDIvolForwardEstimate_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MgYUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)accumulatedCTDIvolForwardEstimate, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113907, DCM, "Reason for Proceeding")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetReasonForProceedingItem(string reason)
        {
            DicomCodeItem code = ConceptNameCodeItems.ReasonForProceeding_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, DicomValueType.Text,reason);
            return dci;
        }

        /// <summary>
        /// EV (113870, DCM, "Person Name")
        /// EV (113875, DCM, "Person Role in Procedure")
        /// </summary>
        /// <param name="personName"></param>
        /// <param name="personRole"></param>
        /// <returns></returns>
        private DicomContentItem GetPersonNameItem(string personName, int personRole)
        {
            DicomCodeItem code = ConceptNameCodeItems.PersonName_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, DicomValueType.PersonName, personName);

            DicomCodeItem prCode = ConceptNameCodeItems.PersonRoleInProcedure_DCM;
            var prResult = DoseReportSRHelper.GetPersonRole(personRole);
            DicomCodeItem prValue = new DicomCodeItem(prResult.Item1, prResult.Item2, prResult.Item3);
            DicomContentItem prDci = new DicomContentItem(prCode, DicomRelationship.HasProperties, prValue);

            dci.Add(prDci);
            return dci;
        }

        /// <summary>
        /// EV (113908, DCM, "Dose Check Notification Details")
        /// </summary>
        /// <param name="nofification"></param>
        /// <returns></returns>
        private DicomContentItem GetDoseNotificationDetailsItem(DoseCheckNotification notification)
        {
            DicomCodeItem code = ConceptNameCodeItems.DoseCheckNotificationDetails_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, DicomContinuity.Separate,
                GetDLPNotificationConfiguredItem(notification.DLPNotificationConfigured),
                GetCTDIvolNotificationConfiguredItem(notification.CTDIvolNotificationConfigured)
                );

            bool isExceed = false;
            if (notification.DLPNotificationConfigured)
            {
                dci.Add(GetDLPNotificationValueItem(notification.DLPNotificationValue));
                if (notification.DLPForwardEstimate >= notification.DLPNotificationValue)
                {
                    dci.Add(GetDLPForwardEstimateItem(notification.DLPForwardEstimate));
                    isExceed = true;
                }
            }
            if (notification.CTDIvolNotificationConfigured)
            {
                dci.Add(GetCTDIvolNotificationValueItem(notification.CTDIolNotificationValue));
                if (notification.CTDIvolForwardEstimate >= notification.CTDIolNotificationValue)
                {
                    dci.Add(GetAccumulatedCTDIvolForwardEstimateItem(notification.CTDIvolForwardEstimate));
                    isExceed = true;
                }
            }

            if (isExceed)
            {
                dci.Add(GetReasonForProceedingItem(notification.ReasonForProceeding));
                dci.Add(GetPersonNameItem(notification.PersonName, notification.PersonRole));
            }
            return dci;
        }


        /// <summary>
        /// EV(113909, DCM, "DLP Notification Value Configured")
        /// </summary>
        /// <param name="isConfigured"></param>
        /// <returns></returns>
        private DicomContentItem GetDLPNotificationConfiguredItem(bool isConfigured)
        {
            DicomCodeItem code = ConceptNameCodeItems.DLPNotificationValueConfigured_DCM;
            var ynResult = DoseReportSRHelper.GetYesNo(isConfigured);
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, ynResult);
            return dci;
        }

        /// <summary>
        /// EV (113910, DCM, "CTDIvol Notification Value Configured")
        /// </summary>
        /// <param name="isConfigured"></param>
        /// <returns></returns>
        private DicomContentItem GetCTDIvolNotificationConfiguredItem(bool isConfigured)
        {
            DicomCodeItem code = ConceptNameCodeItems.CTDIvolNotificationValueConfigured_DCM;
            var ynResult = DoseReportSRHelper.GetYesNo(isConfigured);
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, ynResult);
            return dci;
        }


        /// <summary>
        /// EV (113911, DCM, "DLP Notification Value")
        /// </summary>
        /// <param name="dlpNotificationValue"></param>
        /// <returns></returns>
        private DicomContentItem GetDLPNotificationValueItem(double dlpNotificationValue)
        {
            DicomCodeItem code = ConceptNameCodeItems.DLPNotificationValue_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MgYCMUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)dlpNotificationValue, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV(113912, DCM, "CTDIvol Notification Value")
        /// </summary>
        /// <param name="ctDIvolNotificationValue"></param>
        /// <returns></returns>
        private DicomContentItem GetCTDIvolNotificationValueItem(double ctDIvolNotificationValue)
        {
            DicomCodeItem code = ConceptNameCodeItems.CTDIvolNotificationValue_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MgYUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)ctDIvolNotificationValue, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113913, DCM, "DLP Forward Estimate")
        /// </summary>
        /// <param name="dlpForwardEstimate"></param>
        /// <returns></returns>
        private DicomContentItem GetDLPForwardEstimateItem(double dlpForwardEstimate)
        {
            DicomCodeItem code = ConceptNameCodeItems.DLPForwardEstimate_DCM;
            DicomCodeItem code_unit = UnitCodeItems.MgYCMUnit_DCM;
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)dlpForwardEstimate, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (113914, DCM, "CTDIvol Forward Estimate")
        /// </summary>
        /// <param name="ctDIvolForwardEstimate"></param>
        /// <returns></returns>

        private DicomContentItem GetCTDIvolForwardEstimateItem(double ctDIvolForwardEstimate)
        {
            DicomCodeItem code = ConceptNameCodeItems.CTDIvolForwardEstimate_DCM;
            DicomCodeItem code_unit = new DicomCodeItem("mGy", "UCUM", "mGy", "1.4");
            DicomMeasuredValue value = new DicomMeasuredValue((decimal)ctDIvolForwardEstimate, code_unit);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }

        /// <summary>
        /// EV (121106, DCM, "Comment")
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        private DicomContentItem GetCommentItem(string comment)
        {
            DicomCodeItem code = ConceptNameCodeItems.Comment_DCM;
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, DicomValueType.Text, comment);

            return dci;
        }

        /// <summary>
        /// EV (113876, DCM, "Device Role in Procedure")
        /// </summary>
        /// <param name="deviceRole"></param>
        /// <returns></returns>
        private DicomContentItem GetDeviceRoleItem(CTDeviceRoleParticipant deviceRole)
        {
            DicomCodeItem code = ConceptNameCodeItems.DeviceRoleInProcedure_DCM;
            DicomCodeItem value = new DicomCodeItem("113859", "DCM", "Irradiating Device");
            DicomContentItem dci = new DicomContentItem(code,DicomRelationship.Contains,value);

            dci.Add(GetDeviceNameItem(deviceRole.DeviceName));
            dci.Add(GetDeviceManufacturerItem(deviceRole.Manufacturer));
            dci.Add(GetModelNameItem(deviceRole.ModelName));
            dci.Add(GetDeviceSerialNumberItem(deviceRole.SerialNumber));
            dci.Add(GetObserverUIDItem(deviceRole.ObserverUID));

            return dci;
        }

        /// <summary>
        /// EV (113877, DCM, "Device Name")
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        private DicomContentItem GetDeviceNameItem(string manufacturer)
        {
            DicomCodeItem code = ConceptNameCodeItems.DeviceName_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.HasProperties, DicomValueType.Text, manufacturer);
            return dci;
        }

        /// <summary>
        /// EV (113878, DCM, "Device Manufacturer")
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        private DicomContentItem GetDeviceManufacturerItem(string manufacturer)
        {
            DicomCodeItem code = ConceptNameCodeItems.DeviceManufacturer_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.HasProperties, DicomValueType.Text, manufacturer);
            return dci;
        }

        /// <summary>
        /// EV (113879, DCM, "Device Model Name")
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        private DicomContentItem GetModelNameItem(string modelName)
        {
            DicomCodeItem code = ConceptNameCodeItems.DeviceModelName_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.HasProperties, DicomValueType.Text, modelName);
            return dci;
        }

        /// <summary>
        /// EV (113880, DCM, "Device Serial Number")
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        private DicomContentItem GetDeviceSerialNumberItem(string serialNumber)
        {
            DicomCodeItem code = ConceptNameCodeItems.DeviceSerialNumber_DCM;

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.HasProperties, DicomValueType.Text, serialNumber);
            return dci;
        }

        /// <summary>
        /// EV (121012, DCM, "Device Observer UID")
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        private DicomContentItem GetObserverUIDItem(string observerUID)
        {
            DicomCodeItem code = ConceptNameCodeItems.DeviceObserverUID_SCT;
            DicomUID uid = new DicomUID(observerUID, "UID", DicomUidType.SOPInstance);

            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.HasProperties,  uid);
            return dci;
        }


        /// <summary>
        /// EV (113854, DCM, "Source of Dose Information")
        /// </summary>
        /// <returns></returns>
        private DicomContentItem GetSourceOfDoseInformation()
        {
            DicomCodeItem code = ConceptNameCodeItems.SourceOfDoseInformation_DCM;
            DicomCodeItem value = new DicomCodeItem("113856", "DCM", "Automated Data Collection");
            DicomContentItem dci = new DicomContentItem(code, DicomRelationship.Contains, value);
            return dci;
        }
    }
}
