//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NAudio.Wave;
namespace NV.CT.ConfigManagement.Extensions;

public class NAudioRecorder : ISpeechRecorder
{
    #region Fields

    /// <summary>
    /// 录音文件名
    /// </summary>
    public WaveFileWriter waveFile = null;

    /// <summary>
    /// 录音使用类
    /// </summary>
    public WaveIn waveSource = null;

    /// <summary>
    /// 录音文件名
    /// </summary>
    private string fileName = string.Empty;

    #endregion Fields

    #region Events

    /// <summary>
    /// Defines the DataAvailable.
    /// </summary>
    public event Action<double> DataAvailable;

    #endregion Events

    #region Methods

    /// <summary>
    /// 录音结束后保存的文件路径.
    /// </summary>
    /// <param name="fileName">保存wav文件的路径名.</param>
    public void SetFileName(string fileName)
    {
        this.fileName = fileName;
    }

    /// <summary>
    /// 开始录音.
    /// </summary>
    public void StartRec()
    {
        waveSource = new WaveIn();
        waveSource.WaveFormat = new WaveFormat(16000, 16, 1); // 16bit,16KHz,Mono的录音格式
        waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
        waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);
        waveFile = new WaveFileWriter(fileName, waveSource.WaveFormat);
        waveSource.StartRecording();
    }

    /// <summary>
    /// 停止录音.
    /// </summary>
    public void StopRec()
    {
        if (waveSource != null)
        {
            waveSource.StopRecording();
        }
    }

    /// <summary>
    /// 开始录音回调函数.
    /// </summary>
    /// <param name="sender">.</param>
    /// <param name="e">.</param>
    private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
    {
        if (waveFile != null)
        {
            waveFile.Write(e.Buffer, 0, e.BytesRecorded);
            waveFile.Flush();
            double secondsRecorded = waveFile.Length / waveFile.WaveFormat.AverageBytesPerSecond;
            DataAvailable?.Invoke(secondsRecorded);
        }
    }

    /// <summary>
    /// 录音结束回调函数.
    /// </summary>
    /// <param name="sender">.</param>
    /// <param name="e">.</param>
    private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
    {
        if (waveSource != null)
        {
            waveSource.Dispose();
            waveSource = null;
        }
        if (waveFile != null)
        {
            waveFile.Dispose();
            waveFile = null;
        }
    }

    #endregion Methods
}