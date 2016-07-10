using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DragonSpark.Windows.Runtime
{
	public class DomainAssemblySource : Cache<AppDomain, Assembly[]>
	{
		public static DomainAssemblySource Instance { get; } = new DomainAssemblySource();
		DomainAssemblySource() : base( Create ) {}

		static Assembly[] Create( AppDomain parameter )
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
	}
}