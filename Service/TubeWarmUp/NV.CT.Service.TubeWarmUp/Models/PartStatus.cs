using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.Service.Common.Framework;

namespace NV.CT.Service.TubeWarmUp.Models
{
    /// <summary>
    /// 部件状态。
    /// </summary>
    public enum PartStatus
    {
        Normal,
        Warning,
        Error,
        Disconnection
    }
    public abstract class AbstractPart : ViewModelBase
    {
        private PartStatus status;

        public PartStatus Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

    }
    public class CTBox : AbstractPart
    { }
    public class Pdu : AbstractPart
    {
    }
    public class IFBox : AbstractPart
    {
    }
    public class Gantry : AbstractPart { }
    /// <summary>
    /// aux控制板
    /// </summary>
    public class AuxBoard : AbstractPart
    {
    }
    /// <summary>
    /// 扩展板
    /// </summary>
    public class ExtBoard : AbstractPart
    {
    }
    public class TubeIntf : AbstractPart { }
    public class Table : AbstractPart { }
    public class Tube : AbstractPart
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private float oilTemp;

        public float OilTemp
        {
            get { return oilTemp; }
            set { SetProperty(ref oilTemp, value); }
        }
        private float heatCap;

        public float HeatCap
        {
            get { return heatCap; }
            set { SetProperty(ref heatCap, value); }
        }
    }
}
