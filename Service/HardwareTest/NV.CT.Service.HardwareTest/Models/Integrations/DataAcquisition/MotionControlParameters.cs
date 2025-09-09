using NV.CT.Service.HardwareTest.Categories;

namespace NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition
{
    public class MotionControlParameters
    {
        public MotionControlParameters()
        {
            this.InitializeProperties();
        }

        #region Initialize

        private void InitializeProperties()
        {
            this.GantryParameters = new();
            this.CollimatorParameters = new();
            this.TableParameters = new();
        }

        #endregion

        #region Properties

        public GantryBaseCategory GantryParameters { get; set; } = null!;
        public CollimatorBaseCategory CollimatorParameters { get; set; } = null!;
        public TableBaseCategory TableParameters { get; set; } = null!;

        #endregion

    }
}
