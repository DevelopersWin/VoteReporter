using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Application
{
	public class ApplicationAssemblyFilter : TransformerBase<IEnumerable<Assembly>>
	{
		readonly Func<Assembly, bool> specification;

		/*public ApplicationAssemblyFilter() : this( ApplicationAssemblies.Default.Get() ) {}*/

		public static ApplicationAssemblyFilter Default { get; } = new ApplicationAssemblyFilter();
		ApplicationAssemblyFilter( /*ImmutableArray<Assembly> assemblies*/ ) : this( ApplicationAssemblySpecification.Default.IsSatisfiedBy ) {}

		ApplicationAssemblyFilter( Func<Assembly, bool> specification )
		{
			this.specification = specification;
		}

		public override IEnumerable<Assembly> Get( IEnumerable<Assembly> parameter ) => parameter.Where( specification );
	}
}