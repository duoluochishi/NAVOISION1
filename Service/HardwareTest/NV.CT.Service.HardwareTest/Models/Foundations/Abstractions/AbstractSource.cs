using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.HardwareTest.Share.Enums;

namespace NV.CT.Service.HardwareTest.Models.Foundations.Abstractions
{
    public abstract partial class AbstractSource : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;
        [ObservableProperty]
        private XOnlineStatus online = XOnlineStatus.Offline;
        [ObservableProperty]
        private string firmwareVersion = "0.0.0.0";
        [ObservableProperty]
        private bool hasErrors = false;
        [ObservableProperty]    
        private string? errorMessage;

        public virtual void ResetOnlineStatus() 
        {
            this.Online = XOnlineStatus.Offline;
        }
    }

    public class TubeInterfaceSource : AbstractSource
    {
    }

    public class PDUSource : AbstractSource 
    {
        public PDUSource() : base()
        {
            this.Name = "PDU";
        }
    }

    public class CTBoxSource : AbstractSource 
    {
        public CTBoxSource() : base()
        {
            this.Name = "CTBox";
        }
    }

    public class IFBoxSource : AbstractSource
    {
        public IFBoxSource() : base()
        {
            this.Name = "IFBox";        
        }
    }

    public class AuxBoardSource : AbstractSource
    {
        public AuxBoardSource() : base() 
        {
            this.Name = "Aux Board";        
        }
    }

    public class ExtBoardSource : AbstractSource
    {
        public ExtBoardSource() : base()
        {
            this.Name = "Extension Board";
        }
    }

    public class ControlBoxSource : AbstractSource
    {
        public ControlBoxSource() : base()
        {
            this.Name = "Control Box";
        }
    }

    public class WirelessPanelSource : AbstractSource
    {
        public WirelessPanelSource() : base()
        {
            this.Name = "WirelessPanel";      
        }
    }

    public class GantryPanelSource : AbstractSource 
    {
        public GantryPanelSource() : base()
        {
            this.Name = "GantryPanel";
        }
    }

    public class BreathPanelSource : AbstractSource
    {
        public BreathPanelSource() : base()
        {
            this.Name = "BreathPanel";
        }
    }

}
