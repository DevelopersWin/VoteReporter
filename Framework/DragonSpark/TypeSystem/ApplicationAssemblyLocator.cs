using DragonSpark.Activation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class ApplicationAssemblyLocator : FactoryBase<IEnumerable<Assembly>, Assembly>
	{
		public static ApplicationAssemblyLocator Instance { get; } = new ApplicationAssemblyLocator();
		ApplicationAssemblyLocator() {}

		public override Assembly Create( IEnumerable<Assembly> assemblies ) => assemblies.SingleOrDefault( assembly => assembly.IsDefined( typeof( ApplicationAttribute) ) );
	}
}