using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using DragonSpark.Activation.IoC;
using ExportDescriptorProvider = System.Composition.Hosting.Core.ExportDescriptorProvider;

namespace DragonSpark.Composition
{
	public class InstanceExportDescriptorProvider : ExportDescriptorProvider
	{
		readonly object instance;
		readonly string name;

		public InstanceExportDescriptorProvider( [Required]object instance, string name = null )
		{
			this.instance = instance;
			this.name = name;
		}

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			if ( contract.ContractType.Adapt().IsInstanceOfType( instance ) && contract.ContractName == name )
			{
				new ExportProperties.Instance( instance ).Assign( true );
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, dependencies => ExportDescriptor.Create( ( context, operation ) => instance, NoMetadata ) );
			}
		}
	}

	/*public class HostExportDescriptorProvider : ExportDescriptorProvider
	{
		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			if ( contract.ContractType.Adapt().IsInstanceOfType( instance ) && contract.ContractName == name )
			{
				yield return new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, dependencies => ExportDescriptor.Create( ( context, operation ) => context., NoMetadata ) );
			}
		}
	}*/

	public interface IExportDescriptorProviderRegistry
	{
		void Register( ExportDescriptorProvider provider );
	}

	public class RegisteredExportDescriptorProvider : ExportDescriptorProvider, IExportDescriptorProviderRegistry
	{
		readonly ICollection<ExportDescriptorProvider> providers = new List<ExportDescriptorProvider>();

		public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors( CompositionContract contract, DependencyAccessor descriptorAccessor )
		{
			if ( CanBuildSpecification.Instance.IsSatisfiedBy( contract.ContractType ) )
			{
				contract.ContractType.GetTypeInfo().DeclaredConstructors.Each( info => info.GetParameters() );
			}

			var result = contract.ContractType == typeof(IExportDescriptorProviderRegistry) ?
				new ExportDescriptorPromise( contract, GetType().FullName, true, NoDependencies, dependencies => ExportDescriptor.Create( ( context, operation ) => this, NoMetadata ) ).ToItem()
				:
				providers.SelectMany( provider => provider.GetExportDescriptors( contract, descriptorAccessor ) );
			return result;
		}

		public void Register( ExportDescriptorProvider provider ) => providers.Add( provider );
	}
}