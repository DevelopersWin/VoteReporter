using DragonSpark.Application.Setup;
using DragonSpark.Testing.Framework.Runtime;
using DragonSpark.TypeSystem;
using DragonSpark.Windows;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	public class FrameworkTypesAttribute : TypeProviderAttributeBase
	{
		public FrameworkTypesAttribute() : base( typeof(InitializationCommand), typeof(Configure), typeof(EnableServicesCommand), typeof(MetadataCommand), typeof(MethodFormatter), typeof(TaskContextFormatter) ) {}
	}
}