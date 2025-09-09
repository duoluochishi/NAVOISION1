namespace NV.CT.ErrorCodes.Test;

public class Program
{
	static void Main(string[] args)
	{
		var code = ErrorCodeHelper.GetErrorCode("RDH000000008");

		Console.Read();
	}
}