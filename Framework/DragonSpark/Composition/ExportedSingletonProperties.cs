using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Reflection;
using Defaults = DragonSpark.Activation.Location.Defaults;

namespace DragonSpark.Composition
{
	public sealed class ExportedSingletonProperties : SingletonProperties
	{
		public new static IParameterizedSource<Type, PropertyInfo> Default { get; } = new ExportedSingletonProperties().ToCache();
		ExportedSingletonProperties() : base( Defaults.SourcedSingleton.And( IsExportSpecification.Default.Project<SingletonRequest, PropertyInfo>( request => request.Candidate ) ) ) {}
	}
}