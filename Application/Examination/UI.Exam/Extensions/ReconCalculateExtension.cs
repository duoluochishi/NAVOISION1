//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2025/2/19 9:28:29           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using System.Text;

namespace NV.CT.UI.Exam.Extensions;

public class ReconCalculateExtension
{
	private static int baseAccurate = 165;                      //0.165
	private static int tripleBaseAccurate = 330;                //0.33
	private static int fourfoldBaseAccurate = 660;              //0.66
	private static int maxFov = 506880;                         //506.88
	private static int minFov = 50000;                          //50
	private static int maxFovMatrix = 1000;                     //fov/matrix:max:1.0
	private static int minFovMatrix = 100;                      //fov/matrix:min:0.1

	public static bool ReconParamCalculate(ReconModel reconModel, out string meaasge)
	{
		bool flag = true;
		//根据2025年4月16日的往来邮件，确定暂时先不不加这些参数判断了
		//StringBuilder stringBuilder = new StringBuilder();
		//flag &= ReconFovMatrixValidation(reconModel, ref stringBuilder);
		//flag &= ReconPreBinningValidation(reconModel, ref stringBuilder);
		//flag &= ReconSliceThicknessIncrementValidation(reconModel, ref stringBuilder);
		//meaasge = stringBuilder.ToString();
		meaasge = string.Empty;
		return flag;
	}

	public static bool ReconFovMatrixValidation(ReconModel reconModel, ref StringBuilder stringBuilder)
	{
		bool flag = true;
		//支持FOV在50~506.88的范围内的连续变化过程中进行重建
		if (reconModel.FOVLengthHorizontal < minFov || reconModel.FOVLengthHorizontal > maxFov)
		{
			flag = false;
			stringBuilder.Append("Supports reconstruction during continuous changes in FOV in the range of 50~506.88!");
		}
		// FOV/Matrix比值限制在0.1~1(mm)范围内，都支持进行重建
		var fovmatrix = reconModel.FOVLengthHorizontal / reconModel.ImageMatrixHorizontal;
		if (fovmatrix < minFovMatrix || fovmatrix > maxFovMatrix)
		{
			flag = false;
			stringBuilder.Append("FOV/Matrix ratio is limited to 0.1~1(mm), and reconstruction is supported!");
		}
		return flag;
	}

	private static bool ReconPreBinningValidation(ReconModel reconModel, ref StringBuilder stringBuilder)
	{
		bool flag = true;
		var preBinning = reconModel.PreBinning.ToString().Split("_");
		if (preBinning.Length > 1 && preBinning[1].IndexOf('x') > 0)
		{
			var binning = preBinning[1].Split("x");
			if (binning.Length > 1)
			{
				flag &= ReconPreBinningFirstNValidation(reconModel, binning[0], ref stringBuilder);
				flag &= ReconPreBinningLastNValidation(reconModel, binning[1], ref stringBuilder);
				flag &= ReconIncrementValidation(reconModel, binning[1], ref stringBuilder);
			}
		}
		return flag;
	}

	private static bool ReconPreBinningFirstNValidation(ReconModel reconModel, string firstN, ref StringBuilder stringBuilder)
	{
		bool flag = true;
		if (firstN.Equals("8"))
		{
			flag = false;
		}
		else
		{
			flag = ReconPreBinningFirstNValidation(reconModel, firstN);
		}
		if (!flag)
		{
			stringBuilder.Append("Matrix and FOV mismatch PreBinning!");
		}
		return flag;
	}

	private static bool ReconPreBinningFirstNValidation(ReconModel reconModel, string firstN)
	{
		bool flag = true;
		switch (reconModel.ImageMatrixHorizontal)
		{
			case 512:
				flag = ReconPreBinningFirstN512Validation(reconModel, firstN);
				break;
			case 768:
				flag = ReconPreBinningFirstN768Validation(reconModel, firstN);
				break;
			case 1024:
				flag = ReconPreBinningFirstN1024Validation(reconModel, firstN);
				break;
			case 1536:
				flag = ReconPreBinningFirstN1536Validation(reconModel, firstN);
				break;
			case 2048:
			case 2560:
			case 3072:
				flag = ReconPreBinningFirstNGreaterEqualTo2048Validation(firstN);
				break;
			default:
				flag = false;
				break;
		}
		return flag;
	}

	private static bool ReconPreBinningFirstN512Validation(ReconModel reconModel, string firstN)
	{
		bool flag = true;
		//88480==88.48*1000,506880=506.88*1000
		if (firstN.EndsWith("2") && (reconModel.FOVLengthHorizontal < 84480 || reconModel.FOVLengthHorizontal > maxFov))
		{
			flag = false;
		}
		//168960==168.96*1000,506880=506.88*1000
		else if (firstN.EndsWith("4") && (reconModel.FOVLengthHorizontal < 168960 || reconModel.FOVLengthHorizontal > 337920))
		{
			flag = false;
		}
		return flag;
	}

	private static bool ReconPreBinningFirstN768Validation(ReconModel reconModel, string firstN)
	{
		bool flag = true;
		//126720==126.72*1000,506880=506.88*1000
		if (firstN.EndsWith("2") && (reconModel.FOVLengthHorizontal < 126720 || reconModel.FOVLengthHorizontal > maxFov))
		{
			flag = false;
		}
		//253440==253.44*1000,506880=506.88*1000
		else if (firstN.EndsWith("4") && (reconModel.FOVLengthHorizontal < 253440 || reconModel.FOVLengthHorizontal > maxFov))
		{
			flag = false;
		}
		return flag;
	}

	private static bool ReconPreBinningFirstN1024Validation(ReconModel reconModel, string firstN)
	{
		bool flag = true;
		//168960==168.96*1000,506880=506.88*1000
		if (firstN.EndsWith("2") && (reconModel.FOVLengthHorizontal < 168960 || reconModel.FOVLengthHorizontal > 337920))
		{
			flag = false;
		}
		else if (firstN.EndsWith("4") || firstN.EndsWith("8"))
		{
			flag = false;
		}
		return flag;
	}

	private static bool ReconPreBinningFirstN1536Validation(ReconModel reconModel, string firstN)
	{
		bool flag = true;
		//253440==253.44*1000,506880=506.88*1000
		if (firstN.EndsWith("2") && (reconModel.FOVLengthHorizontal < 253440 || reconModel.FOVLengthHorizontal > maxFov))
		{
			flag = false;
		}
		else if (firstN.EndsWith("4") || firstN.EndsWith("8"))
		{
			flag = false;
		}
		return flag;
	}

	private static bool ReconPreBinningFirstNGreaterEqualTo2048Validation(string firstN)
	{
		bool flag = true;
		if (!firstN.EndsWith("1"))
		{
			flag = false;
		}
		return flag;
	}

	public static bool ReconPreBinningLastNValidation(ReconModel reconModel, string lastN, ref StringBuilder stringBuilder)
	{
		bool flag = true;
		switch (lastN)
		{
			case "1":
				if ((reconModel.SliceThickness - reconModel.SliceThickness % baseAccurate) % baseAccurate != 0)
				{
					flag = false;
					stringBuilder.Append("SliceThickness should be a multiple of 0.165!");
				}
				break;
			case "2":
				if ((reconModel.SliceThickness - reconModel.SliceThickness % baseAccurate) % tripleBaseAccurate != 0)
				{
					flag = false;
					stringBuilder.Append("SliceThickness should be a multiple of 0.33!");
				}
				break;
			case "4":
				if ((reconModel.SliceThickness - reconModel.SliceThickness % baseAccurate) % fourfoldBaseAccurate != 0)
				{
					flag = false;
					stringBuilder.Append("SliceThickness should be a multiple of 0.66!");
				}
				break;
			default:
				flag = false;
				stringBuilder.Append("Unsupported PreBinning Options!");
				break;
		}
		return flag;
	}

	private static bool ReconIncrementValidation(ReconModel reconModel, string lastN, ref StringBuilder stringBuilder)
	{
		bool flag = true;
		switch (lastN)
		{
			case "1":
				if ((reconModel.ImageIncrement - reconModel.ImageIncrement % baseAccurate) % baseAccurate != 0)
				{
					flag = false;
					stringBuilder.Append("ImageIncrement should be a multiple of 0.165!");
				}
				break;
			case "2":
				if ((reconModel.ImageIncrement - reconModel.ImageIncrement % baseAccurate) % tripleBaseAccurate != 0)
				{
					flag = false;
					stringBuilder.Append("ImageIncrement should be a multiple of 0.33!");
				}
				break;
			case "4":
				if ((reconModel.ImageIncrement - reconModel.ImageIncrement % baseAccurate) % fourfoldBaseAccurate != 0)
				{
					flag = false;
					stringBuilder.Append("ImageIncrement should be a multiple of 0.66!");
				}
				break;
			default:
				flag = false;
				stringBuilder.Append("Unsupported PreBinning Options!");
				break;
		}
		return flag;
	}

	public static bool ReconSliceThicknessIncrementValidation(ReconModel reconModel, ref StringBuilder stringBuilder)
	{
		bool flag = true;
		//slice thickness = image increment
		//slice thickness = 2 * image increment		
		if (!(Math.Equals(reconModel.SliceThickness, reconModel.ImageIncrement) || Math.Equals(reconModel.SliceThickness, reconModel.ImageIncrement * 2)))
		{
			flag = false;
			stringBuilder.Append("SliceThickness does not match the ImageIncrement parameter!");
		}
		return flag;
	}
}