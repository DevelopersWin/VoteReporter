using DragonSpark.Activation;
using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	public sealed class ServicesExportDescriptorProvider : ExportDescriptorProvider
	{
		public static ServicesExportDescriptorProvider Default { get; } = new ServicesExportDescriptorProvider();
		ServicesExportDescriptorProvider() : this( DefaultServices.Default ) {}

		readonly IActivator activator;

		[UsedImplicitly]
		public ServicesExportDescriptorProvider( IActivator activator )
		{
			this.activator = activator;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			CompositionDependency dependency;
			if ( !descriptorAccessor.TryResolveOptionalDependency( "Existing Request", contract, true, out dependency ) && activator.IsSatisfiedBy( contract.ContractType ) )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, activator.WithParameter( contract.ContractType ).ToSharedDescriptor );
			}
		}
	}
}