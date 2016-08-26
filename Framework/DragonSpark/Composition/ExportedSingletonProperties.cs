using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class ExportedSingletonProperties : SingletonProperties
	{
		public new static IParameterizedSource<Type, PropertyInfo> Default { get; } = new ExportedSingletonProperties().ToCache();
		ExportedSingletonProperties() : base( SingletonSpecification.Default.And( IsExportSpecification.Default.Project<SingletonRequest, PropertyInfo>( request => request.Candidate ) ) ) {}
	}
}