using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	// ReSharper disable once UnusedTypeParameter
	public sealed class SpecificationRequest<T> {}

	public sealed class SpecificationExporter : ExportDescriptorProviderBase
	{
		public static Type Definition { get; } = typeof(SpecificationRequest<>);

		public static SpecificationExporter Default { get; } = new SpecificationExporter();
		SpecificationExporter() {}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			var adapter = contract.ContractType.Adapt();
			if ( adapter.IsGenericOf( Definition ) )
			{
				var inner = adapter.GetInnerType();
				CompositionDependency dependency;
				var exists = descriptorAccessor.TryResolveOptionalDependency( "Specification Exists Request", contract.ChangeType( inner ), true, out dependency );
				yield return new ExportDescriptorPromise( dependency.Contract, GetType().Name, true, NoDependencies, new Factory( exists ).Get );
			}
		}

		sealed class Factory : FactoryBase
		{
			readonly bool result;

			public Factory( bool result )
			{
				this.result = result;
			}

			protected override object Create( LifetimeContext context, CompositionOperation operation ) => result;
		}
	}
}