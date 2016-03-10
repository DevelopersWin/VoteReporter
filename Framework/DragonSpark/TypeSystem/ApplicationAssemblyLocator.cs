using System.Collections.Generic;
using DragonSpark.Activation;
using DragonSpark.Activation.FactoryModel;
using PostSharp.Patterns.Contracts;
using System.Linq;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;

namespace DragonSpark.TypeSystem
{
	public static class Assemblies
	{
		public static Assembly[] Resolve( IEnumerable<Assembly> assemblies ) => assemblies.AnyOr( () => new AssemblyHost().Item ?? Default<Assembly>.Items ).Fixed();

		public static Assembly[] GetCurrent() => Resolve( Services.Locate<Assembly[]>() );

		public delegate Assembly[] Get();
	}

	public class AssemblyHost : ExecutionContextValue<Assembly[]> {}

	public class ApplicationAssemblyLocator : FactoryBase<Assembly>, IApplicationAssemblyLocator
	{
		readonly Assembly[] assemblies;

		public ApplicationAssemblyLocator( [Required]Assembly[] assemblies )
		{
			this.assemblies = assemblies;
		}

		protected override Assembly CreateItem() => assemblies.SingleOrDefault( assembly => assembly.GetCustomAttribute<ApplicationAttribute>() != null );
	}
}