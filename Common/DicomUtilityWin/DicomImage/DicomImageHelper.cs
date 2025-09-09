//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.LUT;
using FellowOakDicom.Imaging.Reconstruction;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NV.CT.DicomUtility.DicomImage;

public class DicomImageHelper
{
	private static DicomImageHelper _dicomImageHelper;

	public static DicomImageHelper Instance
	{
		get
		{
			if (null == _dicomImageHelper)
			{
				_dicomImageHelper = new DicomImageHelper();
			}
			return _dicomImageHelper;
		}
	}

	public WriteableBitmap GenerateThumbImage(string path, int width, int height)
	{
		var startTime = DateTime.Now;
		var imageData = new ImageData(path);
		var pixels = imageData.Pixels;
		var oriImageWidth = imageData.Geometry.FrameSize.X;
		var oriImageHeight = imageData.Geometry.FrameSize.Y;

		var (dImgW, dImgH) = GetFitWidthHeight(width, height, oriImageWidth, oriImageHeight);


		//获取GrayscaleRenderOptions,并以此生成ModalityLUT
		//todo: 先默认 frame = 0;
		var gro = GrayscaleRenderOptions.FromDataset(imageData.Dataset, 0);
		var modalityLUT = new ModalityRescaleLUT(gro);

		var renderData = new int[pixels.Width * pixels.Height];
		pixels.Render(modalityLUT, renderData);

		//resize

		int[] resizedRenderData;
		unsafe
		{
			fixed (int* pixelPtr = renderData)
			{
				resizedRenderData = Resize(pixelPtr, pixels.Width, pixels.Height, dImgW, dImgH, Interpolation.NearestNeighbor);
			}
		}

		//VOI过程
		var voiLUT = new VOILinearLUT(gro);
		var byteData = new byte[dImgW * dImgH];

		Parallel.For(0, dImgH, y =>
		{
			for (int i = dImgW * y, e = i + dImgW; i < e; i++)
			{
				byteData[i] = (byte)voiLUT[resizedRenderData[i]];
			}
		});

		Trace.WriteLine($"voi: {(DateTime.Now - startTime).TotalMilliseconds}");

		WriteableBitmap wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);

		wb.WritePixels(new Int32Rect((width - dImgW) / 2, (height - dImgH) / 2, dImgW, dImgH), byteData, dImgW, 0);

		return wb;
	}
	public static Bitmap GenerateBitmapImage(string path)
	{
		var startTime = DateTime.Now;
		var imageData = new ImageData(path);
		var pixels = imageData.Pixels;

		//获取GrayscaleRenderOptions,并以此生成ModalityLUT
		//todo: 先默认 frame = 0;
		var gro = GrayscaleRenderOptions.FromDataset(imageData.Dataset, 0);
		var modalityLUT = new ModalityRescaleLUT(gro);

		var renderData = new int[pixels.Width * pixels.Height];
		pixels.Render(modalityLUT, renderData);

		Trace.WriteLine($"lut: {(DateTime.Now - startTime).TotalMilliseconds}");

		var voiLUT = new VOILinearLUT(gro);
		var byteData = new byte[pixels.Width * pixels.Height];


		Parallel.For(0, pixels.Height, y =>
		{
			for (int i = pixels.Width * y, e = i + pixels.Width; i < e; i++)
			{
				byteData[i] = (byte)voiLUT[renderData[i]];
			}
		});

		var bmp = new Bitmap(pixels.Width, pixels.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
		var rect = new Rectangle(0, 0, pixels.Width, pixels.Height);
		var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

		unsafe
		{
			byte* dst = (byte*)bmpData.Scan0;
			for (int y = 0; y < pixels.Height; y++)
			{
				for (int x = 0; x < pixels.Width; x++)
				{
					var v = byteData[y * pixels.Width + x];
					dst[0] = dst[1] = dst[2] = v;
					dst += 3;
				}
				dst += bmpData.Stride - pixels.Width * 3; // 跳过 padding
			}
		}

		bmp.UnlockBits(bmpData);
		return bmp;
	}

	private unsafe static int[] Resize(int* pixels, int widthSource, int heightSource, int width, int height, Interpolation interpolation)
	{
		int[] array = new int[width * height];
		float num = (float)widthSource / (float)width;
		float num2 = (float)heightSource / (float)height;
		switch (interpolation)
		{
			case Interpolation.NearestNeighbor:
				{
					int num23 = 0;
					for (int k = 0; k < height; k++)
					{
						for (int l = 0; l < width; l++)
						{
							float num4 = (float)l * num;
							float num24 = (float)k * num2;
							int num6 = (int)num4;
							int num7 = (int)num24;
							array[num23++] = pixels[num7 * widthSource + num6];
						}
					}

					break;
				}
			case Interpolation.Bilinear:
				{
					int num3 = 0;
					for (int i = 0; i < height; i++)
					{
						for (int j = 0; j < width; j++)
						{
							float num4 = (float)j * num;
							float num5 = (float)i * num2;
							int num6 = (int)num4;
							int num7 = (int)num5;
							float num8 = num4 - (float)num6;
							float num9 = num5 - (float)num7;
							float num10 = 1f - num8;
							float num11 = 1f - num9;
							int num12 = num6 + 1;
							if (num12 >= widthSource)
							{
								num12 = num6;
							}

							int num13 = num7 + 1;
							if (num13 >= heightSource)
							{
								num13 = num7;
							}

							int num14 = pixels[num7 * widthSource + num6];
							byte b = (byte)(num14 >> 24);
							byte b2 = (byte)(num14 >> 16);
							byte b3 = (byte)(num14 >> 8);
							byte b4 = (byte)num14;
							int num15 = pixels[num7 * widthSource + num12];
							byte b5 = (byte)(num15 >> 24);
							byte b6 = (byte)(num15 >> 16);
							byte b7 = (byte)(num15 >> 8);
							byte b8 = (byte)num15;
							int num16 = pixels[num13 * widthSource + num6];
							byte b9 = (byte)(num16 >> 24);
							byte b10 = (byte)(num16 >> 16);
							byte b11 = (byte)(num16 >> 8);
							byte b12 = (byte)num16;
							int num17 = pixels[num13 * widthSource + num12];
							byte b13 = (byte)(num17 >> 24);
							byte b14 = (byte)(num17 >> 16);
							byte b15 = (byte)(num17 >> 8);
							byte b16 = (byte)num17;
							float num18 = num10 * (float)(int)b + num8 * (float)(int)b5;
							float num19 = num10 * (float)(int)b9 + num8 * (float)(int)b13;
							byte b17 = (byte)(num11 * num18 + num9 * num19);
							num18 = num10 * (float)(int)b2 + num8 * (float)(int)b6;
							num19 = num10 * (float)(int)b10 + num8 * (float)(int)b14;
							float num20 = num11 * num18 + num9 * num19;
							num18 = num10 * (float)(int)b3 + num8 * (float)(int)b7;
							num19 = num10 * (float)(int)b11 + num8 * (float)(int)b15;
							float num21 = num11 * num18 + num9 * num19;
							num18 = num10 * (float)(int)b4 + num8 * (float)(int)b8;
							num19 = num10 * (float)(int)b12 + num8 * (float)(int)b16;
							float num22 = num11 * num18 + num9 * num19;
							byte b18 = (byte)num20;
							byte b19 = (byte)num21;
							byte b20 = (byte)num22;
							array[num3++] = (b17 << 24) | (b18 << 16) | (b19 << 8) | b20;
						}
					}

					break;
				}
		}

		return array;
	}

	public WriteableBitmap BitmapToWriteableBitmap(Bitmap src)
	{
		var wb = CreateCompatibleWriteableBitmap(src);
		System.Drawing.Imaging.PixelFormat format = src.PixelFormat;
		if (wb is null)
		{
			wb = new WriteableBitmap(src.Width, src.Height, 0, 0, PixelFormats.Bgra32, null);
			format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
		}
		BitmapCopyToWriteableBitmap(src, wb, new Rectangle(0, 0, src.Width, src.Height), 0, 0, format);
		return wb;
	}

	private WriteableBitmap CreateCompatibleWriteableBitmap(Bitmap src)
	{
		PixelFormat format;
		switch (src.PixelFormat)
		{
			case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
				format = PixelFormats.Bgr555;
				break;
			case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
				format = PixelFormats.Bgr565;
				break;
			case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
				format = PixelFormats.Bgr24;
				break;
			case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
				format = PixelFormats.Bgr32;
				break;
			case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
				format = PixelFormats.Pbgra32;
				break;
			case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
				format = PixelFormats.Bgra32;
				break;
			default:
				return null;
		}
		return new WriteableBitmap(src.Width, src.Height, 0, 0, format, null);
	}

	private void BitmapCopyToWriteableBitmap(Bitmap src, WriteableBitmap dst, Rectangle srcRect, int destinationX, int destinationY, System.Drawing.Imaging.PixelFormat srcPixelFormat)
	{
		var data = src.LockBits(new Rectangle(new System.Drawing.Point(0, 0), src.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, srcPixelFormat);
		dst.WritePixels(new Int32Rect(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height), data.Scan0, data.Height * data.Stride, data.Stride, destinationX, destinationY);
		src.UnlockBits(data);
	}

	/// <summary>
	/// 获取一个dicom文件的所有tag
	/// </summary>
	public List<DicomDetail> GetDicomDetails(string dicomFile)
	{
		var list = new List<DicomDetail>();

		var file = DicomFile.Open(dicomFile);
		if (file is null)
			return list;

		try
		{
			var ds = file.Dataset;
			foreach (var item in ds)
			{
				var dicomDetail = new DicomDetail();
				dicomDetail.TagID = item.Tag.ToString();

				var dicEntry = item.Tag.DictionaryEntry;
				dicomDetail.TagDescription = dicEntry.Name;
				dicomDetail.TagVR = dicEntry.ValueRepresentations.FirstOrDefault()?.ToString() ?? string.Empty;
				dicomDetail.TagVM = dicEntry.ValueMultiplicity.Multiplicity.ToString();

				var isSuccess = ds.TryGetValues(item.Tag, out string[] vals);
				if (isSuccess)
				{
					dicomDetail.TagValue = string.Join(@"\", vals);
				}

				dicomDetail.TagLength = dicomDetail.TagValue.Length.ToString();

				list.Add(dicomDetail);
			}

			return list;
		}
		catch (Exception)
		{
			return list;
		}
	}

	private static (int, int) GetFitWidthHeight(int dstW, int dstH, int oriW, int oriH)
	{
		var oriWHscale = ((double)oriW) / oriH;
		var newW = oriWHscale * dstH;

		if (newW < dstW)
		{
			return ((int)newW, dstH);
		}
		else
		{
			return (dstW, (int)(dstH / oriWHscale));
		}
	}
}

public enum Interpolation
{
	NearestNeighbor,
	Bilinear
}