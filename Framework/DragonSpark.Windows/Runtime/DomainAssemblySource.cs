using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DragonSpark.Windows.Runtime
{
	public class DomainAssemblySource : AssemblyStoreBase
	{
		public static DomainAssemblySource Instance { get; } = new DomainAssemblySource();
		DomainAssemblySource() : this( AppDomain.CurrentDomain ) {}

		public DomainAssemblySource( [Required]AppDomain domain ) : base( Factory.Instance.Create( domain ) ) {}

		class Factory : FactoryBase<AppDomain, Assembly[]>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override Assembly[] Create( AppDomain parameter )
			{
				var query = from assembly in parameter.GetAssemblies()
						where assembly.Not<AssemblyBuilder>()
								&& assembly.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
								&& !string.IsNullOrEmpty( assembly.Location )
				orderby assembly.GetName().Name
				select assembly;
				var result = query.ToArray();
				return result;
			}
		}
	}
}