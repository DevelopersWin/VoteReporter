using DragonSpark.Extensions;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class CompositeTypeDefinitionProvider : ITypeDefinitionProvider
	{
		readonly IEnumerable<ITypeDefinitionProvider> providers;

		public CompositeTypeDefinitionProvider( IEnumerable<ITypeDefinitionProvider> providers )
		{
			this.providers = providers;
		}

		public virtual TypeInfo GetDefinition( TypeInfo info ) => providers.FirstWhere( x => x.GetDefinition( info ) ) ?? info;
	}
}