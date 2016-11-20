using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Application
{
	public sealed class ApplicationAssemblySelector : ParameterizedSourceBase<IEnumerable<Assembly>, Assembly>
	{
		readonly static Func<Assembly, bool> Specification = ApplicationAssemblySpecification.Default.ToDelegate();
		
		public static ApplicationAssemblySelector Default { get; } = new ApplicationAssemblySelector();
		ApplicationAssemblySelector() : this( Specification ) {}

		readonly Func<Assembly, bool> specification;
		
		public ApplicationAssemblySelector( Func<Assembly, bool> specification )
		{
			this.specification = specification;
		}

		public override Assembly Get( IEnumerable<Assembly> parameter ) => parameter.Only( specification );
	}
}