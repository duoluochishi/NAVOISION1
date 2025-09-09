namespace NV.CT.JobViewer.Models;

public class ImportTaskInfo : JobTaskBaseInfo
{
    private string _sourcePath = string.Empty;
    public string SourcePath
    {
        get
        {
            return this._sourcePath;
        }
        set
        {
            this.SetProperty(ref this._sourcePath, value);
        }
    }
}
