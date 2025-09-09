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
using FellowOakDicom;
using System.Drawing.Imaging;
using static System.Windows.Forms.AxHost;

namespace NV.CT.DicomUtility.BlackImageForSR
{
    public class BlackImageHelper
    {
        private static BlackImageHelper _blackImageHelper;

        public static BlackImageHelper Instance
        {
            get
            {
                if (null == _blackImageHelper)
                {
                    _blackImageHelper = new BlackImageHelper();
                }
                return _blackImageHelper;
            }
        }

        private const string ExamTitle = "Examination Information";
        private const string StudyIDPre = "StudyID:        ";
        private const string StudyTimePre = "StudyTime:      ";
        private const string TotalDLPPre = "TotalDLP:       ";

        private const string AcqDoseTitle = "Dose";
        private const string AcqIndexHeader = "#";
        private const string AcqSeriesDescriptionHeader_1 = "Series";
        private const string AcqSeriesDescriptionHeader_2 = "Description";
        private const string AcqScanModeHeader_1 = "Scan";
        private const string AcqScanModeHeader_2 = "Model";
        private const string AcqMAsHeader = "mAs";
        private const string AcqKVHeader = "kV";
        private const string AcqNXTHeader_1 = "N*T";
        private const string AcqNXTHeader_2 = "[mm]";
        private const string AcqCTDIHeader_1 = "CTDIvol";
        private const string AcqCTDIHeader_2 = "[mGy]";
        private const string AcqDLPHeader_1 = "DLP";
        private const string AcqDLPHeader_2 = "mGy▪cm";
        private const string AcqPhantomTypeHeader_1 = "Phantom";
        private const string AcqPhantomTypeHeader_2 = "Type[cm]";

        private const float AcqHeader_Index_PX = 18f;
        private const float AcqHeader_SeriesDescription_PX_1 = 65f;
        private const float AcqHeader_SeriesDescription_PX_2 = 54f;
        private const float AcqHeader_ScanMode_PX_1 = 154f - 100f;
        private const float AcqHeader_ScanMode_PX_2 = 151f - 100f;
        private const float AcqHeader_MAS_PX = 230f - 100f; //200f;
        private const float AcqHeader_KV_PX = 270f - 80f; //240f;
        //private const float AcqHeader_NT_PX_1 = 280f;
        //private const float AcqHeader_NT_PX_2 = 277f;
        private const float AcqHeader_CTDI_PX_1 = 310f - 65f; //325f;
        private const float AcqHeader_CTDI_PX_2 = 315f - 65f; //330f;
        private const float AcqHeader_DLP_PX_1 = 385f - 45f; //400f;
        private const float AcqHeader_DLP_PX_2 = 380f - 45f; //390f;
        private const float AcqHeader_PhantomType_PX_1 = 431f - 20f; //446f;
        private const float AcqHeader_PhantomType_PX_2 = 431f - 20f; //446f;

        private const float AcqInfo_Index_PX = 18f;
        private const float AcqInfo_SeriesDescription_PX = 43f;
        private const float AcqInfo_ScanMode_PX = 152f - 110f;
        private const float AcqInfo_MAS_PX = 228 - 105f; //198f;
        private const float AcqInfo_KV_PX = 268 - 80f; //238f;
        //private const float AcqInfo_NT_PX = 278f;
        private const float AcqInfo_CTDI_PX = 308f - 65f; //323f;
        private const float AcqInfo_DLP_PX = 373f - 45f; //388f;
        private const float AcqInfo_PhantomType_PX = 431f - 20f; //446f;

        private const int SeriesDescriptionLimit = 120;
        private const string Acq_MAS_Format = "F2";// "#.##";
        private const string Acq_KV_Format = "F2"; //"#.##";
        private const string Acq_NT_Format = "F2";// "#.##";
        private const string Acq_CTDI_Format = "F2";//"#.##"
        private const string Acq_DLP_Format = "F2";//"#.##"

        private const float AcqHeaderSingleLine_PY = 11f;
        private const float AcqHeaderDoubleLine_PY1 = 4f;
        private const float AcqHeaderDoubleLine_PY2 = 20f;

        private Font myInfoFont;
        public Font myTitleFont;


        private Brush myInfoBrush;
        private Brush myTitleBrush;

        private Brush myBackgroundBrush;
        private Brush myBorderBrush;
        private Pen myBorderPen;

        private float myMargin = 7.5f;

        private float myExamTitleHeight = 30f;
        private float myExamInfoPosX = 100;
        private float myExamBodyHeight = 70f;

        private float myAcqDoseTitleHeight = 30f;
        private float myAcqDoseHeaderHeight = 40f;

        private float myAcqDoseInfoHeight = 25f;

        private int myWidth, myHeight;

        private float myLineSpacing = 10f;


        private void InitDefault()
        {
            myInfoFont = new Font("Arial", 9);
            myInfoBrush = new SolidBrush(Color.White);

            myTitleFont = new Font("Arial", 10, FontStyle.Bold);
            //myTitleBrush = new SolidBrush(Color.Black);
            myTitleBrush = new SolidBrush(Color.White);

            myBackgroundBrush = new SolidBrush(Color.Black);
            //myBorderBrush = new SolidBrush(Color.White);
            myBorderBrush = new SolidBrush(Color.FromArgb(45,42,46));

            myBorderPen = new Pen(myBorderBrush);

            myWidth = 512;
            myHeight = 512;
        }
        private BlackImageHelper()
        {
            InitDefault();
        }

        public void UpdateBlackImageContent(BlackImageConentInfo info, DicomDataset ds)
        {
            Bitmap bmp = new Bitmap(myWidth, myHeight, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);

            //背景色
            g.FillRectangle(myBackgroundBrush, 0, 0, myWidth, myHeight);

            var currentPosition = myMargin;

            DrawExamRegion(g, info, currentPosition);

            currentPosition += myExamTitleHeight + myExamBodyHeight + myMargin;

            var tableStartY = currentPosition;

            DrawAcqHeaderRegion(g, info, currentPosition);
            g.DrawRectangle(myBorderPen, myMargin, currentPosition, myWidth - myMargin * 2, myHeight - currentPosition - myMargin);

            currentPosition += myAcqDoseTitleHeight + myAcqDoseHeaderHeight;

            foreach (var acqDoseInfo in info.AcqDoseInfos)
            {
                currentPosition += myMargin;
                DrawAcqDoseInfo(g, acqDoseInfo, currentPosition);
                currentPosition += myAcqDoseInfoHeight;
                g.DrawLine(myBorderPen, new PointF(myMargin, currentPosition), new PointF(myWidth - myMargin, currentPosition));
            }
            var tableEndY = currentPosition;
            DrawTableCells(g, info, tableStartY, tableEndY);

            g.Dispose();

            //bitmap起始点是从左下角开始的，Dicom图像起始点从左上角，需要做上下镜像
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            ds.AddOrUpdate(DicomTag.PixelData, GetGrayArrForBitmap(bmp));
        }

        private void DrawTableCells(Graphics g, BlackImageConentInfo info, float startY, float endY)
        {
            g.DrawLine(myBorderPen, new PointF(AcqInfo_ScanMode_PX - myMargin, startY), new PointF(AcqInfo_ScanMode_PX - myMargin, endY));
            g.DrawLine(myBorderPen, new PointF(AcqInfo_MAS_PX - myMargin, startY), new PointF(AcqInfo_MAS_PX - myMargin, endY));
            g.DrawLine(myBorderPen, new PointF(AcqInfo_KV_PX - myMargin, startY), new PointF(AcqInfo_KV_PX - myMargin, endY));
            g.DrawLine(myBorderPen, new PointF(AcqInfo_CTDI_PX - myMargin, startY), new PointF(AcqInfo_CTDI_PX - myMargin, endY));
            g.DrawLine(myBorderPen, new PointF(AcqInfo_DLP_PX - myMargin, startY), new PointF(AcqInfo_DLP_PX - myMargin, endY));
            g.DrawLine(myBorderPen, new PointF(AcqInfo_PhantomType_PX - myMargin, startY), new PointF(AcqInfo_PhantomType_PX - myMargin, endY));
        }

        private byte[] GetGrayArrForBitmap(Bitmap bmp)
        {

            byte[] grayByteArr = new byte[myWidth * myHeight * 2];

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Bmp);
                var bufferData = ms.GetBuffer();
                var dataStartIndex = bufferData.Length - myHeight * myWidth * 4;
                for (int i = 0; i < myWidth * myHeight; i++)
                {
                    grayByteArr[i * 2] = GetPixelGrayData(bufferData, dataStartIndex + i * 4);
                }
            }
            return grayByteArr;
        }

        private byte GetPixelGrayData(byte[] data, int index)
        {
            return (byte)(data[index + 1] * 0.299 + data[index + 2] * 0.587 + data[index + 3] * 0.114);
        }

        private void DrawExamRegion(Graphics g, BlackImageConentInfo info, float regionStartY)
        {
            var marginedWidth = myWidth - myMargin * 2;
            var infoFontSize = myInfoFont.Size;
            //ExamTitle
            var examTitleRect = new RectangleF(myMargin, myMargin, marginedWidth, myExamTitleHeight);
            g.FillRectangle(myBorderBrush, examTitleRect);
            var pos_ExamTitle = regionStartY + (myExamTitleHeight - myTitleFont.Size) / 2;
            g.DrawString(ExamTitle, myTitleFont, myTitleBrush, myMargin * 2, pos_ExamTitle);
            g.DrawRectangle(myBorderPen, myMargin, myMargin, marginedWidth, myExamTitleHeight);

            //ExanInfo
            regionStartY += myExamTitleHeight;
            var pos_StudyID = regionStartY + infoFontSize;
            var pos_StudyTime = regionStartY + infoFontSize * 2 + myLineSpacing;
            var pos_TotalDLP = regionStartY + infoFontSize * 3 + myLineSpacing * 2;
            g.DrawString(StudyIDPre, myInfoFont, myInfoBrush, myMargin * 2, pos_StudyID);
            g.DrawString(info.StudyID, myInfoFont, myInfoBrush, myExamInfoPosX, pos_StudyID);
            g.DrawString(StudyTimePre, myInfoFont, myInfoBrush, myMargin * 2, pos_StudyTime);
            g.DrawString(info.StudyTime.ToString(), myInfoFont, myInfoBrush, myExamInfoPosX, pos_StudyTime);
            g.DrawString(TotalDLPPre, myInfoFont, myInfoBrush, myMargin * 2, pos_TotalDLP);
            g.DrawString(info.TotalDLP.ToString("0.00"), myInfoFont, myInfoBrush, myExamInfoPosX, pos_TotalDLP);
            //ExamRect
            g.DrawRectangle(myBorderPen, myMargin, regionStartY, myWidth - myMargin * 2, myExamBodyHeight);
        }

        private void DrawAcqHeaderRegion(Graphics g, BlackImageConentInfo info, float regionStartY)
        {
            //AcqTitle
            var acqDoseTitleRect = new RectangleF(myMargin, regionStartY, myWidth - myMargin * 2, myAcqDoseTitleHeight);
            g.FillRectangle(myBorderBrush, acqDoseTitleRect);
            var pos_AcqDoseTitle = regionStartY + (myAcqDoseTitleHeight - myTitleFont.Size) / 2;
            g.DrawString(AcqDoseTitle, myTitleFont, myTitleBrush, myMargin * 2, pos_AcqDoseTitle);

            //Acq Header border
            regionStartY += myAcqDoseTitleHeight;
            g.DrawRectangle(myBorderPen, myMargin, regionStartY, myWidth - myMargin * 2, myAcqDoseHeaderHeight);

            //Acq Headers
            var singleLinePoxY = regionStartY + AcqHeaderSingleLine_PY;
            var doubleLinePoxY1 = regionStartY + AcqHeaderDoubleLine_PY1;
            var doubleLinePoxY2 = regionStartY + AcqHeaderDoubleLine_PY2;

            //Index
            g.DrawString(AcqIndexHeader, myInfoFont, myInfoBrush, AcqHeader_Index_PX, singleLinePoxY);
            //SeriesDesction
            //g.DrawString(AcqSeriesDescriptionHeader_1, myInfoFont, myInfoBrush, AcqHeader_SeriesDescription_PX_1, doubleLinePoxY1);
            //g.DrawString(AcqSeriesDescriptionHeader_2, myInfoFont, myInfoBrush, AcqHeader_SeriesDescription_PX_2, doubleLinePoxY2);
            //ScanMode
            g.DrawString(AcqScanModeHeader_1, myInfoFont, myInfoBrush, AcqHeader_ScanMode_PX_1, doubleLinePoxY1);
            g.DrawString(AcqScanModeHeader_2, myInfoFont, myInfoBrush, AcqHeader_ScanMode_PX_2, doubleLinePoxY2);
            //mAs
            g.DrawString(AcqMAsHeader, myInfoFont, myInfoBrush, AcqHeader_MAS_PX, singleLinePoxY);
            //kV
            g.DrawString(AcqKVHeader, myInfoFont, myInfoBrush, AcqHeader_KV_PX, singleLinePoxY);
            //N*T
            //g.DrawString(AcqNXTHeader_1, myInfoFont, myInfoBrush, AcqHeader_NT_PX_1, doubleLinePoxY1);
            //g.DrawString(AcqNXTHeader_2, myInfoFont, myInfoBrush, AcqHeader_NT_PX_2, doubleLinePoxY2);
            //CTDI
            g.DrawString(AcqCTDIHeader_1, myInfoFont, myInfoBrush, AcqHeader_CTDI_PX_1, doubleLinePoxY1);
            g.DrawString(AcqCTDIHeader_2, myInfoFont, myInfoBrush, AcqHeader_CTDI_PX_2, doubleLinePoxY2);
            //DLP
            g.DrawString(AcqDLPHeader_1, myInfoFont, myInfoBrush, AcqHeader_DLP_PX_1, doubleLinePoxY1);
            g.DrawString(AcqDLPHeader_2, myInfoFont, myInfoBrush, AcqHeader_DLP_PX_2, doubleLinePoxY2);
            //PhantomType
            g.DrawString(AcqPhantomTypeHeader_1, myInfoFont, myInfoBrush, AcqHeader_PhantomType_PX_1, doubleLinePoxY1);
            g.DrawString(AcqPhantomTypeHeader_2, myInfoFont, myInfoBrush, AcqHeader_PhantomType_PX_2, doubleLinePoxY2);
        }

        private void DrawAcqDoseInfo(Graphics g, ACQDoseInfoForBlackImage info, float regionStartY)
        {
            //Index
            g.DrawString(info.Index.ToString(), myInfoFont, myInfoBrush, AcqInfo_Index_PX, regionStartY);
            //SeriesDescription
            //g.DrawString(GetLimitedDescription(g, info.SeriesDescription), myInfoFont, myInfoBrush, AcqInfo_SeriesDescription_PX, regionStartY);
            //ScanMode
            g.DrawString(info.ScanMode, myInfoFont, myInfoBrush, AcqInfo_ScanMode_PX, regionStartY);
            //mAs
            g.DrawString(info.MAs.ToString(Acq_MAS_Format), myInfoFont, myInfoBrush, AcqInfo_MAS_PX, regionStartY);
            //kV
            g.DrawString(info.KV.ToString(Acq_KV_Format), myInfoFont, myInfoBrush, AcqInfo_KV_PX, regionStartY);
            //N*T
            //g.DrawString($"{info.Cycles}*{info.RotateTime.ToString(Acq_NT_Format)}", myInfoFont, myInfoBrush, AcqInfo_NT_PX, regionStartY);
            //CTDI
            g.DrawString(info.CTDIvol.ToString(Acq_CTDI_Format), myInfoFont, myInfoBrush, AcqInfo_CTDI_PX, regionStartY);
            //DLP
            g.DrawString(info.DLP.ToString(Acq_DLP_Format), myInfoFont, myInfoBrush, AcqInfo_DLP_PX, regionStartY);
            //PhantomType
            g.DrawString(info.PhantomType, myInfoFont, myInfoBrush, AcqInfo_PhantomType_PX, regionStartY);
        }

        private string GetLimitedDescription(Graphics g, string description)
        {
            if (g.MeasureString(description, myInfoFont).Width < SeriesDescriptionLimit)
            {
                return description;
            }
            var limitedDescription = description;
            int subSize, length;
            do
            {
                length = limitedDescription.Length - 1;
                subSize = length / 2;
                limitedDescription = description.Substring(0, subSize) + description.Substring(description.Length - subSize, subSize);
            }
            while (g.MeasureString(limitedDescription, myInfoFont).Width > SeriesDescriptionLimit - 15);

            return description.Substring(0, subSize) + "..." + description.Substring(description.Length - subSize, subSize);
        }
    }
}
