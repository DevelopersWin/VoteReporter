using DragonSpark.Activation;
using PostSharp.Patterns.Contracts;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	/*public static class Assemblies
	{
		public static Assembly[] GetCurrent() => new CurrentApplication().Item.Context.Assemblies;

		public delegate Assembly[] Get();

		public static System.Type[] GetTypes( this Get @this ) => @this().SelectMany( assembly => assembly.DefinedTypes ).AsTypes().ToArray();
	}*/

	/*public class AssemblyHost : ExecutionContextValue<Assembly[]> {}*/

	public class ApplicationAssemblyLocator : FactoryBase<Assembly>, IApplicationAssemblyLocator
	{
		readonly Assembly[] assemblies;

		public ApplicationAssemblyLocator( [Required]Assembly[] assemblies )
		{
			this.assemblies = assemblies;
		}

		public override Assembly Create() => assemblies.SingleOrDefault( assembly => assembly.GetCustomAttribute<ApplicationAttribute>() != null );
	}
}