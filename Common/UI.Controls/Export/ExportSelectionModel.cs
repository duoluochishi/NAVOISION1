using NV.CT.CTS.Enums;

namespace NV.CT.UI.Controls.Export
{
    public class ExportSelectionModel
    {
        public bool IsAnonymouse { get; set; } = false;

        public bool IsExportedToDICOM { get; set; } = false;

        public bool IsExportedToImage { get; set; } = false;

        public string OutputVirtualPath { get; set; } = string.Empty;

        public string OutputFolder { get; set; } = string.Empty;

        public bool IsBurnToCDROM { get; set; } = false;

        public bool IsAddViewer { get; set; } = false;

        public string DicomTransferSyntax { get; set; }

        public FileExtensionType? PictureType { get; set; } = null;
    }
}
