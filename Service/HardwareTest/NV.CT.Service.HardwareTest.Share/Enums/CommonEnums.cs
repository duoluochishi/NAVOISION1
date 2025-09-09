namespace NV.CT.Service.HardwareTest.Share.Enums
{
    public enum CommonSwitch 
    {
        Disable = 0,
        Enable             
    }

    public enum ConnectionStatus 
    {        
        Disconnected = 0,
        Connected
    }

    public enum XOnlineStatus 
    {
        Offline,
        Online    
    }

    public enum PrintLevel 
    {
        Info,
        Warn,
        Error
    }

    public enum ImageType 
    {
        RawData,
        Dicom
    }

    public enum AcquisitionType 
    {
        DarkField,
        BrightField
    }

}
