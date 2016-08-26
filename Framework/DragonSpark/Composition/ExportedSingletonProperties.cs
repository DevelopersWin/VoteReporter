using System.Reflection;
using DragonSpark.Activation.Location;
using DragonSpark.Specifications;

namespace DragonSpark.Composition
{
	public sealed class ExportedSingletonProperties : SingletonProperties
	{
		public new static ExportedSingletonProperties Default { get; } = new ExportedSingletonProperties();
		ExportedSingletonProperties() : base( SingletonSpecification.Default.And( IsExportSpecification.Default.Project<SingletonRequest, PropertyInfo>( request => request.Candidate ) ) ) {}
	}
}