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
using NV.CT.DicomUtility.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;

namespace NV.CT.DicomUtility.DicomModule
{
    public class OverlayModule : IDicomDatasetUpdater
    {

        public ushort OverlayRows { get; set; }
        public ushort OverlayColumns { get; set; }

        public OverlayTypeCS OverlayTypeCS { get; set; }

        public short[] OverlayOrigin { get; set; } = new short[] { 1, 1 };

        public ushort OverlayBitsAllocated { get; set; }
        public ushort OverlayBitPosition { get; set; }


        public void Update(DicomDataset ds)
        {
            ds.AddOrUpdate(DicomTag.OverlayRows, OverlayRows);
            ds.AddOrUpdate(DicomTag.OverlayColumns, OverlayColumns);
            ds.AddOrUpdate(DicomTag.OverlayType, OverlayTypeCS);
            ds.AddOrUpdate(DicomTag.OverlayBitsAllocated, OverlayBitsAllocated);
            ds.AddOrUpdate(DicomTag.OverlayBitPosition, OverlayBitPosition);
            ds.AddOrUpdate(DicomTag.OverlayOrigin, @$"{OverlayOrigin[0]}\{OverlayOrigin[1]}");
        }
        public void Read(DicomDataset ds)
        {
            //OverlayRows = ushort.Parse(ds.GetString(DicomTag.OverlayRows));
            //OverlayColumns = ushort.Parse(ds.GetString(DicomTag.OverlayColumns));
            //OverlayTypeCS = Enum.Parse<OverlayTypeCS>(ds.GetString(DicomTag.OverlayType));
            //OverlayBitsAllocated = ushort.Parse(ds.GetString(DicomTag.OverlayBitsAllocated));
            //OverlayBitPosition = ushort.Parse(ds.GetString(DicomTag.OverlayBitPosition));

        }
    }
}
