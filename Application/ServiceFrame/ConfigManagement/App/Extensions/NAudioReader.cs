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

public class NAudioReader : ISampleProvider
{
    /// <summary>
    /// Defines the source.
    /// </summary>
    private readonly ISampleProvider source;

    /// <summary>
    /// Gets the WaveFormat.
    /// </summary>
    public WaveFormat WaveFormat => source.WaveFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="NAudioReader"/> class.
    /// </summary>
    /// <param name="source">The source<see cref="ISampleProvider"/>.</param>
    /// <param name="fftLength">The fftLength<see cref="int"/>.</param>
    public NAudioReader(ISampleProvider source, int fftLength = 1024)
    {
        if (!IsPowerOfTwo(fftLength))
        {
            throw new ArgumentException("FFT Length must be a power of two");
        }
        this.source = source;
    }

    /// <summary>
    /// The Read.
    /// </summary>
    /// <param name="buffer">The buffer<see cref="float[]"/>.</param>
    /// <param name="offset">The offset<see cref="int"/>.</param>
    /// <param name="count">The count<see cref="int"/>.</param>
    /// <returns>The <see cref="int"/>.</returns>
    public int Read(float[] buffer, int offset, int count)
    {
        var samplesRead = source.Read(buffer, offset, count);
        return samplesRead;
    }

    /// <summary>
    /// The IsPowerOfTwo.
    /// </summary>
    /// <param name="x">The x<see cref="int"/>.</param>
    /// <returns>The <see cref="bool"/>.</returns>
    internal static bool IsPowerOfTwo(int x)
    {
        return (x & (x - 1)) == 0;
    }
}
