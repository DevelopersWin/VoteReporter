using DragonSpark.Testing.Framework.Runtime;
using DragonSpark.TypeSystem;
using DragonSpark.Windows;
using System;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	public sealed class FrameworkTypesAttribute : TypeProviderAttributeBase
	{
		public FrameworkTypesAttribute() : base( typeof(InitializationCommand), typeof(Configure), typeof(MetadataCommand) ) {}
	}

	public sealed class FormatterTypesAttribute : TypeProviderAttributeBase
	{
		public static Type[] Types { get; } = { typeof(TypeFormatter), typeof(MethodFormatter), typeof(TaskContextFormatter), typeof(ObjectFormatter) };

		public FormatterTypesAttribute() : base( Types ) {}
	}
}