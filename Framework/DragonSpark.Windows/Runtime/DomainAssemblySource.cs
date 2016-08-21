using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DragonSpark.Windows.Runtime
{
	public sealed class DomainAssemblySource : FactoryCache<AppDomain, ImmutableArray<Assembly>>
	{
		public static DomainAssemblySource Default { get; } = new DomainAssemblySource();
		DomainAssemblySource() : this( Specification.Default.IsSatisfiedBy ) {}

		readonly Func<Assembly, bool> specification;

		DomainAssemblySource( Func<Assembly, bool> specification )
		{
			this.specification = specification;
		}

		protected override ImmutableArray<Assembly> Create( AppDomain parameter ) => 
			parameter.GetAssemblies().Where( specification ).OrderBy( a => a.GetName().Name ).ToImmutableArray();

		sealed class Specification : SpecificationBase<Assembly>
		{
			public static Specification Default { get; } = new Specification();
			Specification() {}

			public override bool IsSatisfiedBy( Assembly parameter ) => 
				parameter.Not<AssemblyBuilder>()
				&& parameter.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
				&& !string.IsNullOrEmpty( parameter.Location );
		}
	}
}