using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DicomImageViewer
{
    public class ScreenshotParameters
    {
        public Bitmap? ScreenShotData { get; set; }

        public string? StudyInstanceUID { get; set; }

        public string? SeriesInstanceUID { get; set; }

        public string? SeriesDescription { get; set; }

        public int? InstanceNumber { get; set; }

        public string? Dir { get; set; }

        public string? SeriesNumber { get; set; }

        public ScreenshotParameters(Bitmap bitMap,string studyInstanceUID,string seriesInstanceUID, string seriesDescription,int instanceNumber,string dir,string seriesNumber)
        {
            ScreenShotData= bitMap;
            StudyInstanceUID= studyInstanceUID;
            SeriesInstanceUID= seriesInstanceUID;
            SeriesDescription= seriesDescription;
            InstanceNumber= instanceNumber;
            Dir= dir;
            SeriesNumber= seriesNumber;
        }
    }
}
