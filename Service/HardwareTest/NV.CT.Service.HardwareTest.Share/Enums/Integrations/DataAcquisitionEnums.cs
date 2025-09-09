namespace NV.CT.Service.HardwareTest.Share.Enums.Integrations
{
    public enum DataAcquisitionStatus
    {
        NormalStop,
        AcquiringData                                                                                                                                                                                                             
    }

    public enum CalibrationType 
    {
        Offset,
        Gain
    }

    public enum CorrectionType 
    {
        Offset,
        Gain
    }

    public enum PixelType 
    {
        Ushort,
        Float
    }

    public enum ImageViewerActionType 
    {
        None,
        Zoom,
        Drag,
        Rotate,
        Rectangle,
        Circle,
        RectangleWWWL,
        AutoWWWL,
        OneToOne,
        AutoFit,
        ResetView,
        DeleteView,
        ClearView,
        Scroll
    }

    public enum FramesCountPerCycle 
    {
        Frames_1 = 1,
        Frames_180 = 180,
        Frames_360 = 360,
        Frames_720 = 720,
        Frames_1080 = 1080
    }

}
