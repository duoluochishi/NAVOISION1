using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.Service.Common.Framework;

namespace NV.CT.Service.TubeWarmUp.Models
{
    public class DeviceParts : ViewModelBase
    {
        int tubeCount = 24;
        public DeviceParts()
        {
            this.Pdu = new Pdu();
            this.CTBox = new CTBox();
            this.IFBox = new IFBox();
            this.Gantry = new Gantry();
            this.AuxBoard = new AuxBoard();
            this.ExtBoard = new ExtBoard();
            this.TubeIntfs = new TubeIntf[] {
                new TubeIntf(),new TubeIntf(),new TubeIntf(),
                new TubeIntf(),new TubeIntf(),new TubeIntf(),
            };
            this.Table = new Table();
            this.Tubes = new Tube[tubeCount];
            for (int i = 0; i < tubeCount; i++)
            {
                this.Tubes[i] = new Tube() { Id = i + 1 };
            }
            this.Detector = new Detector();
        }
        /// <summary>
        /// 电源柜控制板
        /// </summary>
        private Pdu pdu;

        public Pdu Pdu
        {
            get { return pdu; }
            set { SetProperty(ref pdu, value); }
        }
        private CTBox ctBox;

        public CTBox CTBox
        {
            get { return ctBox; }
            set { SetProperty(ref ctBox, value); }
        }

        private IFBox ifbox;

        public IFBox IFBox
        {
            get { return ifbox; }
            set { SetProperty(ref ifbox, value); }
        }
        private Gantry gantry;

        public Gantry Gantry
        {
            get { return gantry; }
            set { SetProperty(ref gantry, value); }
        }
        private AuxBoard auxBoard;

        public AuxBoard AuxBoard
        {
            get { return auxBoard; }
            set { SetProperty(ref auxBoard, value); }
        }
        private ExtBoard extBoard;

        public ExtBoard ExtBoard
        {
            get { return extBoard; }
            set { SetProperty(ref extBoard, value); }
        }

        /// <summary>
        /// 高压接口板6个
        /// </summary>
        private TubeIntf[] tubeIntfs;

        public TubeIntf[] TubeIntfs
        {
            get { return tubeIntfs; }
            set { SetProperty(ref tubeIntfs, value); }
        }
        private Table table;

        public Table Table
        {
            get { return table; }
            set { SetProperty(ref table, value); }
        }
        private Tube[] tubes;

        public Tube[] Tubes
        {
            get { return tubes; }
            set { SetProperty(ref tubes, value); }
        }
        private Detector detector;

        public Detector Detector
        {
            get { return detector; }
            set { SetProperty(ref detector, value); }
        }


    }
}
