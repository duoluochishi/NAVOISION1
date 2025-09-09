namespace NV.CT.Service.HardwareTest.Share.Enums.Components
{
    public enum CollimatorCalibrationStatus
    {
        NormalStop,
        PrephaseCalibrating,
        PrephaseCalibrationFailed,
        FrontBladeCalibrating,
        FrontBladeCalibrationFailed,
        RearBladeCalibrating,
        RearBladeCalibrationFailed,
        CalibrationTableSaving,
        CalibrationTableSaveAndConfigureFailed
    }

    public enum IterativeStatus
    {
        Error = -1,
        Continue = 0,
        Complete = 1,
    }
}
