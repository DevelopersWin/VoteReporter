using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
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
		DomainAssemblySource() : this( Specification.DefaultNested.IsSatisfiedBy ) {}

		readonly Func<Assembly, bool> specification;

		DomainAssemblySource( Func<Assembly, bool> specification )
		{
			this.specification = specification;
		}

		protected override ImmutableArray<Assembly> Create( AppDomain parameter ) => 
			parameter.GetAssemblies().Where( specification ).OrderBy( a => a.GetName().Name ).ToImmutableArray();

		sealed class Specification : SpecificationBase<Assembly>
		{
			public static Specification DefaultNested { get; } = new Specification();
			Specification() {}

			public override bool IsSatisfiedBy( Assembly parameter ) => 
				parameter.Not<AssemblyBuilder>()
				&& parameter.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
				&& !string.IsNullOrEmpty( parameter.Location );
		}
	}
}