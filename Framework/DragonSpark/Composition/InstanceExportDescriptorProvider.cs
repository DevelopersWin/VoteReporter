using System.Collections.Generic;
using System.Composition.Hosting.Core;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Composition
{
	public class InstanceExportDescriptorProvider : ExportDescriptorProvider
	{
		readonly object instance;

		public InstanceExportDescriptorProvider( [Required]object instance )
		{
			this.instance = instance;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			if ( contract.ContractType.Adapt().IsInstanceOfType( instance ) )
			{
				yield return new ExportDescriptorPromise( contract, contract.ContractType.FullName, true, NoDependencies, dependencies => ExportDescriptor.Create( ( context, operation ) => instance, NoMetadata ) );
			}
		}
	}
}