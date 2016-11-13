using System;
using System.Collections.Generic;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Application
{
	public sealed class ApplicationAssemblyLocator : ParameterizedSourceBase<IEnumerable<Assembly>, Assembly>
	{
		readonly static Func<Assembly, bool> Specification = ApplicationAssemblySpecification.Default.ToDelegate();
		
		public static ApplicationAssemblyLocator Default { get; } = new ApplicationAssemblyLocator();
		ApplicationAssemblyLocator() : this( Specification ) {}

		readonly Func<Assembly, bool> specification;
		
		public ApplicationAssemblyLocator( Func<Assembly, bool> specification )
		{
			this.specification = specification;
		}

		public override Assembly Get( IEnumerable<Assembly> parameter ) => parameter.Only( specification );
	}
}