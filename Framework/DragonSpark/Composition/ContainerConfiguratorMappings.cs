using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using DragonSpark.Sources;

namespace DragonSpark.Composition
{
	public class ContainerConfiguratorMappings : ContainerConfigurator
	{
		readonly IItemSource<ExportMapping> source;

		public ContainerConfiguratorMappings( IItemSource<ExportMapping> source )
		{
			this.source = source;
		}

		public override ContainerConfiguration Get( ContainerConfiguration parameter )
		{
			var mappings = source.Get();
			var subjects = mappings.Select( mapping => mapping.Subject );
			var builder = new ConventionBuilder();
			foreach ( var mapping in mappings )
			{
				builder.ForType( mapping.Subject ).Export( conventionBuilder => conventionBuilder.AsContractType( mapping.ExportAs ?? mapping.Subject ) );	
			}
			var result = parameter.WithParts( subjects, builder );
			return result;
		}
	}
}