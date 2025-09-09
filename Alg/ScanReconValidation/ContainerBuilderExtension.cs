using Autofac;

namespace NV.CT.Alg.ScanReconValidation;

public static class ContainerBuilderExtension
{
	public static void AddAlgRawDataValidationModule(this ContainerBuilder builder)
	{
		builder.RegisterModule<AlgRawDataValidationModule>();
	}
}

