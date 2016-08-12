using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DragonSpark.Activation.Sources.Caching;

namespace DragonSpark.Windows.Runtime
{
	public class DomainAssemblySource : FactoryCache<AppDomain, ImmutableArray<Assembly>>
	{
		public static DomainAssemblySource Instance { get; } = new DomainAssemblySource();
		DomainAssemblySource() : this( Specification.Instance.IsSatisfiedBy ) {}

		readonly Func<Assembly, bool> specification;

		DomainAssemblySource( Func<Assembly, bool> specification )
		{
			this.specification = specification;
		}

		/*DomainAssemblySource() : base( Create ) {}

		static Assembly[] Create( AppDomain parameter )
		{
			
		}*/

		/*DomainAssemblySource() : this( AppDomain.CurrentDomain ) {}

		public DomainAssemblySource( [Required]AppDomain domain ) : base( Factory.Instance.Create( domain ) ) {}

		class Factory : Cache<AppDomain, Assembly[]>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override Assembly[] Create( AppDomain parameter )
			{
				
			}
		}*/
		protected override ImmutableArray<Assembly> Create( AppDomain parameter ) => 
			parameter.GetAssemblies().Where( specification ).OrderBy( a => a.GetName().Name ).ToImmutableArray();

		class Specification : GuardedSpecificationBase<Assembly>
		{
			public static Specification Instance { get; } = new Specification();
			Specification() {}

			public override bool IsSatisfiedBy( Assembly parameter ) => 
				parameter.Not<AssemblyBuilder>()
				&& parameter.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
				&& !string.IsNullOrEmpty( parameter.Location );
		}
	}
}